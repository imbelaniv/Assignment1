# EventEase Venue Booking System — Implementation Plan

## Overview

ASP.NET Core MVC application for EventEase, an event management company. Built in three parts (POE Parts 1–3), each submitted separately. The admin-only platform lets booking specialists manage venues, events, and bookings on behalf of customers.

---

## Current State

Vanilla ASP.NET Core MVC scaffold — only `HomeController`, default views, and no models beyond `ErrorViewModel`. Everything below needs to be built from scratch.

---

## Part 1 — Project Foundation and Initial Deployment

### A. Database Design

**Three required tables:**

```sql
-- Venue
VenueId       INT           PRIMARY KEY IDENTITY
VenueName     NVARCHAR(255) NOT NULL
Location      NVARCHAR(255) NOT NULL
Capacity      INT           NOT NULL
ImageUrl      NVARCHAR(500) NULL   -- placeholder URL in Part 1

-- Event
EventId       INT           PRIMARY KEY IDENTITY
EventName     NVARCHAR(255) NOT NULL
EventDate     DATETIME      NOT NULL
Description   NVARCHAR(MAX) NULL
VenueId       INT           NULL   FK -> Venue(VenueId)
              -- nullable: events can be loaded before a venue is assigned

-- Booking
BookingId     INT           PRIMARY KEY IDENTITY
EventId       INT           NOT NULL  FK -> Event(EventId)
VenueId       INT           NOT NULL  FK -> Venue(VenueId)
BookingDate   DATETIME      NOT NULL
```

**Deliverables:**
- ERD diagram (provided as PDF — `Database Design and ERD.pdf` already in repo)
- SQL script: table creation + seed data inserts + constraints (PK, FK, NOT NULL)

### B. Web Application

**Models** (`WebApplication1/Models/`):
- `Venue.cs` — maps to Venue table
- `Event.cs` — maps to Event table; includes nullable `VenueId` FK + navigation property
- `Booking.cs` — maps to Booking table; includes FK navigation to both Venue and Event

**DbContext** (`WebApplication1/Data/AppDbContext.cs`):
- Inherits `DbContext`
- `DbSet<Venue>`, `DbSet<Event>`, `DbSet<Booking>`
- Connection string read from `appsettings.json` (placeholder for now, replaced with Azure SQL in deployment)

**Connection string placeholder in `appsettings.json`:**
```json
"ConnectionStrings": {
  "DefaultConnection": "YOUR_CONNECTION_STRING_HERE"
}
```

**Controllers** (`WebApplication1/Controllers/`):
- `VenuesController.cs` — full CRUD (Index, Details, Create, Edit, Delete)
- `EventsController.cs` — full CRUD
- `BookingsController.cs` — full CRUD

**Views** (`WebApplication1/Views/`):
- `Venues/` — Index, Details, Create, Edit, Delete
- `Events/` — Index, Details, Create, Edit, Delete
- `Bookings/` — Index, Details, Create, Edit, Delete

**Image handling (Part 1 only):** Store a plain URL string in `ImageUrl` on Venue. Use any publicly accessible placeholder image URL (e.g., `https://placehold.co/400x300`). No file upload yet.

**Navigation:** Update `_Layout.cshtml` to include nav links for Venues, Events, Bookings.

**NuGet packages required:**
- `Microsoft.EntityFrameworkCore.SqlServer`
- `Microsoft.EntityFrameworkCore.Tools`
- `Microsoft.EntityFrameworkCore.Design`

