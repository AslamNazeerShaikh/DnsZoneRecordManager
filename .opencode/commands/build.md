---
name: build
description: Build both DnsZoneRecordManager solutions (server) and the Next.js client
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
examples:
  - dotnet build Solution1/DnsZoneRecordManager.slnx
  - dotnet build Solution2/DnsZoneRecordManager.slnx -c Release
  - dotnet build Solution2/src/server/DnsZoneRecordManager/DnsZoneRecordManager.csproj --no-restore
  - npm run build --prefix Solution2/src/client
