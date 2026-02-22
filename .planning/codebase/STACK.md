# Technology Stack

**Analysis Date:** 2026-02-22

## Languages

**Primary:**
- C# (.NET 7.0) - All application code

**Secondary:**
- SCSS - Module and layout styling (compiled via LibSassBuilder)
- HTML/Razor - Component templates

## Runtime

**Environment:**
- .NET 7.0 (ASP.NET Core / Blazor Server)
- SignalR for real-time UI updates (Blazor Server default)

**Package Manager:**
- NuGet - .NET package management
- LibMan - Client-side library management (`Dashbrd.Shared/libman.json`)

## Frameworks

**Core:**
- ASP.NET Core Blazor Server - Web framework with server-side rendering
- Radzen.Blazor 4.16.2 - UI component library (progress bars, etc.)

**Testing:**
- None - No test framework in use

**Build/Dev:**
- LibSassBuilder 3.0.0 - SCSS to CSS compilation
- Docker multi-stage build - `Dashbrd/Dockerfile`

## Key Dependencies

**Critical:**
- MQTTnet.Extensions.ManagedClient 4.3.1 - MQTT messaging for Jellyfin and Coop modules (`Dashbrd.Shared/Dashbrd.Shared.csproj`)
- ChartJs.Blazor.Fork 2.0.2 - Chart visualization for SolarEdge (`Dashbrd.Shared/Modules/SolarEdge/SolarEdge.razor.cs`)
- Fusonic.Ical.Net 5.0.3 - iCal feed parsing for Calendar (`Dashbrd.Shared/Modules/Calendar/Calendar.razor.cs`)
- CSharpFunctionalExtensions 2.40.1 - Railway-oriented error handling (`Dashbrd.Shared/Modules/Jellyfin/Jellyfin.razor.cs`)
- Polly 8.2.0 - Retry policies for Photoprism auth (`Dashbrd.Shared/Modules/PhotoprismBackgroundImageSlideshow/PhotoprismBackgroundImageSlideshow.razor.cs`)

**Infrastructure:**
- Microsoft.Extensions.Caching.Memory - IMemoryCache for API response caching
- IHttpClientFactory - HTTP client pooling (registered in `Dashbrd/Startup.cs`)
- Newtonsoft.Json - JSON deserialization for Jellyfin MQTT payloads

**Frontend:**
- roboto-fontface 0.10.0 - Roboto Condensed font (`Dashbrd.Shared/libman.json`)
- weathericons 2.1.0 - Weather icon font (`Dashbrd.Shared/libman.json`)

## Configuration

**Environment:**
- Layered configuration: `appsettings.json` -> `secrets.json` -> Docker secrets (`/run/secrets/*.json`)
- Configuration loading: `Dashbrd/Program.cs`
- Per-module config via `Settings:ModuleName` sections in appsettings
- API keys and secrets in `secrets.json` or user secrets (not checked in)

**Build:**
- `Dashbrd/Dashbrd.csproj` - Host project
- `Dashbrd.Shared/Dashbrd.Shared.csproj` - Shared library with all module dependencies

## Platform Requirements

**Development:**
- .NET 7 SDK
- Any platform with .NET support (macOS/Linux/Windows)

**Production:**
- Docker container (Linux-based)
- Base image: `mcr.microsoft.com/dotnet/aspnet:7.0`
- Ports: 80 (HTTP), 443 (HTTPS)
- Entry point: `dotnet Dashbrd.dll`

---

*Stack analysis: 2026-02-22*
*Update after major dependency changes*
