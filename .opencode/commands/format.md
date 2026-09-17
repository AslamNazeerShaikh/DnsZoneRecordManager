---
name: format
description: Format DnsZoneRecordManager code (CSharpier for C#, Prettier for the Next.js client)
category: quality
command: dotnet csharpier
args:
  - name: check
    description: Only check formatting, don't modify
    type: boolean
    default: false
examples:
  - dotnet csharpier format .
  - dotnet csharpier check .
  - npm run format --prefix Solution2/src/client
  - npm run format:check --prefix Solution2/src/client
