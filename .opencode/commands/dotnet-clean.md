---
name: dotnet-clean
description: Clean a DnsZoneRecordManager solution
category: build
command: dotnet clean
args:
  - name: solution
    description: Solution to clean
    type: string
    default: Solution1/DnsZoneRecordManager.slnx
    choices: [Solution1/DnsZoneRecordManager.slnx, Solution2/DnsZoneRecordManager.slnx]
  - name: configuration
    description: Build configuration
    type: string
    default: Debug
    choices: [Debug, Release]
examples:
  - dotnet clean Solution1/DnsZoneRecordManager.slnx
  - dotnet clean Solution2/DnsZoneRecordManager.slnx -c Release
