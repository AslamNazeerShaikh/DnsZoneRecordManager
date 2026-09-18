using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.Json.Serialization;
using DnsZoneRecordManager.Cqrs;
using DnsZoneRecordManager.Cqrs.Records;
using DnsZoneRecordManager.Cqrs.Zones;
using DnsZoneRecordManager.Data;
using DnsZoneRecordManager.Validation;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Serilog;

namespace DnsZoneRecordManager
{
    /// <summary>Application entry point: structured logging, SQLite, hand-rolled CQRS, Scalar reference.</summary>
    public class Program
    {
        /// <summary>Entry point (host lifetime only; the pipeline is built by <see cref="BuildApp"/>).</summary>
        /// <param name="args">Command-line arguments.</param>
        /// <returns>Process exit code (0 ok, 1 fatal startup failure).</returns>
        [ExcludeFromCodeCoverage]
        public static async Task<int> Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateBootstrapLogger();
            try
            {
                Log.Information("Starting DNS Zone Record Manager API");
                await (await BuildApp(args)).RunAsync();
                return 0;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application terminated unexpectedly");
                return 1;
            }
            finally
            {
                await Log.CloseAndFlushAsync();
            }
        }

        /// <summary>Builds the configured application (logging, data, CQRS, pipeline, seed).</summary>
        /// <param name="args">Command-line arguments.</param>
        /// <returns>The built application (run it with RunAsync).</returns>
        public static async Task<WebApplication> BuildApp(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Host.UseSerilog(
                (context, services, configuration) =>
                    configuration.ReadFrom.Configuration(context.Configuration).ReadFrom.Services(services).Enrich.FromLogContext(),
                preserveStaticLogger: true
            );

            builder.Services.AddControllers().AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
            builder.Services.AddProblemDetails();
            builder.Services.AddOpenApi();
            builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
            builder.Services.AddCqrsHandlers(Assembly.GetExecutingAssembly());
            builder.Services.AddScoped<IValidator<CreateZoneCommand>, CreateZoneValidator>();
            builder.Services.AddScoped<IValidator<RenameZoneCommand>, RenameZoneValidator>();
            builder.Services.AddScoped<IValidator<CreateRecordCommand>, CreateRecordValidator>();
            builder.Services.AddScoped<IValidator<UpdateRecordCommand>, UpdateRecordValidator>();
            builder.Services.AddCors(options =>
                options.AddPolicy(
                    "Client",
                    policy =>
                        policy
                            .WithOrigins(builder.Configuration.GetSection("ClientOrigins").GetChildren().Select(c => c.Value!).ToArray())
                            .AllowAnyHeader()
                            .AllowAnyMethod()
                )
            );

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                await context.Database.MigrateAsync();
                await DbSeeder.SeedAsync(context, CancellationToken.None);
            }

            app.UseExceptionHandler();

            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.MapScalarApiReference();
            }
            else
            {
                app.UseHsts();
            }

            app.UseSerilogRequestLogging();

            // Security headers for API consumers (no CSP: Scalar serves its own scripts).
            app.Use(
                async (context, next) =>
                {
                    context.Response.Headers.XContentTypeOptions = "nosniff";
                    context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
                    context.Response.Headers.XFrameOptions = "SAMEORIGIN";
                    await next();
                }
            );

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseCors("Client");
            app.MapControllers();

            return app;
        }
    }
}
