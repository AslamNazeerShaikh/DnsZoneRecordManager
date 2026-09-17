# DNS Zone & Records Manager — Requirements Analysis (accurate, file-by-file)

Source folder: `/Users/aslamshaikh/Downloads/Other` (assessment bundle).
Generated: 2026-09-17. Every file below was opened and read; every hyperlink was extracted from the PDF binary/annotations and live-checked.

## 1. File inventory (all files, no exceptions)

| # | Path | Size | What it is | Read? |
|---|------|------|------------|-------|
| 1 | `#take-home/WebApp-TakeHome_2025.pdf` | 102,452 bytes, 2 pages | The actual assignment. Only normative requirement doc. | Yes — full text + 8 link annotations extracted |
| 2 | `#take-home/zone_sample/nahuexolab.com.dns` | 715 bytes, 23 lines | Azure-exported sample zone file for `nahuexolab.com` | Yes — full content below |
| 3 | `#take-home/zone_sample/Dns_Records_05_24_2024 20_06_01.xlsx` | 10,265 bytes, 1 sheet `Dns Records`, table `DnsRecords` A1:H6 | Same 5 records as the .dns + UI screenshot, in export/grid shape | Yes — parsed via stdlib zip/xml (no Excel needed) |
| 4 | `#take-home/zone_sample/DM_Zone_snapshot.png` | 53,536 bytes | UI screenshot of a "DNS Records" grid showing the same 5 records | Yes — viewed as image |
| 5 | `#take-home/zone_sample.zip` | 52,926 bytes, 3 files | Zip containing exactly files 2–4 above | Yes — `unzip -l` verified |
| 6 | `DS-ENG_take-home.zip` | 196,555 bytes, 7 entries | Outer bundle: PDF + `zone_sample/` + `zone_sample.zip` | Yes — `unzip -l` verified |
| 7 | `#take-home/zone_sample 2/*` (3 files) | byte-identical | Duplicate extraction of `zone_sample.zip` (`diff -r` → IDENTICAL) | Yes — diffed, safe to delete one copy |
| 8 | `.DS_Store` files | — | macOS Finder metadata, ignore | Listed only |

No `.txt`, `.doc/.docx`, or other docs exist. No hyperlinks exist inside the `.dns`, `.xlsx` (workbook flag `HyperlinksChanged=false`, no hyperlink rels), or the `.png` (flat screenshot, no embedded links). All hyperlinks live in the PDF (page 2).

## 2. Assignment verbatim (so nothing is lost)

**Objective:** "Leveraging a modern ASP.Net stack, develop a simple web application that allows users to Query, Create, Modify and Delete DNS Zones and DNS Records. (definitions)"

**Problem:** DNS zone management today is programmatic (customers must integrate with an existing API + build their own client). Many customers are not engineering-savvy and asked for a UI for basic DNS ops: query/modify/delete/add zones and records. UI + UX must be "simple, intuitive, and provide automatic feedback to the user where sensible."

**Task details — stack (original):**
- Languages: ASP.NET (6.x / current-stable), JavaScript, HTML/CSS
- Frameworks/Libraries: optional, at implementor discretion
- Database: In-Memory
- ORM: optional (Entity Framework preferred)

**Task details — UI:**
- Simple, intuitive; "pixel perfection" NOT expected; Bootstrap or custom CSS fine
- Vanilla JS encouraged; jQuery/React/Angular acceptable
- LOB app: focus on CRUD operations + associated HTML elements

**Assumptions (these ARE business rules, numbered for traceability):**
- A1: A zone file may contain single or many DNS records. An "empty" zone still has minimum **(4) NS records**.
- A2: A zone file must not contain more than **(10) total DNS records**.
- A3: Only record types **A, AAAA, CNAME, NS, TXT** are allowed.
- A4: Customers are not DNS experts — the tool must guide them.
- A5: The tool must give feedback and ensure only valid entries are submitted.
- A6: **Duplicate zones and duplicate records are not allowed.**

**Delivery:** zip file + prominent `readme.md` with context + build/run instructions.
**Time box:** max **8 hours over ≤ 5 days** (discuss with hiring manager first if infeasible).
**Grading:** clean, extensible, secure, maintainable code (perf secondary); UI tested with non-technical persona (working DNS knowledge, not expert); code comments reviewed (may also note "would improve with more time"); `readme.md` reviewed (basic markdown expected).

## 3. Hyperlinks in the PDF (all 8, in document order, live-checked 2026-09-17)

PDF annotation order matches document order, so mapping is 1:1:

