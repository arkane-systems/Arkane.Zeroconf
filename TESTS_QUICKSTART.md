# Quick Start: Running Tests for Arkane.Zeroconf

## TL;DR

```bash
# Run full suite
dotnet test Arkane.Zeroconf.Tests

# Run non-integration tests only
dotnet test Arkane.Zeroconf.Tests --filter "FullyQualifiedName!~Integration"

# Run Windows fallback integration test only
dotnet test Arkane.Zeroconf.Tests --filter "FullyQualifiedName~WindowsMdnsBrowserIntegrationTests"
```

## Key behavior updates

- On Windows 11+, if Bonjour is unavailable, the library can still browse/resolve via the `WindowsMdns` lookup-only provider.
- Publishing in fallback mode is intentionally unsupported. Use `ZeroconfSupport.CanPublish` before publish operations.

## Windows fallback integration test

`WindowsMdnsBrowserIntegrationTests` probes multiple common service types and passes if at least one service is discovered.

Override service types for your network:

```powershell
$env:ARKANE_ZEROCONF_TEST_SERVICE_TYPES = "_http._tcp,_ipp._tcp,_printer._tcp"
dotnet test Arkane.Zeroconf.Tests --filter "FullyQualifiedName~WindowsMdnsBrowserIntegrationTests"
```

## Bonjour integration tests

For full lookup-and-publish integration on Windows, install Bonjour.

## Useful commands

```bash
# Build
dotnet build

# List tests
dotnet test Arkane.Zeroconf.Tests --list-tests

# Coverage
dotnet-coverage collect -f cobertura -o coverage.xml dotnet test Arkane.Zeroconf.Tests