**EF Migrations:**
```
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### C. Azure Deployment

1. **Azure Web App Service** — create via Azure Portal, deploy from Visual Studio publish profile
2. **Azure SQL Database** — create server + database, run migration script or use EF migrate on startup
3. Update `appsettings.json` (or App Service environment variables) with real Azure SQL connection string
4. Submit deployed URL in format: `STxxx.azurewebsites.net`

**Screenshots required:**
- Azure Portal showing Web App Service created
- Visual Studio successful deployment message
- Azure Portal showing SQL Database created
- Azure Query Editor showing tables and migrated data

### D. Theory Questions (in Word document)

1. **Cloud vs on-premises deployment** — cover security, deployment speed, resource management with examples
2. **IaaS vs PaaS vs SaaS** — definitions, differences, why PaaS suits EventEase

---

## Part 2 — Enhancing Functionality and Integrating Cloud Storage

### A. Azure Blob Storage Integration

Replace the placeholder `ImageUrl` string with actual file upload to Azure Blob Storage.

**NuGet package:** `Azure.Storage.Blobs`

**Blob config placeholder in `appsettings.json`:**
```json
"AzureStorage": {
  "ConnectionString": "YOUR_BLOB_STORAGE_CONNECTION_STRING_HERE",
  "ContainerName": "venue-images"
}
```

**Changes:**
- Add `IFormFile ImageFile` property (not mapped) to `Venue` model for upload
- In `VenuesController` Create/Edit: upload file to blob, save returned URL into `ImageUrl`
- Display images in views using the stored URL
- On Delete: remove blob from storage as well as the DB record

### B. Error Handling and Validation

**Double-booking prevention:**
- In `BookingsController.Create` (POST): before saving, query DB to check if a booking already exists for the same `VenueId` + same date. If conflict found, add a `ModelState` error and re-display the form with an alert.

**Restrict deletion of venues/events with active bookings:**
- In `VenuesController.Delete` (POST): check `Bookings.Any(b => b.VenueId == id)`. If true, do not delete — display TempData alert instead.
- In `EventsController.Delete` (POST): same check against `EventId`.

**Required field validation:**
- Add `[Required]`, `[StringLength]`, `[Range]` data annotations to all models
- Views already include `_ValidationScriptsPartial` — ensure it is rendered on Create/Edit views for client-side validation
- Wrap all controller actions in try/catch; return friendly error views on unexpected failures

**Alert display:**
- Use `TempData["ErrorMessage"]` / `TempData["SuccessMessage"]` pattern
- Render TempData alerts in `_Layout.cshtml` so they appear on any page

### C. Enhanced Booking Display and Search

**Database View (SQL):**
```sql
CREATE VIEW vw_BookingDetails AS
SELECT
    b.BookingId,
    b.BookingDate,
    e.EventId,
    e.EventName,
    e.EventDate,
    e.Description,
    v.VenueId,
    v.VenueName,
    v.Location,
    v.Capacity,
    v.ImageUrl
