using DnsZoneRecordManager.Cqrs.Records;
using DnsZoneRecordManager.Cqrs.Zones;
using DnsZoneRecordManager.Data;
using DnsZoneRecordManager.Models;
using DnsZoneRecordManager.Results;
using DnsZoneRecordManager.Validation;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace DnsZoneRecordManager.Tests
{
    /// <summary>Isolated SQLite stores (one open in-memory connection per context).</summary>
    public static class TestDb
    {
        /// <summary>Creates a fresh database (schema ensured, shared open connection).</summary>
        /// <returns>Context plus its connection (dispose both).</returns>
        public static (AppDbContext Context, SqliteConnection Connection) New()
        {
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();
            var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(connection).Options;
            var context = new AppDbContext(options);
            context.Database.EnsureCreated();
            return (context, connection);
        }

        /// <summary>Seeds one zone with 4 NS records plus extras.</summary>
        /// <param name="context">Context.</param>
        /// <param name="zoneName">Zone name.</param>
        /// <param name="extras">Extra records.</param>
        /// <returns>Zone id.</returns>
        public static async Task<int> SeedZoneAsync(AppDbContext context, string zoneName, params DnsRecord[] extras)
        {
            var now = DateTime.UtcNow;
            var zone = new DnsZone { Name = zoneName, CreatedUtc = now, UpdatedUtc = now };
            await context.Zones.AddAsync(zone);
            await context.SaveChangesAsync();
            foreach (var i in Enumerable.Range(1, 4))
            {
                await context.Records.AddAsync(
                    new DnsRecord
                    {
                        ZoneId = zone.Id,
                        Name = "@",
                        Type = RecordType.NS,
                        Ttl = 172800,
                        Data = $"ns{i}.{zoneName}.",
                        CreatedUtc = now,
                        UpdatedUtc = now,
                    }
                );
            }

            foreach (var extra in extras)
            {
                extra.ZoneId = zone.Id;
                extra.CreatedUtc = now;
                extra.UpdatedUtc = now;
                await context.Records.AddAsync(extra);
            }

            await context.SaveChangesAsync();
            return zone.Id;
        }
    }

    /// <summary>Command validator tests: every shape rule plus all per-type data arms.</summary>
    public class ValidatorsTests
    {
        [Fact]
        public void should_accept_sample_zone_commands()
        {
            new CreateZoneValidator().Validate(new CreateZoneCommand("nahuexolab.com")).IsValid.Should().BeTrue();
            new RenameZoneValidator().Validate(new RenameZoneCommand(1, "nahuexolab.com")).IsValid.Should().BeTrue();
        }

        [Fact]
        public void should_reject_empty_long_and_malformed_zone_names()
        {
            var create = new CreateZoneValidator();
            create.Validate(new CreateZoneCommand("")).IsValid.Should().BeFalse();
            create.Validate(new CreateZoneCommand(new string('a', 250) + ".com")).IsValid.Should().BeFalse();
            create.Validate(new CreateZoneCommand("-bad.example")).IsValid.Should().BeFalse();
            new RenameZoneValidator().Validate(new RenameZoneCommand(1, "bad_.example")).IsValid.Should().BeFalse();
        }

        [Fact]
        public void should_accept_seed_shaped_record_commands()
        {
            new CreateRecordValidator().Validate(new CreateRecordCommand(1, "@", RecordType.NS, 172800, "ns1-33.azure-dns.com.")).IsValid.Should().BeTrue();
            new UpdateRecordValidator().Validate(new UpdateRecordCommand(9, "_dmarc", RecordType.TXT, 3600, "v=DMARC1")).IsValid.Should().BeTrue();
        }

        [Fact]
        public void should_reject_bad_record_commands()
        {
            var create = new CreateRecordValidator();
            create.Validate(new CreateRecordCommand(1, "", RecordType.A, 300, "10.0.0.1")).IsValid.Should().BeFalse();
            create.Validate(new CreateRecordCommand(1, "bad name!", RecordType.A, 300, "10.0.0.1")).IsValid.Should().BeFalse();
            create.Validate(new CreateRecordCommand(1, "www", (RecordType)99, 300, "10.0.0.1")).IsValid.Should().BeFalse();
            create.Validate(new CreateRecordCommand(1, "www", RecordType.A, 0, "10.0.0.1")).IsValid.Should().BeFalse();
            create.Validate(new CreateRecordCommand(1, "www", RecordType.A, 300, "999.1.1.1")).IsValid.Should().BeFalse();
            create.Validate(new CreateRecordCommand(1, "www", RecordType.AAAA, 300, "1.2.3.4")).IsValid.Should().BeFalse();
            create.Validate(new CreateRecordCommand(1, "alias", RecordType.CNAME, 300, "not a host!")).IsValid.Should().BeFalse();
            new UpdateRecordValidator().Validate(new UpdateRecordCommand(1, "www", RecordType.A, 300, "")).IsValid.Should().BeFalse();
        }

        [Fact]
        public void should_validate_all_per_type_data_arms()
        {
            RecordDataValidators.IsValid(RecordType.A, "10.0.0.1").Should().BeTrue();
            RecordDataValidators.IsValid(RecordType.AAAA, "2001:db8::1").Should().BeTrue();
            RecordDataValidators.IsValid(RecordType.AAAA, "not-an-ip").Should().BeFalse();
            RecordDataValidators.IsValid(RecordType.CNAME, "alias.example.com.").Should().BeTrue();
            RecordDataValidators.IsValid(RecordType.NS, "ns1.example.com").Should().BeTrue();
            RecordDataValidators.IsValid(RecordType.TXT, "hello").Should().BeTrue();
            RecordDataValidators.IsValid(RecordType.TXT, "").Should().BeFalse();
            RecordDataValidators.IsValid((RecordType)99, "x").Should().BeFalse();
            DnsRules.IsValidHostname("bad_host!").Should().BeFalse();
            DnsRules.NormalizeZoneName("  NAHUEXOLAB.COM. ").Should().Be("nahuexolab.com");
            DnsRules.NormalizeRecordName("  WWW ").Should().Be("www");
            DnsRules.ToFqdn("@", "svc.example").Should().Be("svc.example");
            DnsRules.ToFqdn("www", "svc.example").Should().Be("www.svc.example");
        }

        [Fact]
        public void should_generate_valid_fixtures_with_bogus()
        {
            var faker = new Bogus.Faker<CreateRecordCommand>()
                .CustomInstantiator(f => new CreateRecordCommand(1, f.Internet.DomainName().Split('.')[0], RecordType.A, 3600, f.Internet.Ip()));
            var command = faker.Generate();
            new CreateRecordValidator().Validate(command).IsValid.Should().BeTrue();
        }
    }

    /// <summary>Result pattern member tests (equality, factories, kind checks).</summary>
    public class ResultTypesTests
    {
        [Fact]
        public void should_compare_service_errors_by_value()
        {
            var a = new ServiceError(ErrorKind.Conflict, "dup");
            var b = new ServiceError(ErrorKind.Conflict, "dup");
            (a == b).Should().BeTrue();
            (a != new ServiceError(ErrorKind.NotFound, "missing")).Should().BeTrue();
            a.GetHashCode().Should().Be(b.GetHashCode());
            a.ToString().Should().Contain("Conflict");
            var (kind, message) = a;
            kind.Should().Be(ErrorKind.Conflict);
            message.Should().Be("dup");
            a.Should().Be(b with { Message = "dup" });
        }

        [Fact]
        public void should_build_success_and_typed_failures()
        {
            var ok = ServiceResult<int>.Ok(7);
            ok.Success.Should().BeTrue();
            ok.Data.Should().Be(7);
            ok.HasError(ErrorKind.NotFound).Should().BeFalse();

            var missing = ServiceResult<int>.Fail(ErrorKind.NotFound, "gone");
            missing.Success.Should().BeFalse();
            missing.HasError(ErrorKind.NotFound).Should().BeTrue();
            missing.Errors.Should().ContainSingle().Which.Message.Should().Be("gone");

            var multi = ServiceResult<int>.Fail(ErrorKind.Validation, new[] { "a", "b" });
            multi.Errors.Should().HaveCount(2);

            var typed = ServiceResult<int>.Fail(new[] { new ServiceError(ErrorKind.RuleViolation, "rule") });
            typed.HasError(ErrorKind.RuleViolation).Should().BeTrue();
        }
    }
}
