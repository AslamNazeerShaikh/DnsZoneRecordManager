---
name: dotnet-run
description: Run a DnsZoneRecordManager server project (Solution1 MVC or Solution2 API)
category: run
command: dotnet run
args:
  - name: project
    description: Server project to run
    type: string
    default: Solution1/src/server/DnsZoneRecordManager/DnsZoneRecordManager.csproj
    choices: [Solution1/src/server/DnsZoneRecordManager/DnsZoneRecordManager.csproj, Solution2/src/server/DnsZoneRecordManager/DnsZoneRecordManager.csproj]
  - name: configuration
    description: Build configuration
    type: string
    default: Debug
    choices: [Debug, Release]
  - name: launch-profile
    description: launchSettings.json profile
    type: string
    default: http
examples:
  - dotnet run --project Solution1/src/server/DnsZoneRecordManager
  - dotnet run --project Solution2/src/server/DnsZoneRecordManager
  - dotnet run --project Solution2/src/server/DnsZoneRecordManager -c Release
