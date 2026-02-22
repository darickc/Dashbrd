# Testing Patterns

**Analysis Date:** 2026-02-22

## Test Framework

**Runner:**
- None - No formal test framework in use

**Assertion Library:**
- None

**Run Commands:**
```bash
# No test commands available
# No test projects exist
```

## Test File Organization

**Location:**
- No test files exist in the codebase
- No test projects (no `*.Tests.csproj`)
- No `Tests/` or `test/` directories

## In-Code Testing Patterns

**Test Mode Configuration:**
The codebase uses a configuration-driven test mode pattern instead of formal tests:

```csharp
// Property bound from appsettings
public bool Test { get; set; }

// In data fetch method:
if (Test)
{
    // Return hardcoded mock data
    return JsonConvert.DeserializeObject<WeatherInfo>(testJson);
}
// Otherwise make actual API call
```

**Modules with Test Mode:**
- `Dashbrd.Shared/Modules/Weather/Weather.razor.cs` - Embedded JSON test data (~315 lines)
- `Dashbrd.Shared/Modules/SolarEdge/SolarEdge.razor.cs` - Embedded JSON test data (~435 lines of `PowerJson` and `EnergyJson` constants)

**Test Mode Activation:**
- Set `Test: true` in `appsettings.Development.json` under the module's settings section
- Bypasses API calls and returns hardcoded data

## Coverage

**Requirements:**
- No coverage targets
- No coverage tooling configured

## Test Types

**Unit Tests:**
- Not present

**Integration Tests:**
- Not present

**E2E Tests:**
- Not present

## Missing Test Infrastructure

- No xUnit, NUnit, or MSTest references
- No Moq, FakeItEasy, or NSubstitute for mocking
- No FluentAssertions
- No test container setup
- No CI/CD test execution

## Recommendations for Adding Tests

**New Test Project:**
- Create `Dashbrd.Tests/Dashbrd.Tests.csproj`
- Framework suggestion: xUnit + Moq + FluentAssertions
- Test module logic by mocking IHttpClientFactory, IMemoryCache, IConfiguration

**Priority Areas:**
- Extension methods (pure functions, easy to test): `WeatherExtensions.cs`, `CalendarExtensions.cs`
- MessageService event bus logic
- API response parsing and model deserialization
- Cache hit/miss behavior

---

*Testing analysis: 2026-02-22*
*Update when test patterns change*