| Label in PDF | URL (exact from PDF) | Live check |
|--------------|----------------------|------------|
| Definitions → DNS Zones → Overview | `https://en.wikipedia.org/wiki/DNS_zone` | 200 OK |
| Definitions → DNS Zones → Sample Zone File | `http://www.zonefile.org/` | **DEAD (HTTP 500)** — do not depend on it; use the bundled `nahuexolab.com.dns` instead |
| Definitions → DNS Records → Record Types | `https://en.wikipedia.org/wiki/List_of_DNS_record_types` | 200 OK |
| Definitions → DNS Records → Records Explained | `https://ns1.com/resources/dns-records-explained` | **Redirects** → `https://www.ibm.com/think/topics/dns-records` (IBM acquired NS1), 200 OK |
| Learning → How DNS works | `https://howdns.works/` | 200 OK |
| Learning → Domain Name System – Wikipedia | `https://en.wikipedia.org/wiki/Domain_Name_System` | 200 OK |
| Learning → AzureDNS | `https://azure.microsoft.com/en-us/services/dns/` | **Redirects** → `https://azure.microsoft.com/en-us/products/dns/`, 200 OK |
| Learning → DNS Parameters / RFCs | `https://www.iana.org/assignments/dns-parameters/dns-parameters.xhtml` | **Redirects** → `https://www.iana.org/assignments/dns-parameters`, 200 OK |

Net: 5 links work as-is, 3 redirect-or-dead as noted. For the build, the normative references are Wikipedia DNS zone / record types, IANA parameters, and the bundled sample files — not the dead zonefile.org link.

## 4. Sample data (all three samples agree — this is the seed dataset)

### 4a. `nahuexolab.com.dns` (full content)

```dns
; Exported zone file from Azure DNS
;      Zone name: nahuexolab.com
;      Resource Group Name: Forward_Zones
;      Date and time (UTC): 05/24/2024 20:05:30

$TTL 300
$ORIGIN nahuexolab.com.

@ 3600 IN SOA ns1-33.azure-dns.com. azuredns-hostmaster.microsoft.com (
              1 ; serial
              3600 ; refresh
              300 ; retry
              2419200 ; expire
              300 ; minimum
              )

  172800 IN NS ns1-33.azure-dns.com.
  172800 IN NS ns2-33.azure-dns.net.
  172800 IN NS ns3-33.azure-dns.org.
  172800 IN NS ns4-33.azure-dns.info.

_dmarc 3600 IN TXT "v=DMARC1; p=reject; pct=100; rua=mailto:rua@dmarc.microsoft; ruf=mailto:ruf@dmarc.microsoft; fo=1"
```

Notes: SOA + `$TTL`/`$ORIGIN` header lines are Azure export metadata (not counted as manageable records); the 5 manageable records are 4× NS + 1× TXT. This satisfies A1 (≥4 NS) and A2 (≤10).

### 4b. `Dns_Records_05_24_2024 20_06_01.xlsx` (decoded)

Sheet `Dns Records`, table `DnsRecords`, 8 columns × 5 data rows. Author: `Jason Guiberson (ALLYIS INC)`, created 2024-05-24T20:06:01Z. No embedded hyperlinks.

| FQDN | Zone | Record Name | Record Type | TTL | Record Data | Owner Group | Comment |
|------|------|-------------|-------------|-----|-------------|-------------|---------|
| nahuexolab.com | nahuexolab.com | @ | NS | 172800 | ns1-33.azure-dns.com. | | |
| nahuexolab.com | nahuexolab.com | @ | NS | 172800 | ns2-33.azure-dns.net. | | |
| nahuexolab.com | nahuexolab.com | @ | NS | 172800 | ns3-33.azure-dns.org. | | |
| nahuexolab.com | nahuexolab.com | @ | NS | 172800 | ns4-33.azure-dns.info. | | |
| _dmarc.nahuexolab.com | nahuexolab.com | _dmarc | TXT | 3600 | v=DMARC1; p=reject; pct=100; rua=mailto:rua@dmarc.microsoft; ruf=mailto:ruf@dmarc.microsoft; fo=1 | domainsdns@microsoft.com | |

### 4c. `DM_Zone_snapshot.png` (what the image actually shows)

A reference LOB grid (not a build mandate), titled "DNS Records":
- Top bar: zone dropdown (`nahuexolab.com`), `Export` button, `Search…` box, `By Zone` / `Forward` toggles.
- Grid columns: `FQDN | Record Name | Type | TTL | Data | Modified ↓ | Action ▾` — 5 rows matching the .dns/xlsx exactly (4× NS @ apex + 1× TXT `_dmarc`, Modified `5/23/2024 8:31:02 AM`).
- Row affordances: copy icons, per-row `Action` dropdown (implied edit/delete), expander arrows.
- Footer: pagination `1 – 5 of 5 items`, `100 items per page`.
- Takeaway for our UI: zone picker + search + sortable grid + per-row actions + export affordance. Reproduce the workflow, not the pixels.

