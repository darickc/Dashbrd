# External Integrations

**Analysis Date:** 2026-02-22

## APIs & External Services

**Weather Data:**
- OpenWeatherMap One Call API - Current weather, daily forecasts, sunrise/sunset
  - Endpoint: `https://api.openweathermap.org/data/2.5/onecall`
  - Auth: API key via `AppId` config property
  - Parameters: Latitude, Longitude, Units (imperial), Exclude (hourly, minutely)
  - Cache: IMemoryCache with configurable TTL
  - Test mode: Embedded JSON mock data
  - File: `Dashbrd.Shared/Modules/Weather/Weather.razor.cs`

**Solar Production:**
- SolarEdge Monitoring API - Power and energy production data
  - Power endpoint: `https://monitoringapi.solaredge.com/site/{siteId}/power`
  - Energy endpoint: `https://monitoringapi.solaredge.com/site/{siteId}/energy`
  - Auth: API key via `ApiKey` config property (query parameter)
  - Visualization: ChartJs (Bar charts for energy, Line charts for power)
  - Cache: IMemoryCache (separate keys for power/energy)
  - Test mode: Embedded JSON mock data
  - File: `Dashbrd.Shared/Modules/SolarEdge/SolarEdge.razor.cs`

**Photo Gallery:**
- Photoprism REST API - Photo slideshow from favorites
  - Base: `{PhotoprismApiUrl}/api/v1/`
  - Auth: Session-based (`POST /api/v1/session` with username/password, `X-Session-ID` header)
  - Photos: `GET /api/v1/photos/view?count=1000&q=favorites`
  - Thumbnails: Fit1920, Fit1280, Fit720 sizes
  - Retry: Polly async retry on 401 (auto token refresh)
  - File: `Dashbrd.Shared/Modules/PhotoprismBackgroundImageSlideshow/PhotoprismBackgroundImageSlideshow.razor.cs`

**Calendar:**
- iCal/iCalendar feeds - Calendar event display
  - Integration: HTTP fetch of `.ics` URLs
  - Parser: Fusonic.Ical.Net
  - Features: Multiple calendar URLs, next 5 events for 2 months
  - Cache: IMemoryCache
  - File: `Dashbrd.Shared/Modules/Calendar/Calendar.razor.cs`

## MQTT Integrations

**Jellyfin Media Server:**
- Protocol: MQTT via MQTTnet (TCP)
  - Connection: Configurable URI, port (default 1883), TLS optional
  - Auth: Username/password
  - Topics subscribed:
    - `{MqttBaseTopic}/PlaybackStart`
    - `{MqttBaseTopic}/PlaybackStop`
    - `{MqttBaseTopic}/PlaybackProgress`
  - Features: Receives playback events, fetches media metadata via Jellyfin HTTP API
  - Jellyfin API auth: `X-Emby-Token` header with API key
  - Auto-reconnect: 5-second delay on connection failure
  - File: `Dashbrd.Shared/Modules/Jellyfin/Jellyfin.razor.cs`

**Coop Monitoring:**
- Protocol: MQTT via MQTTnet (TCP)
  - Topics monitored:
    - `{MqttBaseTopic}/water/temp` - Water temperature
    - `{MqttBaseTopic}/water/level` - Water level
    - `{MqttBaseTopic}/water/heater` - Heater state
    - `{MqttBaseTopic}/temp` - Ambient temperature
    - `{MqttBaseTopic}/food/level` - Food level
    - `{MqttBaseTopic}/door/state` - Door state
  - Connection: TLS support, credentials, auto-reconnect
  - File: `Dashbrd.Shared/Modules/Coop/Coop.razor.cs`

## Data Storage

**Databases:**
- None - No database used

**File Storage:**
- Local filesystem for BackgroundImageSlideshow (`Dashbrd.Shared/Modules/BackgroundImageSlideshow/BackgroundImageSlideshow.razor.cs`)
- Rotates images from a configured directory path

**Caching:**
- IMemoryCache (in-process, server-side)
  - Weather: `"WeatherCache"` key
  - Calendar: `"CalendarCache"` key
  - SolarEdge: `"SolarEdgePower"` and `"SolarEdgeEnergy"` keys

## Authentication & Identity

**Auth Provider:**
- None - No user authentication (kiosk-style display)

**Service Authentication:**
- OpenWeatherMap: API key (query parameter)
- SolarEdge: API key (query parameter)
- Photoprism: Session auth (POST credentials, receive session ID)
- Jellyfin: API key (`X-Emby-Token` header) + MQTT credentials
- MQTT Broker: Username/password with optional TLS

## Monitoring & Observability

**Error Tracking:**
- None - No external error tracking service

**Analytics:**
- None

**Logs:**
- ASP.NET Core built-in logging (`ILogger<T>`)
- Console/stdout output

## CI/CD & Deployment

**Hosting:**
- Docker container (Linux)
  - Base: `mcr.microsoft.com/dotnet/aspnet:7.0`
  - Ports: 80, 443
  - File: `Dashbrd/Dockerfile`

**CI Pipeline:**
- Not detected

## Environment Configuration

**Development:**
- Required: API keys for Weather, SolarEdge, Photoprism, Jellyfin, MQTT broker
- Secrets location: `secrets.json` (gitignored) or .NET User Secrets
- Test mode: `appsettings.Development.json` with `Test: true` flags

**Production:**
- Secrets management: Docker secrets (`/run/secrets/*.json`)
- Configuration layering: appsettings.json -> secrets.json -> Docker secrets
- File: `Dashbrd/Program.cs` (configuration loading)

## Inter-Module Communication

**MessageService Event Bus:**
- Singleton service registered in `Dashbrd/Startup.cs`
- Pub/sub pattern for module-to-module messages
- Used by: Jellyfin -> PhotoprismBackgroundImageSlideshow (backdrop display)
- Used by: Jellyfin -> BackgroundImageSlideshow (pause/resume)
- File: `Dashbrd.Shared/Common/MessageService.cs`

---

*Integration audit: 2026-02-22*
*Update when adding/removing external services*
