# Arkane.Zeroconf Test Implementation Notes

## Current state

The suite now includes coverage for the Windows fallback lookup provider in addition to Bonjour-based paths.

### Windows fallback implementation coverage

- `Providers/WindowsMdnsProviderTests.cs`
  - Provider capability contract (browse-only)
  - Availability check behavior
  - Publish guard exception behavior
  - Basic Windows fallback browser/TXT behaviors

- `Integration/WindowsMdnsBrowserIntegrationTests.cs`
  - Verifies fallback discovery against real network services
  - Probes multiple service types in parallel
  - Configurable service-type list via `ARKANE_ZEROCONF_TEST_SERVICE_TYPES`

### Capability API coverage

- `Facades/ZeroconfSupportTests.cs`
  - Validates `ZeroconfSupport.Capabilities`
  - Validates `CanPublish` consistency with capability flags

## Operational guidance

- Use `ZeroconfSupport.CanPublish` before publish attempts.
- On Windows 11+, lookup can work without Bonjour through Windows DNS-SD fallback.
- Integration behavior depends on environment service availability.

## Recommended command set

```bash
# Build
dotnet build

# Core non-integration tests
dotnet test Arkane.Zeroconf.Tests --filter "FullyQualifiedName!~Integration"

# Windows fallback provider tests
dotnet test Arkane.Zeroconf.Tests --filter "FullyQualifiedName~WindowsMdnsProviderTests"

# Windows fallback integration test
dotnet test Arkane.Zeroconf.Tests --filter "FullyQualifiedName~WindowsMdnsBrowserIntegrationTests"
```

PowerShell override for integration probe types:

```powershell
$env:ARKANE_ZEROCONF_TEST_SERVICE_TYPES = "_http._tcp,_ipp._tcp,_printer._tcp"
