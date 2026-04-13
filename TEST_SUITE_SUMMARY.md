# Arkane.Zeroconf Test Suite - Summary

## Overview

The test suite validates:
- Public facades (`ServiceBrowser`, `RegisterService`, `TxtRecord`)
- Provider discovery and capability behavior
- Bonjour integration paths
- Windows DNS-SD fallback lookup paths
- Performance and stress behavior

## Notable recent additions

- `Facades/ZeroconfSupportTests.cs`
  - Verifies capability API behavior (`Capabilities`, `CanBrowse`, `CanPublish`).

- `Providers/WindowsMdnsProviderTests.cs`
  - Verifies Windows fallback provider capabilities (browse-only).
  - Verifies lookup-only publish guard (`PlatformNotSupportedException`).

- `Integration/WindowsMdnsBrowserIntegrationTests.cs`
  - Verifies discovery via Windows fallback provider.
  - Probes common service types in parallel.
  - Supports `ARKANE_ZEROCONF_TEST_SERVICE_TYPES` override.

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
```

PowerShell service type override for fallback integration:

```powershell
$env:ARKANE_ZEROCONF_TEST_SERVICE_TYPES = "_http._tcp;_ssh._tcp"
```

## Notes

- Bonjour integration tests still require Bonjour/Avahi availability.
- Windows fallback provider supports lookup only; publishing is intentionally unsupported.
