# Arkane.Zeroconf Test Suite - Summary

## Overview

The test suite validates:
- Public facades (`ServiceBrowser`, `RegisterService`, `TxtRecord`)
- Provider discovery and capability behavior
- Bonjour integration paths
- Windows DNS-SD fallback lookup paths
- Linux systemd-resolved fallback paths
- Performance and stress behavior

## Notable recent additions

- `Facades/ZeroconfSupportTests.cs`
  - Verifies capability API behavior (`Capabilities`, `CanBrowse`, `CanPublish`).

- `Providers/WindowsMdnsProviderTests.cs`
  - Verifies Windows fallback provider capabilities (browse-only).
  - Verifies lookup-only publish guard (`PlatformNotSupportedException`).

- `Providers/SystemdResolvedProviderTests.cs`
  - Verifies Linux systemd-resolved fallback provider capabilities.
  - Verifies browse/resolve behavior via D-Bus.
  - Verifies publish authorization behavior (polkit guard).
  - Skips automatically when systemd-resolved is unavailable.

- `Integration/WindowsMdnsBrowserIntegrationTests.cs`
  - Verifies discovery via Windows fallback provider.
  - Probes common service types in parallel.
  - Supports `ARKANE_ZEROCONF_TEST_SERVICE_TYPES` override.

- `Platform/PlatformSpecificTests.cs`
  - `LinuxMDNS_IsAvailable` — verifies Avahi/Bonjour presence on Linux.
  - `LinuxSystemdResolved_IsAvailable` — verifies systemd-resolved provider availability.
  - `WindowsMDNS_IsAvailable` — verifies Windows Bonjour/DNS-SD presence.

## Running targeted suites

```bash
# Build
dotnet build

# Non-integration tests
dotnet test Arkane.Zeroconf.Tests --filter "FullyQualifiedName!~Integration"

# Windows fallback provider unit tests
dotnet test Arkane.Zeroconf.Tests --filter "FullyQualifiedName~WindowsMdnsProviderTests"

# Windows fallback integration test
dotnet test Arkane.Zeroconf.Tests --filter "FullyQualifiedName~WindowsMdnsBrowserIntegrationTests"

# Linux systemd-resolved provider tests
dotnet test Arkane.Zeroconf.Tests --filter "FullyQualifiedName~SystemdResolvedProviderTests"

# Linux systemd-resolved provider tests with publish paths (requires sudo)
sudo dotnet test Arkane.Zeroconf.Tests --filter "FullyQualifiedName~SystemdResolvedProviderTests"
```

PowerShell service type override for fallback integration:

```powershell
$env:ARKANE_ZEROCONF_TEST_SERVICE_TYPES = "_http._tcp;_ssh._tcp"
```

## Notes

- Bonjour integration tests still require Bonjour/Avahi availability.
- Windows fallback provider supports lookup only; publishing is intentionally unsupported.
- Linux systemd-resolved provider supports publish only with polkit authorization (`sudo` or polkit rule).
- After running tests with `sudo`, restore obj/ ownership: `sudo find . -path '*/obj/*' | xargs sudo chown $USER`
