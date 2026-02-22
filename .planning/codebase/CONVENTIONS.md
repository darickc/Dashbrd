# Coding Conventions

**Analysis Date:** 2026-02-22

## Naming Patterns

**Files:**
- PascalCase for all files: `Weather.razor.cs`, `MessageService.cs`
- Three-file module pattern: `[Name].razor` + `[Name].razor.cs` + `[Name].razor.scss`
- Extension files: `[Name]Extensions.cs` or `Extensions.cs`
- Model files: PascalCase nouns matching class name

**Functions:**
- PascalCase for public methods: `GetWeatherInfo()`, `CardinalWindDirection()`
- Private event handlers: `_timer_Elapsed`, `_client_ConnectingFailedAsync`

**Variables:**
- PascalCase for public properties: `UpdateInterval`, `ApiKey`
- Underscore prefix for private fields: `_timer`, `_client`, `_messages`
- API-bound properties preserve source naming (including snake_case): `Feels_Like`, `Wind_speed`, `Timezone_offset`

**Types:**
- PascalCase for classes, interfaces, enums: `WeatherInfo`, `SolarEdgeType`, `ItemType`
- PascalCase for enum values: `Power`, `Energy`, `Audio`, `Movie`
- No I-prefix convention observed on interfaces

**Namespaces:**
- Pattern: `Dashbrd.Shared.Modules.[ModuleName]`
- Newer files use file-scoped namespaces: `namespace Dashbrd.Shared.Modules.Weather;`
- Older files use block-scoped: `namespace Dashbrd.Shared.Modules.Weather { ... }`

## Code Style

**Formatting:**
- 4-space indentation
- Allman-style braces (opening brace on new line for class/method declarations)
- Blank lines between methods and logical sections
- No .editorconfig or Prettier/formatting config detected

**Linting:**
- No explicit linting configuration detected
- Relies on default C# compiler warnings

## Import Organization

**Order:**
1. System namespaces
2. Microsoft namespaces
3. Third-party packages (MQTTnet, ChartJs, etc.)
4. Project namespaces (Dashbrd.Shared.*)

**Grouping:**
- Grouped logically, ordered alphabetically within groups
- Global usings in `Dashbrd.Shared/_Imports.razor`

**Path Aliases:**
- None - uses full namespace references

## Error Handling

**Patterns (inconsistent across modules):**
- Jellyfin: Railway-oriented with `Result<T>` chaining via CSharpFunctionalExtensions (`.Map()`, `.Tap()`, `.Bind()`, `.TapError()`)
- Calendar: Traditional try/catch with `ILogger.LogError()`
- Weather/SolarEdge: Empty catch blocks (silent exception swallowing)
- Photoprism: Polly retry policy for 401 auth failures

**Error Types:**
- No custom error/exception classes defined
- Relies on framework exceptions

## Logging

**Framework:**
- `ILogger<T>` via dependency injection

**Patterns:**
- Injected in some modules: Jellyfin, Calendar
- Inconsistently used - Weather and SolarEdge have no logging
- Jellyfin uses `.TapError(e => Logger.LogError(e))` with functional extensions

## Comments

**When to Comment:**
- Sparse commenting style - self-documenting code preferred
- XML doc comments on enums with descriptions (`Dashbrd.Shared/Modules/Jellyfin/ItemType.cs`)
- Inline comments rare, only for non-obvious logic

**TODO Comments:**
- Not widely used in codebase

## Function Design

**Size:**
- Most methods are moderate length
- Some large methods in modules that handle API calls + caching + rendering

**Parameters:**
- Configuration via property binding rather than constructor parameters
- `[Inject]` attribute for DI dependencies
- `[Parameter]` attribute for Blazor component parameters

**Return Values:**
- Async methods return `Task` or `Task<T>`
- Jellyfin module uses `Result<T>` for functional chaining

## Module Design

**Component Pattern:**
- Partial class: `.razor` (markup) + `.razor.cs` (logic)
- Code-behind uses `partial class` keyword
- DI via `[Inject]` property injection
- Config via `Configuration.GetSection("Settings:ModuleName").Bind(this)`

**Lifecycle Pattern:**
- `OnInitializedAsync()` - Bind config, create timer, initial data fetch
- Timer callback - Refresh data on interval
- `IDisposable.Dispose()` - Clean up timers and connections (inconsistently implemented)

**Caching Pattern:**
- `IMemoryCache` with string cache keys: `"WeatherCache"`, `"CalendarCache"`, `"SolarEdgePower"`
- TTL: `UpdateInterval - 5` minutes
- Cache-first: check cache before API call

**Test Mode Pattern:**
- `public bool Test { get; set; }` property bound from config
- `if (Test)` branch returns hardcoded JSON mock data
- Enabled via `appsettings.Development.json`

---

*Convention analysis: 2026-02-22*
*Update when patterns change*
