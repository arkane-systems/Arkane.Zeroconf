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
- Provider selection uses priority and availability probing

**Current provider behavior:**
- **Bonjour provider**: preferred when available; supports browse + publish
- **WindowsMdns provider**: fallback on Windows 11+ when Bonjour is unavailable; supports browse/resolve only

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

## Code Conventions

### Namespace Structure
```
ArkaneSystems.Arkane.Zeroconf
ArkaneSystems.Arkane.Zeroconf.Providers
ArkaneSystems.Arkane.Zeroconf.Providers.Bonjour
ArkaneSystems.Arkane.Zeroconf.Providers.WindowsMdns
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

### Nullability and guards
- Use `ArgumentNullException.ThrowIfNull(param)`
- Use `ArgumentException.ThrowIfNullOrWhiteSpace(param)` for required strings
- Use `string.IsNullOrWhiteSpace(param)` for checks

## Testing & Build

### Build command
```pwsh
dotnet build
```

### Key test classes
- `Facades/ZeroconfSupportTests.cs`
- `Providers/WindowsMdnsProviderTests.cs`
- `Integration/WindowsMdnsBrowserIntegrationTests.cs`

### WindowsMdns integration test behavior
`WindowsMdnsBrowserIntegrationTests` probes multiple common service types in parallel and passes when at least one service is discovered.

You can override service probe types with:
```powershell
$env:ARKANE_ZEROCONF_TEST_SERVICE_TYPES = "_http._tcp,_ipp._tcp,_printer._tcp"
```

## Common editing patterns

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

## Key files reference
- `Providers/ProviderFactory.cs`
- `ZeroconfSupport.cs`
- `Providers/Bonjour/ZeroconfProvider.cs`
- `Providers/WindowsMdns/ZeroconfProvider.cs`
- `Providers/WindowsMdns/NativeWindowsDns.cs`
- `Providers/WindowsMdns/MdnsClient.cs`
- `Integration/WindowsMdnsBrowserIntegrationTests.cs`

## Troubleshooting

- **"No Zeroconf providers could be found"**
  - No available provider initialized on current OS/runtime.

- **Windows lookup works but publish fails**
  - Expected when fallback `WindowsMdns` provider is active; check `ZeroconfSupport.CanPublish`.

- **Windows fallback integration test discovers nothing**
  - Ensure mDNS-advertised services exist on the network.
  - Override `ARKANE_ZEROCONF_TEST_SERVICE_TYPES` to match local environment.
