---
name: dotnet-build
description: Build a DnsZoneRecordManager solution (net10.0)
category: build
command: dotnet build
args:
  - name: solution
    description: Solution to build
    type: string
    default: Solution1/DnsZoneRecordManager.slnx
    choices: [Solution1/DnsZoneRecordManager.slnx, Solution2/DnsZoneRecordManager.slnx]
  - name: configuration
    description: Build configuration
    type: string
    default: Debug
    choices: [Debug, Release]
  - name: no-restore
    description: Skip implicit restore
    type: boolean
    default: false
  - name: verbosity
    description: Verbosity level
    type: string
    default: minimal
    choices: [quiet, minimal, normal, detailed, diagnostic]
examples:
  - dotnet build Solution1/DnsZoneRecordManager.slnx
  - dotnet build Solution2/DnsZoneRecordManager.slnx -c Release
  - dotnet build Solution1/DnsZoneRecordManager.slnx --no-restore -v normal
