---
name: dotnet-test
description: Run xunit tests for a DnsZoneRecordManager solution (net10.0)
category: test
command: dotnet test
args:
  - name: solution
    description: Solution to test
    type: string
    default: Solution1/DnsZoneRecordManager.slnx
    choices: [Solution1/DnsZoneRecordManager.slnx, Solution2/DnsZoneRecordManager.slnx]
  - name: configuration
    description: Build configuration
    type: string
    default: Debug
    choices: [Debug, Release]
  - name: filter
    description: Test filter expression
    type: string
    default: ""
  - name: collect-coverage
    description: Collect code coverage
    type: boolean
    default: false
  - name: verbosity
    description: Verbosity level
    type: string
    default: normal
    choices: [quiet, minimal, normal, detailed, diagnostic]
examples:
  - dotnet test Solution1/DnsZoneRecordManager.slnx
  - dotnet test Solution2/DnsZoneRecordManager.slnx -c Release
  - dotnet test Solution1/DnsZoneRecordManager.slnx --filter "FullyQualifiedName~ToolchainSmokeTests"
  - dotnet test Solution2/DnsZoneRecordManager.slnx --collect:"XPlat Code Coverage"
  - dotnet test Solution1/DnsZoneRecordManager.slnx --logger "console;verbosity=detailed"
