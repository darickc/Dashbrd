# Architecture

**Analysis Date:** 2026-02-22

## Pattern Overview

**Overall:** Two-Project Monolithic Blazor Server Dashboard

**Key Characteristics:**
- Single-page kiosk display with modular grid layout
- Server-side rendering via SignalR (Blazor Server)
- Self-contained modules with timer-based refresh cycles
- Event bus for inter-module communication

## Layers

**UI Layer (Layout & Composition):**
- Purpose: Define grid regions and compose modules into layout
- Contains: Layout component with 13 named regions, MainLayout with module placement
- Location: `Dashbrd/Shared/MainLayout.razor`, `Dashbrd/Shared/Layout.razor`, `Dashbrd/Shared/Layout.razor.cs`
- Depends on: Module layer (renders module components)
- Used by: Blazor routing system

**Module Layer:**
- Purpose: Self-contained feature units (weather, calendar, media, etc.)
- Contains: Razor markup, code-behind logic, SCSS styles, supporting models
- Location: `Dashbrd.Shared/Modules/[ModuleName]/`
- Depends on: Service layer (DI, HTTP, caching, messaging)
- Used by: UI layer (composed in MainLayout)

**Service Layer:**
- Purpose: Shared infrastructure services
- Contains: MessageService (event bus), IHttpClientFactory, IMemoryCache, IConfiguration
- Location: `Dashbrd.Shared/Common/MessageService.cs`, `Dashbrd/Startup.cs`
- Depends on: .NET framework services
- Used by: Module layer

**External Integration Layer:**
- Purpose: Communication with external APIs and services
- Contains: REST API calls, MQTT subscriptions, iCal feed parsing
- Location: Embedded in module code-behind files
- Depends on: HTTP clients, MQTT clients
- Used by: Module layer

## Data Flow

**Application Startup:**

1. `Dashbrd/Program.cs` builds host, loads layered configuration
2. `Dashbrd/Startup.cs` registers DI services (Blazor, HttpClient, MessageService)
3. Router maps to `Index.razor` -> `MainLayout.razor` -> `Layout.razor`
4. Layout renders 13 named regions with composed modules
5. Each module initializes independently via `OnInitializedAsync()`

**Module Refresh Cycle:**

1. Module binds config: `Configuration.GetSection("Settings:ModuleName").Bind(this)`
2. Creates timer with configured interval
3. Timer fires -> check IMemoryCache for cached data
4. Cache miss -> make HTTP/MQTT request to external service
5. Cache result with TTL = UpdateInterval - 5 minutes
6. Update component state properties
7. Call `InvokeAsync(StateHasChanged)` to trigger UI re-render

**Inter-Module Communication (Jellyfin -> Slideshow):**

1. Jellyfin receives MQTT playback event
2. Fetches media metadata and backdrop image via Jellyfin API
3. Sends message via `MessageService.SendMessage()`
4. PhotoprismBackgroundImageSlideshow receives via `OnMessage` event
5. Pauses slideshow, displays media backdrop
6. Resumes slideshow when playback stops

**State Management:**
- Server-side (Blazor Server): component state lives on server, synced via SignalR
- IMemoryCache for API response caching (Weather, Calendar, SolarEdge)
- No persistent state / database

## Key Abstractions

**Module:**
- Purpose: Self-contained dashboard feature unit
- Examples: `Clock`, `Weather`, `Calendar`, `SolarEdge`, `Jellyfin`, `PhotoprismBackgroundImageSlideshow`, `BackgroundImageSlideshow`, `Coop`
- Pattern: Partial class with .razor markup + .razor.cs code-behind + .razor.scss styles
- Location: `Dashbrd.Shared/Modules/[Name]/`

**Layout Region:**
- Purpose: Named placement slot in the dashboard grid
- Examples: `TopLeft`, `TopCenter`, `TopRight`, `BottomLeft`, `BottomCenter`, `BottomRight`, `FullscreenBelow`, `FullscreenAbove`, `UpperThird`, `MiddleCenter`, `LowerThird`, `TopBar`, `BottomBar`
- Pattern: RenderFragment parameters on Layout component
- Location: `Dashbrd/Shared/Layout.razor.cs`

**MessageService (Event Bus):**
- Purpose: Pub/sub communication between modules
- Examples: Jellyfin playback -> slideshow backdrop display
- Pattern: Singleton service with event delegate
- Location: `Dashbrd.Shared/Common/MessageService.cs`

## Entry Points

**Application Host:**
- Location: `Dashbrd/Program.cs`
- Triggers: `dotnet run` or Docker container start
- Responsibilities: Build host, load configuration, start web server

**Service Registration:**
- Location: `Dashbrd/Startup.cs`
- Triggers: Host startup
- Responsibilities: Register DI services, configure middleware, map Blazor hub

**Module Composition:**
- Location: `Dashbrd/Shared/MainLayout.razor`
- Triggers: Page render
- Responsibilities: Place modules into layout regions

## Error Handling

**Strategy:** Mixed - inconsistent across modules

**Patterns:**
- Jellyfin: Railway-oriented with `Result<T>` chaining via CSharpFunctionalExtensions (`Dashbrd.Shared/Modules/Jellyfin/Jellyfin.razor.cs`)
- Calendar: Try/catch with `ILogger` error logging (`Dashbrd.Shared/Modules/Calendar/Calendar.razor.cs`)
- Weather/SolarEdge: Silent exception swallowing with empty catch blocks
- Photoprism: Polly retry policy for auth failures (401 -> token refresh)

## Cross-Cutting Concerns

**Logging:**
- `ILogger<T>` injected in some modules (Jellyfin, Calendar)
- Inconsistently used - many modules don't log errors

**Caching:**
- `IMemoryCache` for API responses (Weather, Calendar, SolarEdge)
- TTL pattern: UpdateInterval - 5 minutes

**Configuration:**
- Layered: appsettings.json -> secrets.json -> Docker secrets
- Module binding: `Configuration.GetSection("Settings:ModuleName").Bind(this)`

---

*Architecture analysis: 2026-02-22*
*Update when major patterns change*