FROM Booking b
JOIN Event   e ON b.EventId  = e.EventId
JOIN Venue   v ON b.VenueId  = v.VenueId;
```

**ViewModel** (`Models/BookingDetailsViewModel.cs`) — mirrors the view columns.

**Bookings Index** — replace the plain Booking list with this consolidated view. Show venue name, event name, date, location, capacity.

**Search** — add a search bar to the Bookings Index:
- Filter by `BookingId` (exact match) OR `EventName` (contains, case-insensitive)
- Pass search term via query string; controller filters the EF query accordingly

### D. Azure Deployment Updates

- Redeploy updated app to same Azure Web App Service
- Run the `CREATE VIEW` script against Azure SQL Database via Query Editor
- Screenshot: Query Editor showing the new view

**Screenshots required:**
- Updated deployed app URL showing new features
- Azure Query Editor showing `vw_BookingDetails` view exists

### E. Theory Questions (in Word document)

1. **Azure Cognitive Search vs traditional search engines** — differences, use cases, limitations, mitigations
2. **Database normalization in cloud-based design** — importance, normalized vs denormalized performance/scalability in Azure

---

## Part 3 — Advanced Filtering, Reporting, and Documentation

### A. Advanced Filtering

**New table — EventType lookup:**
```sql
EventTypeId   INT           PRIMARY KEY IDENTITY
TypeName      NVARCHAR(100) NOT NULL
-- Seed data: Conference, Wedding, Concert, Corporate, Exhibition, Other
```

**Update Event table:**
```sql
ALTER TABLE Event ADD EventTypeId INT NULL REFERENCES EventType(EventTypeId);
```

**Update Venue table:**
```sql
ALTER TABLE Venue ADD IsAvailable BIT NOT NULL DEFAULT 1;
```

**Model updates:**
- Add `EventType.cs` model
- Add `EventTypeId` FK + navigation property to `Event.cs`
- Add `IsAvailable` bool property to `Venue.cs`

**Filter UI on Bookings Index (extends Part 2 search):**
- Dropdown: filter by EventType (populated from `EventType` table)
- Date range pickers: `DateFrom` / `DateTo` — filters on `Event.EventDate`
- Dropdown/checkbox: Venue Availability (`Venue.IsAvailable`)
- All filters combinable; pass as query string params; controller applies each conditionally

**Controller logic sketch:**
```csharp
var query = _context.vw_BookingDetails.AsQueryable();
if (eventTypeId.HasValue)    query = query.Where(b => b.EventTypeId == eventTypeId);
if (dateFrom.HasValue)       query = query.Where(b => b.EventDate >= dateFrom);
if (dateTo.HasValue)         query = query.Where(b => b.EventDate <= dateTo);
if (availableOnly == true)   query = query.Where(b => b.IsAvailable);
```

### B. Azure Deployment Updates

- Redeploy updated app
- Run `CREATE TABLE EventType` + `ALTER TABLE` scripts on Azure SQL
- Screenshots: Query Editor showing `EventType` table and updated `Venue` table with `IsAvailable` column

### C. Reflective Technical Report (Word document)

Must include:

1. **Full feature list** — bullet each feature with a brief explanation of how it works
2. **Component discussion:**
   - Azure Web App Service — why used, alternatives (e.g., Azure Container Apps, AKS)
   - Azure SQL Database — why used, alternatives (e.g., Cosmos DB, PostgreSQL Flexible Server)
   - Azure Blob Storage — why used, alternatives (e.g., Azure Files, CDN)
   - ASP.NET Core MVC — why used, alternatives (e.g., Razor Pages, Blazor)
   - Entity Framework Core — why used, alternatives (e.g., Dapper)
3. **Reflection** — personal experience, challenges faced, lessons learned, current understanding of cloud-based application architecture

---

## Config / Secrets Summary

Never commit real credentials. Use placeholders in source and override via Azure App Service Application Settings or `appsettings.Development.json` (gitignored).

| Setting | Placeholder | Where overridden |
|---|---|---|
| SQL connection string | `YOUR_CONNECTION_STRING_HERE` | Azure App Service → Configuration |
| Blob storage connection string | `YOUR_BLOB_STORAGE_CONNECTION_STRING_HERE` | Azure App Service → Configuration |
| Blob container name | `venue-images` | Can hardcode or put in config |

---

## File Structure (target end-state)

```
WebApplication1/
├── Controllers/
│   ├── HomeController.cs          (existing)
│   ├── VenuesController.cs        (new Part 1)
│   ├── EventsController.cs        (new Part 1)
│   └── BookingsController.cs      (new Part 1)
├── Data/
│   └── AppDbContext.cs            (new Part 1)
├── Models/
│   ├── ErrorViewModel.cs          (existing)
│   ├── Venue.cs                   (new Part 1)
│   ├── Event.cs                   (new Part 1)
│   ├── Booking.cs                 (new Part 1)
│   ├── BookingDetailsViewModel.cs (new Part 2)
│   └── EventType.cs               (new Part 3)
├── Views/
│   ├── Venues/   (Index, Details, Create, Edit, Delete)
│   ├── Events/   (Index, Details, Create, Edit, Delete)
│   ├── Bookings/ (Index with search+filters, Details, Create, Edit, Delete)
│   └── Shared/   (existing + updated _Layout.cshtml)
├── Migrations/                    (generated by EF)
├── appsettings.json               (with placeholders)
└── Program.cs                     (register DbContext + Blob service)
```

---

## Submission Checklist (per part)

### Part 1
- [ ] ERD diagram
- [ ] SQL database script (CREATE + INSERT + constraints)
- [ ] ASP.NET project with Venue/Event/Booking MVC (CRUD)
- [ ] Placeholder image URLs visible in app
- [ ] Azure Web App Service screenshot (portal + VS deployment success)
- [ ] Azure SQL Database screenshot (portal + Query Editor with tables/data)
- [ ] Deployed URL (`STxxx.azurewebsites.net`)
- [ ] Word doc with theory answers (Q1: cloud vs on-prem, Q2: IaaS/PaaS/SaaS)

### Part 2
- [ ] Images uploading to and displaying from Azure Blob Storage
- [ ] Double-booking validation with user alert
- [ ] Cannot delete venue/event with active bookings (alert shown)
- [ ] No crash on missing required fields
- [ ] Consolidated booking view (vw_BookingDetails) in DB and app
- [ ] Search by BookingId or EventName on bookings page
- [ ] Redeployed to Azure with updated features
- [ ] Screenshot: vw_BookingDetails in Azure Query Editor
- [ ] Word doc with theory answers (Cognitive Search, normalization)

### Part 3
- [ ] EventType lookup table seeded with categories
- [ ] IsAvailable field on Venue table
- [ ] Filters: EventType, date range, venue availability
- [ ] Redeployed to Azure
- [ ] Screenshot: EventType table in Query Editor
- [ ] Screenshot: Venue table showing IsAvailable column in Query Editor
- [ ] Reflective Technical Report (feature list + component discussion + personal reflection)
