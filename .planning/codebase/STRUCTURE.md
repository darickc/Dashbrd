# Codebase Structure

**Analysis Date:** 2026-02-22

## Directory Layout

```
Dashbrd/
├── Dashbrd.sln                    # Solution file (2 projects)
│
├── Dashbrd/                        # ASP.NET Core Host Project
│   ├── Program.cs                  # Entry point, host configuration
│   ├── Startup.cs                  # DI registration, middleware
│   ├── App.razor                   # Router + layout mapping
│   ├── appsettings.json           # Default configuration
│   ├── appsettings.Development.json # Dev overrides (test flags)
│   ├── Dashbrd.csproj             # Host project file
│   ├── Dockerfile                  # Docker container build
│   ├── Pages/
│   │   └── Index.razor            # Route "/" entry page
│   ├── Shared/
│   │   ├── MainLayout.razor       # Module composition (edit to add/move)
│   │   ├── MainLayout.razor.scss
│   │   ├── Layout.razor           # Grid template with 13 regions
│   │   ├── Layout.razor.cs        # RenderFragment parameters
│   │   └── Layout.razor.scss      # CSS Grid styling
│   ├── Properties/
│   │   └── launchSettings.json
│   └── wwwroot/
│       └── css/site.scss          # Global styles, utility classes
│
└── Dashbrd.Shared/                 # Razor Class Library
    ├── Dashbrd.Shared.csproj      # All module dependencies
    ├── _Imports.razor              # Global using statements
    ├── libman.json                 # Client-side libraries (fonts, icons)
    │
    ├── Common/
    │   └── MessageService.cs       # Event bus (singleton)
    │
    ├── Modules/
    │   ├── Clock/                  # System timer display
    │   │   ├── Clock.razor
    │   │   ├── Clock.razor.cs
    │   │   └── Clock.razor.scss
    │   │
    │   ├── Weather/                # OpenWeatherMap integration
    │   │   ├── Weather.razor
    │   │   ├── Weather.razor.cs
    │   │   ├── Weather.razor.scss
    │   │   ├── WeatherInfo.cs, Current.cs, Daily.cs, Temp.cs
    │   │   ├── Feels_Like.cs, DataSet.cs
    │   │   └── WeatherExtensions.cs
    │   │
    │   ├── Calendar/               # iCal feed parsing
    │   │   ├── Calendar.razor
    │   │   ├── Calendar.razor.cs
    │   │   ├── Calendar.razor.scss
    │   │   └── CalendarExtensions.cs
    │   │
    │   ├── SolarEdge/              # Solar monitoring + charts
    │   │   ├── SolarEdge.razor
    │   │   ├── SolarEdge.razor.cs
    │   │   ├── SolarEdge.razor.scss
    │   │   ├── SolarEdgeType.cs
    │   │   └── Solar.cs, SolarData.cs, SolarValue.cs
    │   │
    │   ├── Jellyfin/               # MQTT media playback
    │   │   ├── Jellyfin.razor
    │   │   ├── Jellyfin.razor.cs
    │   │   ├── Jellyfin.razor.scss
    │   │   ├── JellyfinNotificationData.cs
    │   │   ├── Item.cs, ItemType.cs
    │   │   └── Extensions.cs
    │   │
    │   ├── PhotoprismBackgroundImageSlideshow/  # Remote photo slideshow
    │   │   ├── PhotoprismBackgroundImageSlideshow.razor
    │   │   ├── PhotoprismBackgroundImageSlideshow.razor.cs
    │   │   ├── PhotoprismBackgroundImageSlideshow.razor.scss
    │   │   ├── PhotoprismImage.cs, PhotoprismThumb.cs
    │   │   ├── PhotoPrismSessionResponse.cs, PhotoPrismUser.cs
    │   │   └── PhotoprismBackgroundImageSlideshowMesage.cs
    │   │
    │   ├── BackgroundImageSlideshow/  # Local filesystem slideshow
    │   │   ├── BackgroundImageSlideshow.razor
    │   │   ├── BackgroundImageSlideshow.razor.cs
    │   │   ├── BackgroundImageSlideshow.razor.scss
    │   │   └── BackgroundImageSlideshowMesage.cs
    │   │
    │   └── Coop/                   # Coop monitoring (MQTT)
    │       ├── Coop.razor
    │       ├── Coop.razor.cs
    │       ├── Coop.razor.scss
    │       └── Extensions.cs
    │
    └── wwwroot/                    # Static assets
        ├── css/
        ├── lib/weathericons/
        └── fonts/
```

