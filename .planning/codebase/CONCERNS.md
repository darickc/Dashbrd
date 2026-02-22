# Codebase Concerns

**Analysis Date:** 2026-02-22

## Tech Debt

**Missing IDisposable on timer-based modules:**
- Issue: Multiple modules create `System.Timers.Timer` objects but don't implement `IDisposable` to dispose them
- Files: `Dashbrd.Shared/Modules/Weather/Weather.razor.cs`, `Dashbrd.Shared/Modules/SolarEdge/SolarEdge.razor.cs`, `Dashbrd.Shared/Modules/Calendar/Calendar.razor.cs`, `Dashbrd.Shared/Modules/Clock/Clock.razor.cs`, `Dashbrd.Shared/Modules/BackgroundImageSlideshow/BackgroundImageSlideshow.razor.cs`, `Dashbrd.Shared/Modules/PhotoprismBackgroundImageSlideshow/PhotoprismBackgroundImageSlideshow.razor.cs`
- Why: Likely oversight during rapid development
- Impact: Resource leaks - timers continue firing after component disposal
- Fix approach: Implement `IDisposable` on each module, dispose timer in `Dispose()` method

**Event handler memory leak in MessageService subscribers:**
- Issue: Modules subscribe to `MessageService.OnMessage` but never unsubscribe
- Files: `Dashbrd.Shared/Modules/PhotoprismBackgroundImageSlideshow/PhotoprismBackgroundImageSlideshow.razor.cs`, `Dashbrd.Shared/Modules/BackgroundImageSlideshow/BackgroundImageSlideshow.razor.cs`
- Why: MessageService is a singleton; event handlers persist for app lifetime
- Impact: Prevents component garbage collection, potential memory growth
- Fix approach: Unsubscribe from `OnMessage` in `Dispose()` method

**Large embedded test data:**
- Issue: Hundreds of lines of hardcoded JSON test data embedded in module files
- Files: `Dashbrd.Shared/Modules/Weather/Weather.razor.cs` (~315 lines of JSON), `Dashbrd.Shared/Modules/SolarEdge/SolarEdge.razor.cs` (~435 lines of JSON)
- Why: Quick test mode implementation
- Impact: Code files are bloated and hard to navigate (SolarEdge.razor.cs is ~700 lines)
- Fix approach: Extract test data to embedded resource files or a test data directory

**Naming typo in message classes:**
- Issue: "Message" is misspelled as "Mesage" in class names and filenames
- Files: `Dashbrd.Shared/Modules/PhotoprismBackgroundImageSlideshow/PhotoprismBackgroundImageSlideshowMesage.cs`, `Dashbrd.Shared/Modules/BackgroundImageSlideshow/BackgroundImageSlideshowMesage.cs`
- Impact: Inconsistent naming, unprofessional
- Fix approach: Rename classes and files to correct spelling

**.NET 7 end-of-life:**
- Issue: Both projects target `net7.0` which reached end-of-support in May 2024
- Files: `Dashbrd/Dashbrd.csproj`, `Dashbrd.Shared/Dashbrd.Shared.csproj`
- Impact: No security patches, no official support
- Fix approach: Upgrade to `net8.0` (LTS) or `net9.0`

## Known Bugs

**No known bugs documented.** However, the silent exception swallowing (see below) may be hiding issues.

## Security Considerations

**HTTPS redirection disabled:**
- Risk: `app.UseHttpsRedirection()` is commented out in `Dashbrd/Startup.cs`
- Current mitigation: Likely running on local network only (kiosk)
- Recommendations: Enable HTTPS if exposed beyond local network; API keys are transmitted in URLs

**Hardcoded API base URLs:**
- Risk: API endpoints hardcoded in module files rather than configuration
- Files: `Dashbrd.Shared/Modules/Weather/Weather.razor.cs` (OpenWeatherMap URL), `Dashbrd.Shared/Modules/SolarEdge/SolarEdge.razor.cs` (SolarEdge URL)
- Current mitigation: Only base URLs are hardcoded; API keys are in config
- Recommendations: Move base URLs to `appsettings.json` for flexibility

## Performance Bottlenecks

**No significant performance issues detected.** The caching strategy (IMemoryCache with TTL) is appropriate for this use case.

## Fragile Areas

**Silent exception swallowing:**
- Files: `Dashbrd.Shared/Modules/Weather/Weather.razor.cs`, `Dashbrd.Shared/Modules/SolarEdge/SolarEdge.razor.cs`
- Why fragile: Empty `catch { }` blocks hide API failures, auth issues, network problems
- Common failures: API key expiration, rate limiting, network outages go undetected
- Safe modification: Add `Logger.LogError(ex, "message")` to all catch blocks
- Test coverage: No tests

**Async void event handlers:**
- Files: `Dashbrd.Shared/Modules/PhotoprismBackgroundImageSlideshow/PhotoprismBackgroundImageSlideshow.razor.cs` (MessageService_OnMessage, Timer_Elapsed), `Dashbrd.Shared/Modules/BackgroundImageSlideshow/BackgroundImageSlideshow.razor.cs` (MessageService_OnMessage, Timer_Elapsed)
- Why fragile: `async void` methods are fire-and-forget; unhandled exceptions crash the app
- Safe modification: Wrap method body in try/catch, or restructure to avoid async void

**Inconsistent error handling across modules:**
- Why fragile: Jellyfin uses `Result<T>` chaining, Calendar uses try/catch + logging, Weather/SolarEdge swallow silently
- Common failures: Difficult to diagnose issues when some modules log and others don't
- Safe modification: Standardize on try/catch + `ILogger` across all modules

## Dependencies at Risk

**.NET 7.0:**
- Risk: End-of-life since May 2024, no security patches
- Impact: Potential unpatched vulnerabilities
- Migration plan: Upgrade to .NET 8.0 LTS (minimal breaking changes expected)

**ChartJs.Blazor.Fork 2.0.2:**
- Risk: Fork of a library; may not receive updates
- Impact: SolarEdge charting could break with future .NET upgrades
- Migration plan: Monitor for alternatives or consider Radzen charts (already a dependency)

## Test Coverage Gaps

**Entire codebase untested:**
- What's not tested: All modules, services, extension methods, models
- Risk: Any change could break functionality undetected
- Priority: Medium (kiosk app with limited user interaction)
- Difficulty to test: Modules are tightly coupled to Blazor lifecycle; extension methods and model logic are easily testable

## Missing Critical Features

No critical features missing for current use case (kiosk display).

---

*Concerns audit: 2026-02-22*
*Update as issues are fixed or new ones discovered*
