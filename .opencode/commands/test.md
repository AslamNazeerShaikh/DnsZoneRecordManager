---
name: test
description: Run xunit tests for DnsZoneRecordManager (Moq + Bogus + FluentAssertions + coverlet)
category: test
command: dotnet test
args:
  - name: solution
    description: Solution to test
    type: string
    default: Solution1/DnsZoneRecordManager.slnx
    choices: [Solution1/DnsZoneRecordManager.slnx, Solution2/DnsZoneRecordManager.slnx]
  - name: filter
    description: Test filter expression
    type: string
    default: ""
  - name: coverage
    description: Collect code coverage (coverlet)
    type: boolean
    default: false
examples:
  - dotnet test Solution1/DnsZoneRecordManager.slnx
  - dotnet test Solution2/DnsZoneRecordManager.slnx
  - dotnet test Solution2/DnsZoneRecordManager.slnx --filter "FullyQualifiedName~ToolchainSmokeTests"
  - dotnet test Solution1/DnsZoneRecordManager.slnx --collect:"XPlat Code Coverage"