### 4d. Consistency

All three samples describe the same zone state. Use the 5 records above as the default seed for both builds.

## 5. What we will build (user decision, deviation from brief noted)

Original brief says DB: In-Memory. **User decision: EF Core + SQLite for both builds** (file-backed, still zero-setup, persists across restarts — better for demo + grading than a process-lifetime store). This is an intentional, documented deviation; call it out in each solution's `readme.md`.

Two solutions, both on latest stable (verified `dotnet --list-sdks`: **10.0.300 + 10.0.400**, i.e. .NET 10):

- **Solution A — `DnsManager.Mvc`**: ASP.NET Core MVC (.NET 10), Razor views + Bootstrap, vanilla JS (per brief's encouragement), EF Core + SQLite. Single project, server-rendered CRUD for zones + records.
- **Solution B — `DnsManager.Api` + `dns-manager-web`**: ASP.NET Core Web API (.NET 10, EF Core + SQLite) + client in **React + TypeScript + Next.js (App Router) + Tailwind CSS**, all latest. REST JSON between them; validation mirrored client + server (server authoritative).

## 6. Data model (shared by both solutions)

```text
Zone: Id (int, PK) | Name (unique, e.g. nahuexolab.com) | CreatedUtc | UpdatedUtc
  └─ Records (1..10, ≥4 NS when "empty")
Record: Id (int, PK) | ZoneId (FK, cascade) | Name (e.g. @, _dmarc, www) | Type (A|AAAA|CNAME|NS|TXT) | TTL (int, seconds) | Data (string) | CreatedUtc | UpdatedUtc
  └─ Unique(ZoneId, Name, Type, Data)  → enforces A6
  └─ FQDN is derived (Name=@ → zone apex), not stored
```

Seed: zone `nahuexolab.com` + the 5 records from §4.

## 7. Validation rules (from A1–A6 + DNS common sense; server is authoritative)

1. Zone name: valid hostname/FQDN (letters/digits/hyphens/dots, no leading/trailing hyphen/dot, total ≤253, label ≤63); normalized lowercase, trimmed trailing dot; unique (case-insensitive).
2. Record count per zone: min = 4 NS records still present after any delete/update ("empty" zone floor, A1); max = 10 records total (A2) — reject creates/updates breaching either with a clear message.
3. Record type ∈ {A, AAAA, CNAME, NS, TXT} (A3); per-type `Data` checks: A = IPv4, AAAA = IPv6, CNAME/NS = valid hostname (FQDN, trailing dot optional), TXT = non-empty ≤ 255 chars per string (allow long DMARC-style values as in sample), TTL = positive int (accept common range 60–86400+, preserve sample values 3600/172800).
4. CNAME exclusivity: a name with a CNAME must hold no other record (standard DNS rule; prevents silent breakage).
5. Duplicates (A6): reject duplicate zone names; reject duplicate (Name, Type, Data) within a zone (case-insensitive for names/hostnames).
6. UX (A4/A5): inline errors next to fields + toast/summary on save; disable invalid submits where trivial (e.g. empty name) but always re-validate server-side; confirm destructive deletes; show count meter "n / 10 records, x NS" so the A1/A2 limits are visible before submit.

Ambiguities resolved this way (call out in readmes): SOA/`$TTL` header lines are not user-editable records; "Modified" column = record `UpdatedUtc`; Export = CSV download of filtered grid (screenshot shows the button but no format — CSV is the lazy defensible choice); zone delete cascades its records after confirmation.

## 8. Scope checklist (maps 1:1 to grading)

- Zones: list/search, create, rename, delete (with record-count + confirm).
- Records: list by zone/search/filter-by-type, create, edit, delete — all within A1–A6 rules.
- Feedback: validation messages, success/error toasts, count meter, confirm dialogs.
- Seed data (§4) auto-applied on first run (both solutions).
- `readme.md` per solution: context, stack, how to build/run (dotnet CLI + npm), EF Core + SQLite note, assumptions, what was cut for the 8h box.
- Clean/extensible/secure/maintainable: thin controllers, service/validation layer, parameterized EF queries, no inline SQL, minimal comments noting trade-offs.
- Conventions: UTC timestamps everywhere (code uses `DateTime.UtcNow`, never local time; docs/snapshots dated in UTC); XML doc comments (`///`) on all public APIs.

## 9. Build order (when authorized — doc only for now)

1. Solution A (MVC) first — satisfies the brief literally, fastest to grade.
2. Solution B (API + Next.js) second — reuses the same EF Core model/validation.
3. Zip each with its `readme.md` per delivery clause.

## 10. Open questions for hiring manager (optional, non-blocking; defaults above apply if unanswered)

1. Is EF Core + SQLite acceptable in place of In-Memory? (assumed yes per user direction)
2. Should zone export include SOA/`$TTL` header lines or records-only CSV? (default: records-only CSV)
3. Is the 4-NS floor per-zone or per-"empty"-zone only? (default: enforced per zone at all times)

## 11. Bundle provenance — people, organization, file metadata (forensics, 2026-09-17 UTC)

File-embedded metadata names two people; public records (devselect.com/about, LinkedIn, ZoomInfo, ContactOut, RocketReach — checked 2026-09-17) confirm both at DevSelect, LLC:

| Person | Title | Organization | Location | Evidence |
|---|---|---|---|---|
| Terry (Terrence) Gaughan | Founder / CEO | DevSelect, LLC (founded 1999; 505 Broadway E PMB 376, Seattle, WA 98102; devselect.com; (800) 962-8779) | Seattle, WA | PDF Info dict + XMP `dc:creator` = "Terry Gaughan"; footer "DevSelect Confidential"; devselect.com/about lists him as founder/CEO |
| Jason Guiberson | Vice President, Software Development (2018–present; ex-Microsoft: Groove Music, Movies & TV, MSN apps) | DevSelect, LLC | Kirkland, WA | xlsx `dc:creator`/`lastModifiedBy` = "Jason Guiberson (ALLYIS INC)" — file tag names a vendor employer-of-record, public role is DevSelect VP; email format `{first}@devselect.com` |

### 11a. Filesystem metadata (this machine, IST; uid 501/staff)

| File | Birth | Modified | Size |
|---|---|---|---|
| `Other/` (folder) | 2026-09-17 19:13 IST | 2026-09-17 19:33 IST | — |
| `DS-ENG_take-home.zip` | 2026-09-17 19:13 IST | 2026-09-17 19:13 IST | 196,555 |
| `REQUIREMENTS_ANALYSIS.md` (this doc's twin) | 2026-09-17 19:33 IST | 2026-09-17 19:33 IST | 12,877 (`diff` vs repo copy: IDENTICAL) |
| `WebApp-TakeHome_2025.pdf` | 2024-05-24 02:56 IST | 2024-05-24 02:56 IST | 102,452 |
| `zone_sample.zip` | 2024-05-25 01:38 IST | 2024-05-25 01:38 IST | 52,926 |
| `zone_sample/*` (3 files) | 2024-05-25 01:35–01:37 IST | same | 53,536 / 10,265 / 715 |
| `zone_sample 2/*` (3 files) | 2024-05-24 13:05–13:07 IST | same | byte-identical to `zone_sample/` (`diff -r`) |

Birth = arrival/handling on this machine lineage; content times below are authoritative for authoring.

### 11b. Embedded + archive metadata (authoring side)

| File | Embedded facts |
|---|---|
| PDF | Author "Terry Gaughan" (Info + XMP, no doc title); Creator/Producer "Microsoft® Word for Microsoft 365"; CreationDate = ModDate = 2023-01-26 13:26:12 **-08:00 (PST)**; PDF 1.7, 2 pages; fonts Arial/Calibri/Bierstadt/Courier New/Symbol; DocID `uuid:57A0DB91-…` |
| xlsx | creator = lastModifiedBy = "Jason Guiberson (ALLYIS INC)"; created 2024-05-24T20:06:01Z, modified 20:07:16Z; Excel 16.0300; 1 sheet; MIP sensitivity label `{f42aa342-…}` (Microsoft tenant); no `Company` field |
| PNG | 1485×892, 8-bit RGBA, **zero** text/EXIF chunks — no author, software, timestamp, or GPS |
| `zone_sample.zip` | **made-by: Windows** (MS-DOS/FAT); entries 2024-05-24 13:05–13:08 (author-local) |
| `DS-ENG_take-home.zip` | **made-by: Unix** (macOS-style); assembled 2024-05-23 14:26 (author-local); 7 entries |

Timezone cross-check (authoring = US Pacific): xlsx stamp is UTC (20:06Z) while the same moment in zip/file times reads 13:06 → PDT (UTC-7); PDF stamps -08:00 (PST, January). Receipt here (IST) lands May 24 ~02:56 IST = May 23 14:26 PDT — outer-zip assembly to the minute.

### 11c. Emails found in bundle

`domainsdns@microsoft.com` (xlsx Owner Group — a distribution list, plus MIP label confirms a Microsoft tenant); `rua@dmarc.microsoft` / `ruf@dmarc.microsoft` (DMARC sample data, not people). No other personal emails in PDF/`.dns`/xlsx.

### 11d. Explicitly absent (checked, not present)

No IPs, no GPS/location, no device IDs, no xlsx revision history, no PDF title, no PNG metadata of any kind. `.DS_Store` files are Finder window state — nothing authorial, not parsed.
