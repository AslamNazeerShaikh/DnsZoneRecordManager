---
name: run
description: Run a DnsZoneRecordManager server (MVC or API) or the Next.js client
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
  - npm run dev --prefix Solution2/src/client
