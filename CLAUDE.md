# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build & Run Commands

```bash
# Build the solution
dotnet build

# Run the web app (dev mode)
dotnet run --project Dashbrd

# Publish for production
dotnet publish Dashbrd -c Release

# Docker build (run from repo root)
docker build -f Dashbrd/Dockerfile .
```

There are no tests in this project.

## Architecture

**Dashbrd** is a kiosk-style information dashboard built with Blazor Server (.NET 10). It displays weather, calendar, solar production, media playback, and photo slideshows on a single-page dark-themed display.

### Two-Project Structure

- **Dashbrd/** — Main web app (ASP.NET Core host, layout, configuration, entry point)
- **Dashbrd.Shared/** — Razor class library containing all modules/components and shared services

### Layout System

`Layout.razor` defines a grid of named regions: `FullscreenBelow`, `FullscreenAbove`, `TopLeft`, `TopCenter`, `TopRight`, `BottomLeft`, `BottomCenter`, `BottomRight`, `UpperThird`, `MiddleCenter`, `LowerThird`, `TopBar`, `BottomBar`.

`MainLayout.razor` composes modules into these regions. To add/move a module, edit MainLayout.razor.

### Module Pattern

Each module in `Dashbrd.Shared/Modules/` is a self-contained Blazor component:
- `ModuleName.razor` — Markup
- `ModuleName.razor.cs` — Code-behind (partial class with logic, timers, API calls)
- `ModuleName.razor.scss` — Styles (compiled to CSS by LibSassBuilder)
- Supporting model classes in the same directory

Modules load their config from `appsettings.json` via `Configuration.GetSection("Settings:ModuleName").Bind(this)`. Each module manages its own timer-based refresh cycle and disposes resources via `IDisposable`.

### Modules

| Module | Integration | Key Details |
|--------|-------------|-------------|
| Clock | System timer | 200ms tick |
| Calendar | iCal URLs | Parses `.ics` feeds, uses IMemoryCache |
| Weather | OpenWeatherMap REST API | Uses IMemoryCache, has test mode |
| SolarEdge | SolarEdge Monitoring API | ChartJs charts, Power vs Energy views via `SolarEdgeType` enum |
| Jellyfin | MQTT (MQTTnet) | Subscribes to media playback events |
| PhotoprismBackgroundImageSlideshow | Photoprism REST API | Background image rotation with session auth |
| BackgroundImageSlideshow | Local filesystem | Rotates images from a configured directory |

### Key Services

- **MessageService** (`Dashbrd.Shared/Common/`) — Singleton event bus for inter-component communication. Injected via DI.
- **IHttpClientFactory** — Registered in Startup.cs for HTTP calls in modules.
- **IMemoryCache** — Used by Weather and Calendar modules for API response caching.

### Configuration & Secrets

Configuration is layered: `appsettings.json` → `secrets.json` (optional) → Docker secrets (`/run/secrets/*.json`). API keys and sensitive settings go in `secrets.json` or user secrets (not checked in). The `Settings` section contains per-module config blocks.

### Styling

SCSS files compile automatically via LibSassBuilder. The app uses a dark theme (black background, no cursor, no scrollbars) designed for always-on displays. CSS sizing classes: `.xsmall`, `.small`, `.medium`, `.large`, `.xlarge`. Dimming classes: `.dimmed`, `.normal`, `.bright`. Font: Roboto Condensed.
