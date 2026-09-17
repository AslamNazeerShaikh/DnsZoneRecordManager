---
name: dotnet-lint
description: Run Roslyn static analysis on a DnsZoneRecordManager solution via build
category: lint
command: dotnet build
args:
  - name: solution
    description: Solution to analyze
    type: string
    default: Solution1/DnsZoneRecordManager.slnx
    choices: [Solution1/DnsZoneRecordManager.slnx, Solution2/DnsZoneRecordManager.slnx]
  - name: configuration
    description: Build configuration
    type: string
    default: Release
    choices: [Debug, Release]
  - name: warnings-as-errors
    description: Treat warnings as errors
    type: boolean
    default: true
examples:
  - dotnet build Solution1/DnsZoneRecordManager.slnx -c Release /p:TreatWarningsAsErrors=true
  - dotnet build Solution2/DnsZoneRecordManager.slnx -c Release /p:TreatWarningsAsErrors=true
