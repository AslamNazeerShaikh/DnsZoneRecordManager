---
name: testing-strategy
description: xunit + Moq + Bogus + FluentValidation + FluentAssertions strategy for DnsZoneRecordManager
version: 2.0.0
author: opencode
tags:
  - testing
  - xunit
  - moq
  - bogus
  - fluentvalidation
  - fluentassertions
capabilities:
  - unit-testing
  - validator-testing
  - test-fixtures
  - coverage-analysis
references:
  - "https://xunit.net/"
  - "https://documentation.help/Moq/"
  - "https://github.com/bchavez/Bogus"
  - "https://docs.fluentvalidation.net/"
  - "https://fluentassertions.com/"
examples:
  - name: "Test project references (pinned, net10.0)"
    description: "Identical stack in both *.Tests projects; server projects reference FluentValidation only"
    code: |
      <ItemGroup>
        <PackageReference Include="Bogus" Version="35.6.5" />
        <PackageReference Include="coverlet.collector" Version="6.0.4" />
        <PackageReference Include="FluentAssertions" Version="8.11.0" />
        <PackageReference Include="FluentValidation" Version="12.1.1" />
        <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.14.1" />
        <PackageReference Include="Moq" Version="4.20.72" />
        <PackageReference Include="xunit" Version="2.9.3" />
        <PackageReference Include="xunit.runner.visualstudio" Version="3.1.4" />
      </ItemGroup>
  - name: "Bogus DNS fixtures"
    description: "Seed-shaped data: nahuexolab.com + 4x NS @ apex + 1x TXT _dmarc; timestamps UTC"
    code: |
      var faker = new Faker<ZoneInput>()
          .CustomInstantiator(f => new ZoneInput(f.Internet.DomainName()))
          .RuleFor(z => z.CreatedUtc, f => f.Date.Past().ToUniversalTime());
      var zones = faker.Generate(5);
      zones.Should().HaveCount(5);
      zones.Should().OnlyContain(z => z.CreatedUtc.Kind == DateTimeKind.Utc);
  - name: "Moq + FluentAssertions"
    description: "Stub lookups/handlers, assert behavior"
    code: |
      var lookup = new Mock<IZoneLookup>();
      lookup.Setup(m => m.Exists("nahuexolab.com")).Returns(true);
      lookup.Object.Exists("nahuexolab.com").Should().BeTrue();
  - name: "Validator test"
    description: "Every §7 rule gets a validator test (NS floor, 10-record ceiling, types, CNAME, duplicates)"
    code: |
      [Fact]
      public void should_reject_empty_zone_name()
      {
          new ZoneInputValidator().Validate(new ZoneInput("")).IsValid.Should().BeFalse();
      }
  - name: "Run + coverage"
    description: "Per-solution runs; coverlet collects without extra config"
    code: |
      dotnet test Solution1/DnsZoneRecordManager.slnx
      dotnet test Solution2/DnsZoneRecordManager.slnx --collect:"XPlat Code Coverage"