## Directory Purposes

**Dashbrd/ (Host Project):**
- Purpose: ASP.NET Core web app host, entry point, configuration
- Contains: Program.cs, Startup.cs, layout files, configuration
- Key files: `Program.cs` (entry), `Startup.cs` (DI), `Shared/MainLayout.razor` (composition)

**Dashbrd.Shared/ (Class Library):**
- Purpose: All reusable modules, components, and shared services
- Contains: Module components, MessageService, client-side assets
- Key files: `Common/MessageService.cs`, all module directories

**Dashbrd.Shared/Modules/[Name]/:**
- Purpose: Self-contained dashboard module
- Contains: .razor (markup), .razor.cs (logic), .razor.scss (styles), model classes
- Pattern: Each module is independent with its own models and extensions

**Dashbrd.Shared/Common/:**
- Purpose: Shared services used across modules
- Contains: `MessageService.cs` (event bus singleton)

## Key File Locations

**Entry Points:**
- `Dashbrd/Program.cs` - Application host startup
- `Dashbrd/Startup.cs` - DI registration and middleware

**Configuration:**
- `Dashbrd/appsettings.json` - Default settings with per-module config
- `Dashbrd/appsettings.Development.json` - Dev overrides (test flags)
- `Dashbrd/Dockerfile` - Docker container build

**Core Logic:**
- `Dashbrd/Shared/MainLayout.razor` - Module placement into regions
- `Dashbrd/Shared/Layout.razor` + `Layout.razor.cs` - Grid system with 13 regions
- `Dashbrd.Shared/Common/MessageService.cs` - Inter-module event bus

**Styling:**
- `Dashbrd/wwwroot/css/site.scss` - Global styles, utility classes
- `Dashbrd/Shared/Layout.razor.scss` - Grid layout styling
- `Dashbrd.Shared/Modules/[Name]/[Name].razor.scss` - Per-module styles

## Naming Conventions

**Files:**
- PascalCase for all C# and Razor files: `Weather.razor.cs`, `MessageService.cs`
- Three-file module pattern: `[Name].razor`, `[Name].razor.cs`, `[Name].razor.scss`
- Model classes: PascalCase nouns: `WeatherInfo.cs`, `SolarData.cs`, `PhotoprismImage.cs`
- Extensions: `[Name]Extensions.cs` or `Extensions.cs`

**Directories:**
- PascalCase for all directories: `Modules/`, `Weather/`, `Common/`
- Module directories match component name exactly

**Special Patterns:**
- `_Imports.razor` - Global using directives
- `*Mesage.cs` - Message DTOs (note: typo in "Message" -> "Mesage")

## Where to Add New Code

**New Module:**
- Create directory: `Dashbrd.Shared/Modules/[ModuleName]/`
- Add files: `[Name].razor`, `[Name].razor.cs`, `[Name].razor.scss`
- Add config section in `Dashbrd/appsettings.json` under `Settings:[ModuleName]`
- Place in layout: edit `Dashbrd/Shared/MainLayout.razor`

**New Shared Service:**
- Implementation: `Dashbrd.Shared/Common/[ServiceName].cs`
- Register DI: `Dashbrd/Startup.cs`

**New Model/DTO:**
- Place in module directory: `Dashbrd.Shared/Modules/[ModuleName]/[ModelName].cs`

**New Extension Methods:**
- Place in module directory: `Dashbrd.Shared/Modules/[ModuleName]/[Name]Extensions.cs`

## Special Directories

**wwwroot/ (both projects):**
- Purpose: Static files served by ASP.NET Core
- Source: Client-side CSS, fonts, icons
- Committed: Yes

**.planning/:**
- Purpose: Project planning and codebase documentation
- Source: Generated by analysis tools
- Committed: Yes (documentation)

---

*Structure analysis: 2026-02-22*
*Update when directory structure changes*
