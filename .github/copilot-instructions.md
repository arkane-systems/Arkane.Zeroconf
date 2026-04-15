# Arkane.Zeroconf Copilot Instructions

## Project Overview
**Arkane.Zeroconf** is a .NET 10 library for Zero Configuration Networking (Bonjour/mDNS). It provides a cross-platform abstraction over platform-specific implementations.

**Key Components:**
- **Arkane.ZeroConf**: Core library with platform-agnostic APIs
- **azclient**: CLI tool demonstrating service browsing and publishing

## Architecture & Patterns

### Provider Factory Pattern
The library uses a **plugin architecture** with dynamic provider selection:
- `ProviderFactory` discovers and instantiates providers via `ZeroconfProviderAttribute`
- Providers implement `IZeroconfProvider` and expose three types: `ServiceBrowser`, `RegisterService`, `TxtRecord`
- Providers now expose capabilities and availability checks (`Capabilities`, `IsAvailable()`)
- Provider selection treats the Windows fallback system as a simple mechanism rather than a general configurable provider-priority system; do not add configurable priorities unless explicitly requested.

**Current provider behavior:**
- **Bonjour provider**: preferred when available; supports browse + publish; priority 100
- **SystemdResolved provider**: fallback on Linux when Avahi is unavailable; supports browse + resolve always; publish when `MulticastDNS=yes` AND polkit authorized; priority 50
- **WindowsMdns provider**: fallback on Windows 11+ when Bonjour is unavailable; supports browse/resolve only; priority 10

### Public Facade Pattern
Wrapper classes like `ServiceBrowser` and `RegisterService` delegate to provider implementations:
- Use `Activator.CreateInstance()` to instantiate concrete provider classes
- Forward calls/events to the internal implementation
- Consumers code against facades, not provider internals

**Default domain fallback**: pass `domain ?? "local"` to provider.

### Capability API Pattern
Use `ZeroconfSupport` before operations:
- `ZeroconfSupport.Capabilities`
- `ZeroconfSupport.CanBrowse`
- `ZeroconfSupport.CanPublish`

Publishing with lookup-only providers is expected to throw `PlatformNotSupportedException`.
Publishing with the `SystemdResolved` provider also throws `PlatformNotSupportedException` when polkit denies the D-Bus call; check `ZeroconfSupport.CanPublish`.

## Windows fallback implementation

### WindowsMdns provider
- Namespace: `ArkaneSystems.Arkane.Zeroconf.Providers.WindowsMdns`
- Uses Windows DNS-SD APIs in `dnsapi.dll`:
  - `DnsServiceBrowse`
  - `DnsServiceResolve`
- Interop contracts are in `Providers/WindowsMdns/NativeWindowsDns.cs`
- Lookup flow is implemented in `Providers/WindowsMdns/MdnsClient.cs`

### Important constraints
- Keep Windows fallback **lookup-only**; no publishing support
- Preserve facade API compatibility
- Ensure native resources are always freed in callback/finally paths (`DnsRecordListFree`, `DnsServiceFreeInstance`)
- Keep OS checks for fallback availability (`OperatingSystem.IsWindowsVersionAtLeast(10,0,22000)`)

## Linux fallback implementation

### SystemdResolved provider
- Namespace: `ArkaneSystems.Arkane.Zeroconf.Providers.SystemdResolved`
- Uses D-Bus via `Tmds.DBus.Protocol` NuGet package to call `org.freedesktop.resolve1.Manager`:
  - `ResolveRecord` — PTR queries for browse emulation
  - `ResolveService` — SRV/address/TXT resolution
  - `RegisterService` / `UnregisterService` — service publishing
- D-Bus proxy and property types are in `Providers/SystemdResolved/ResolveManagerProxy.cs`
- Core client logic is in `Providers/SystemdResolved/SystemdResolvedClient.cs`

### SystemdResolved provider availability
- `IsAvailable()` creates a temporary D-Bus connection and reads the `MulticastDNS` property
- Returns `false` when not on Linux, systemd-resolved is unreachable, or `MulticastDNS="no"`
- `Initialize()` sets capabilities: `Browse` always; adds `Publish` when `MulticastDNS="yes"` **and** `ProbePublishAuthorized()` succeeds

### Publish authorization probe
- `ProbePublishAuthorized()` attempts a dummy `RegisterService` D-Bus call; polkit denial
  (error names: `InteractiveAuthorizationRequired`, `AccessDenied`, `AuthFailed`, `NotSupported`)
  causes downgrade to browse-only capability
- Running with `sudo` or granting a polkit rule for `org.freedesktop.resolve1` enables publish

### BrowseService.Resolve() pattern
- `Resolve()` = fire-and-forget: `_ = Task.Run(() => ResolveAsync())`
- `ResolveAsync()` only rethrows `OperationCanceledException` when the caller's token is cancelled;
  internal D-Bus timeouts (`TaskCanceledException` with default token) are swallowed and logged

### Important constraints
- Keep Linux fallback **browse-only by default**; publish capability depends on polkit at runtime
- Preserve facade API compatibility
- D-Bus errors from `DBusErrorReplyException` (not the obsolete `DBusException`)
- `SystemdResolvedClient.BrowseTimeout` and `ResolveTimeout` are public static settable (used in tests)
- Keep OS check: `OperatingSystem.IsLinux()`

## Code Conventions

