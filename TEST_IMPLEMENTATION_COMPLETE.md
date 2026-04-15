# Arkane.Zeroconf Test Implementation Notes

## Current state

The suite now includes coverage for both fallback providers (Windows DNS-SD and Linux systemd-resolved)
in addition to Bonjour-based paths.

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

### Linux systemd-resolved fallback implementation coverage

- `Providers/SystemdResolvedProviderTests.cs`
  - Provider capability contract
  - Availability check behavior
  - Browse/resolve behavior via D-Bus
  - Publish authorization behavior (polkit guard)
  - Service browser creation and disposal
  - TxtRecord creation and manipulation
  - All tests skip automatically when systemd-resolved is unavailable

- `Platform/PlatformSpecificTests.cs`
  - `LinuxMDNS_IsAvailable` — verifies Avahi/Bonjour presence on Linux
  - `LinuxSystemdResolved_IsAvailable` — verifies systemd-resolved provider availability
  - `WindowsMDNS_IsAvailable` — verifies Windows Bonjour/DNS-SD presence

### Capability API coverage

- `Facades/ZeroconfSupportTests.cs`
  - Validates `ZeroconfSupport.Capabilities`
  - Validates `CanPublish` consistency with capability flags

## Operational guidance

- Use `ZeroconfSupport.CanPublish` before publish attempts.
- On Windows 11+, lookup can work without Bonjour through Windows DNS-SD fallback.
- On Linux, browse/resolve can work without Avahi through systemd-resolved fallback.
- On Linux with systemd-resolved, publish requires polkit authorization.
- Run systemd-resolved publish tests with `sudo`; restore obj/ ownership afterwards.
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

# Linux systemd-resolved provider tests
dotnet test Arkane.Zeroconf.Tests --filter "FullyQualifiedName~SystemdResolvedProviderTests"

# Linux systemd-resolved provider tests with publish paths (requires sudo)
sudo dotnet test Arkane.Zeroconf.Tests --filter "FullyQualifiedName~SystemdResolvedProviderTests"
```

PowerShell override for integration probe types:

```powershell
$env:ARKANE_ZEROCONF_TEST_SERVICE_TYPES = "_http._tcp,_ipp._tcp,_printer._tcp"
```
