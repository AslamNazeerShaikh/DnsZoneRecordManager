---
name: dotnet-publish
description: Publish a DnsZoneRecordManager server project (Release, net10.0)
category: build
command: dotnet publish
args:
  - name: project
    description: Server project to publish
    type: string
    default: Solution1/src/server/DnsZoneRecordManager/DnsZoneRecordManager.csproj
    choices: [Solution1/src/server/DnsZoneRecordManager/DnsZoneRecordManager.csproj, Solution2/src/server/DnsZoneRecordManager/DnsZoneRecordManager.csproj]
  - name: configuration
    description: Build configuration
    type: string
    default: Release
    choices: [Debug, Release]
  - name: runtime-identifier
    description: Runtime identifier (empty = portable)
    type: string
    default: ""
  - name: output
    description: Output directory
    type: string
    default: ""
examples:
  - dotnet publish Solution1/src/server/DnsZoneRecordManager -c Release
  - dotnet publish Solution2/src/server/DnsZoneRecordManager -c Release
  - dotnet publish Solution2/src/server/DnsZoneRecordManager -c Release -r osx-arm64
