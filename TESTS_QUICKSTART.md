# Quick Start: Running Tests for Arkane.Zeroconf

## TL;DR

```bash
# Run full suite
dotnet test Arkane.Zeroconf.Tests

# Run non-integration tests only
dotnet test Arkane.Zeroconf.Tests --filter "FullyQualifiedName!~Integration"

# Run Windows fallback integration test only
dotnet test Arkane.Zeroconf.Tests --filter "FullyQualifiedName~WindowsMdnsBrowserIntegrationTests"

# Run systemd-resolved provider tests (Linux; sudo for publish paths)
sudo dotnet test Arkane.Zeroconf.Tests --filter "FullyQualifiedName~SystemdResolvedProviderTests"
```

## Key behavior

- On Windows 11+, if Bonjour is unavailable, the library can still browse/resolve via the `WindowsMdns` lookup-only provider.
- On Linux, if Avahi is unavailable, the library falls back to the `SystemdResolved` provider via D-Bus.
- Publishing in fallback mode may be unavailable. Use `ZeroconfSupport.CanPublish` before publish operations.
- On Linux with `SystemdResolved`, publishing requires polkit authorization; run with `sudo` or grant the polkit policy.

## Windows fallback integration test

`WindowsMdnsBrowserIntegrationTests` probes multiple common service types and passes if at least one service is discovered.

Override service types for your network:

```powershell
$env:ARKANE_ZEROCONF_TEST_SERVICE_TYPES = "_http._tcp,_ipp._tcp,_printer._tcp"
dotnet test Arkane.Zeroconf.Tests --filter "FullyQualifiedName~WindowsMdnsBrowserIntegrationTests"
```

## systemd-resolved provider tests (Linux)

`SystemdResolvedProviderTests` mirrors the Windows provider test structure.  Tests that require publish authorization are automatically skipped when polkit denies the call.  Run with `sudo` to exercise the full publish path.

**Note**: After a `sudo` run, restore obj/ permissions if needed:

```bash
sudo find . -path '*/obj/*' | xargs sudo chown $USER
```

## Bonjour integration tests

For full lookup-and-publish integration on Windows or Linux (Avahi), install Bonjour/Avahi.

## Useful commands

```bash
# Build
dotnet build

# List tests
dotnet test Arkane.Zeroconf.Tests --list-tests

# Coverage
dotnet-coverage collect -f cobertura -o coverage.xml dotnet test Arkane.Zeroconf.Tests
```
