using Bogus;
using DnsZoneRecordManager.Data;
using DnsZoneRecordManager.Models;
using DnsZoneRecordManager.Services;
using DnsZoneRecordManager.Validation;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace DnsZoneRecordManager.Tests
{
    /// <summary>Shared InMemory unit-of-work factory: one isolated database per test.</summary>
    public static class TestUow
    {
        /// <summary>Creates a unit of work over a fresh InMemory database.</summary>
        /// <returns>Isolated unit of work.</returns>
        public static IUnitOfWork New()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase("UnitTestDb-" + Guid.NewGuid())
                .Options;
            return new UnitOfWork(new AppDbContext(options));
        }

        /// <summary>Seeds one zone with 4 NS records plus the given extra records.</summary>
        /// <param name="uow">Unit of work.</param>
        /// <param name="zoneName">Zone name.</param>
        /// <param name="extras">Extra records to add.</param>
        /// <returns>The zone id.</returns>
        public static async Task<int> SeedZoneAsync(
            IUnitOfWork uow,
            string zoneName,
            params DnsRecord[] extras
        )
        {
            var now = DateTime.UtcNow;
            var zone = new DnsZone
            {
                Name = zoneName,
                CreatedUtc = now,
                UpdatedUtc = now,
            };
            await uow.Zones.AddAsync(zone);
            await uow.SaveAsync();
            foreach (var i in Enumerable.Range(1, 4))
            {
                await uow.Records.AddAsync(
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
                await uow.Records.AddAsync(extra);
            }

            await uow.SaveAsync();
            return zone.Id;
        }
    }

    /// <summary>Validator tests: every shape rule plus all per-type data arms.</summary>
    public class ValidatorsTests
    {
        private static DnsRecord Record(RecordType type, string data)
        {
            return new DnsRecord
            {
                Name = "www",
                Type = type,
                Ttl = 300,
                Data = data,
            };
        }

        [Fact]
        public void should_accept_sample_zone_name()
        {
            new ZoneValidator()
                .Validate(new DnsZone { Name = "nahuexolab.com" })
                .IsValid.Should()
                .BeTrue();
        }

        [Fact]
        public void should_reject_empty_long_and_malformed_zone_names()
        {
            var validator = new ZoneValidator();
            validator.Validate(new DnsZone { Name = "" }).IsValid.Should().BeFalse();
            validator
                .Validate(new DnsZone { Name = new string('a', 250) + ".com" })
                .IsValid.Should()
                .BeFalse();
            validator.Validate(new DnsZone { Name = "-bad.example" }).IsValid.Should().BeFalse();
            validator.Validate(new DnsZone { Name = "bad_.example" }).IsValid.Should().BeFalse();
        }

        [Fact]
        public void should_accept_seed_shaped_records()
        {
            var validator = new RecordValidator();
            validator
                .Validate(
                    new DnsRecord
                    {
                        Name = "@",
                        Type = RecordType.NS,
                        Ttl = 172800,
                        Data = "ns1-33.azure-dns.com.",
                    }
                )
                .IsValid.Should()
                .BeTrue();
            validator
                .Validate(
                    new DnsRecord
                    {
                        Name = "_dmarc",
                        Type = RecordType.TXT,
                        Ttl = 3600,
                        Data = "v=DMARC1; p=reject",
                    }
                )
                .IsValid.Should()
                .BeTrue();
        }

        [Fact]
        public void should_reject_bad_names_types_ttls_and_data()
        {
            var validator = new RecordValidator();
            validator.Validate(Record(RecordType.A, "")).IsValid.Should().BeFalse();
            validator
                .Validate(
                    new DnsRecord
                    {
                        Name = "bad name!",
                        Type = RecordType.A,
                        Ttl = 300,
                        Data = "1.2.3.4",
                    }
                )
                .IsValid.Should()
                .BeFalse();
            validator
                .Validate(
                    new DnsRecord
                    {
                        Name = "www",
                        Type = (RecordType)99,
                        Ttl = 300,
                        Data = "1.2.3.4",
                    }
                )
                .IsValid.Should()
                .BeFalse();
            validator.Validate(Record(RecordType.A, "999.1.1.1")).IsValid.Should().BeFalse();
            validator.Validate(Record(RecordType.A, "not-an-ip")).IsValid.Should().BeFalse();
            validator.Validate(Record(RecordType.AAAA, "1.2.3.4")).IsValid.Should().BeFalse();
            validator.Validate(Record(RecordType.CNAME, "not a host!")).IsValid.Should().BeFalse();
            validator.Validate(Record(RecordType.NS, "")).IsValid.Should().BeFalse();
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
        }

        [Fact]
        public void should_generate_valid_fixtures_with_bogus()
        {
            var faker = new Faker<DnsRecord>()
                .RuleFor(r => r.Name, f => f.Internet.DomainName().Split('.')[0])
                .RuleFor(r => r.Type, RecordType.A)
                .RuleFor(r => r.Ttl, 3600)
                .RuleFor(r => r.Data, f => f.Internet.Ip());
            var record = faker.Generate();
            new RecordValidator().Validate(record).IsValid.Should().BeTrue();
        }
    }
}
