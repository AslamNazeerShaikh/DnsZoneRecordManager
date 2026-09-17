---
name: lint
description: Static analysis for DnsZoneRecordManager (Roslyn analyzers via build + eslint for the client)
category: quality
command: dotnet build
args:
  - name: solution
    description: Solution to analyze
    type: string
    default: Solution1/DnsZoneRecordManager.slnx
    choices: [Solution1/DnsZoneRecordManager.slnx, Solution2/DnsZoneRecordManager.slnx]
  - name: warnings-as-errors
    description: Treat warnings as errors
    type: boolean
    default: true
examples:
  - dotnet build Solution1/DnsZoneRecordManager.slnx -c Release /p:TreatWarningsAsErrors=true
  - dotnet build Solution2/DnsZoneRecordManager.slnx -c Release /p:TreatWarningsAsErrors=true
  - npm run lint --prefix Solution2/src/client