### Namespace Structure
```
ArkaneSystems.Arkane.Zeroconf
ArkaneSystems.Arkane.Zeroconf.Providers
ArkaneSystems.Arkane.Zeroconf.Providers.Bonjour
ArkaneSystems.Arkane.Zeroconf.Providers.WindowsMdns
ArkaneSystems.Arkane.Zeroconf.Providers.SystemdResolved
ArkaneSystems.Arkane.Zeroconf.Client
```

### File Headers
Every file includes a header block:
```csharp
#region header
// Arkane.ZeroConf - FileName.cs
#endregion

#region using
using System;
#endregion
```

### Nullability and Guards
- Use `ArgumentNullException.ThrowIfNull(param)`
- Use `ArgumentException.ThrowIfNullOrWhiteSpace(param)` for required strings
- Use `string.IsNullOrWhiteSpace(param)` for checks
- For nullability migrations, prioritize contract correctness even if nullable metadata changes require consumers to update code, as long as underlying API behavior/contracts do not change.

### Interop Constants
- For Bonjour interop enums like `ServiceType` and `ServiceClass`, document them as interop constants.
- Keep unused `ServiceType` members for consistency/completeness; if they are not part of the intended public API, prefer making them internal.

## Testing & Build

### Build command
```pwsh
dotnet build
```

### Key test classes
- `Facades/ZeroconfSupportTests.cs`
- `Providers/WindowsMdnsProviderTests.cs`
- `Providers/SystemdResolvedProviderTests.cs`
- `Integration/WindowsMdnsBrowserIntegrationTests.cs`
- `Platform/PlatformSpecificTests.cs`

### SystemdResolved provider tests behavior
`SystemdResolvedProviderTests` mirrors the `WindowsMdnsProviderTests` structure.
- All tests skip when `new ZeroconfProvider().IsAvailable()` returns `false`.
- Publish-path tests skip when polkit denies authorization (i.e. non-sudo run).
- Run with `sudo` to exercise the full publish path.
- `BrowseService_ResolveAsync_WithUnknownService` shortens `ResolveTimeout` to 300ms to avoid blocking.

### WindowsMdns integration test behavior
`WindowsMdnsBrowserIntegrationTests` probes multiple common service types in parallel and passes when at least one service is discovered.

You can override service probe types with:
```powershell
$env:ARKANE_ZEROCONF_TEST_SERVICE_TYPES = "_http._tcp,_ipp._tcp,_printer._tcp"
```

### sudo and obj/ permissions
Running `dotnet test` under `sudo` leaves obj/ cache files owned by root.
Fix: `sudo find . -path '*/obj/*' | xargs sudo chown $USER`

## Common Editing Patterns

1. **Adding/updating a provider**
   - Implement `IZeroconfProvider`
   - Register with `[assembly: ZeroconfProvider(typeof(MyProvider), priority: ...)]`
   - Implement capability/availability members

2. **Updating native interop**
   - Keep signatures in provider-native interop file
   - Pair every returned native allocation with matching free API
   - Avoid swallowing interop failures silently

3. **Updating tests**
   - Prefer targeted test runs first (`FullyQualifiedName~...`)
   - Keep network-dependent tests explicit about environmental prerequisites

## Documentation Guidelines
- For issue work on documentation, use a two-phase process:
  - First, add comprehensive XML documentation to the public API.
  - Then, produce comprehensive Markdown API documentation linked from the README.
- Document platform-specific behavior in both places where appropriate.

## Key Files Reference
- `Providers/ProviderFactory.cs`
- `ZeroconfSupport.cs`
- `Providers/Bonjour/ZeroconfProvider.cs`
- `Providers/WindowsMdns/ZeroconfProvider.cs`
- `Providers/WindowsMdns/NativeWindowsDns.cs`
- `Providers/WindowsMdns/MdnsClient.cs`
- `Providers/SystemdResolved/ZeroconfProvider.cs`
- `Providers/SystemdResolved/ResolveManagerProxy.cs`
- `Providers/SystemdResolved/SystemdResolvedClient.cs`
- `Providers/SystemdResolved/BrowseService.cs`
- `Providers/SystemdResolved/ServiceBrowser.cs`
- `Providers/SystemdResolved/RegisterService.cs`
- `Arkane.Zeroconf.Tests/Providers/SystemdResolvedProviderTests.cs`
- `Arkane.Zeroconf.Tests/Platform/PlatformSpecificTests.cs`
- `Integration/WindowsMdnsBrowserIntegrationTests.cs`

## Troubleshooting

- **"No Zeroconf providers could be found"**
  - No available provider initialized on current OS/runtime.
  - On Linux: ensure Avahi is installed, or systemd-resolved is running with `MulticastDNS` ≠ `no`.

- **Windows lookup works but publish fails**
  - Expected when fallback `WindowsMdns` provider is active; check `ZeroconfSupport.CanPublish`.

- **Linux lookup works but publish fails**
  - Expected when systemd-resolved provider is active and polkit denies authorization.
  - Check `ZeroconfSupport.CanPublish`; run with `sudo` or configure a polkit rule.

- **Windows fallback integration test discovers nothing**
  - Ensure mDNS-advertised services exist on the network.
  - Override `ARKANE_ZEROCONF_TEST_SERVICE_TYPES` to match local environment.

- **SystemdResolved tests are all skipped**
  - systemd-resolved may not be running, or `MulticastDNS` is set to `no`.
  - Check: `systemctl status systemd-resolved` and `resolvectl status | grep -i multicast`.
