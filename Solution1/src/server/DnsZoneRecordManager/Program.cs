using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using DnsZoneRecordManager.Data;
using DnsZoneRecordManager.Models;
using DnsZoneRecordManager.Services;
using DnsZoneRecordManager.Validation;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

namespace DnsZoneRecordManager
{
    /// <summary>Application entry point: wires MVC, EF Core In-Memory store, validators, services, and seed data.</summary>
    public class Program
    {
        /// <summary>Entry point (host lifetime only; the pipeline is built by <see cref="BuildApp"/>).</summary>
        /// <param name="args">Command-line arguments.</param>
        /// <returns>A task for the host lifetime.</returns>
        [ExcludeFromCodeCoverage]
        public static async Task Main(string[] args)
        {
            await (await BuildApp(args)).RunAsync();
        }

        /// <summary>Builds the configured application (host setup, DI, seed data, pipeline).</summary>
        /// <param name="args">Command-line arguments.</param>
        /// <returns>The built application (run it with RunAsync).</returns>
        public static async Task<WebApplication> BuildApp(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder
                .Services.AddControllersWithViews()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                });
            builder.Services.AddOpenApi();
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase("DnsZoneDb")
            );
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddScoped<IValidator<DnsZone>, ZoneValidator>();
            builder.Services.AddScoped<IValidator<DnsRecord>, RecordValidator>();
            builder.Services.AddScoped<IZoneService, ZoneService>();
            builder.Services.AddScoped<IRecordService, RecordService>();

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                await DbSeeder.SeedAsync(scope.ServiceProvider.GetRequiredService<AppDbContext>());
            }

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.MapScalarApiReference();
            }

            // Security headers for a browser-rendered app (no CSP: the inline theme script must run before first paint).
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

            app.UseAuthorization();

            app.MapStaticAssets();

            // Attribute routes only (controllers carry explicit [Route]/[HttpGet]/[HttpPost]
            // templates mirroring the old conventional URLs, so ApiExplorer — and therefore
            // the OpenAPI document behind /scalar — sees every action with its HTTP method).
            app.MapControllers();

            return app;
        }
    }
}
