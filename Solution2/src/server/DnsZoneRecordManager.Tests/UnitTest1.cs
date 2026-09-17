using Bogus;
using FluentAssertions;
using FluentValidation;
using Moq;

namespace DnsZoneRecordManager.Tests
{
    public sealed record ZoneInput(string Name);

    public sealed class ZoneInputValidator : AbstractValidator<ZoneInput>
    {
        public ZoneInputValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(253);
        }
    }

    public interface IZoneLookup
    {
        bool Exists(string name);
    }

    public class ToolchainSmokeTests
    {
        [Fact]
        public void Bogus_generates_zone_names()
        {
            var faker = new Faker<ZoneInput>().CustomInstantiator(f => new ZoneInput(
                f.Internet.DomainName()
            ));

            var zones = faker.Generate(5);

            zones.Should().HaveCount(5);
            zones.Should().OnlyContain(z => !string.IsNullOrWhiteSpace(z.Name));
        }

        [Fact]
        public void FluentValidation_rejects_empty_zone_name()
        {
            var validator = new ZoneInputValidator();

            validator.Validate(new ZoneInput("nahuexolab.com")).IsValid.Should().BeTrue();
            validator.Validate(new ZoneInput("")).IsValid.Should().BeFalse();
        }

        [Fact]
        public void Moq_stubs_zone_lookup()
        {
            var lookup = new Mock<IZoneLookup>();
            lookup.Setup(m => m.Exists("nahuexolab.com")).Returns(true);

            lookup.Object.Exists("nahuexolab.com").Should().BeTrue();
            lookup.Object.Exists("other.example").Should().BeFalse();
        }
    }
}
