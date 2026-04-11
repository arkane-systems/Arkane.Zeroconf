# Arkane.Zeroconf Copilot Instructions

## Project Overview
**Arkane.Zeroconf** is a .NET 10 library for Zero Configuration Networking (Bonjour/mDNS). It provides a cross-platform abstraction over platform-specific mDNS implementations (Windows Bonjour, macOS native, Linux Avahi).

**Key Components:**
- **Arkane.ZeroConf**: Core library with platform-agnostic APIs
- **azclient**: CLI tool demonstrating service browsing and publishing

## Architecture & Patterns

### Provider Factory Pattern
The library uses a **plugin architecture** with dynamic provider selection:
- `ProviderFactory` discovers and instantiates providers via `ZeroconfProviderAttribute`
- Providers implement `IZeroconfProvider` and expose three types: `ServiceBrowser`, `RegisterService`, `TxtRecord`
- The Bonjour provider uses P/Invoke to call native DNS-SD libraries (`DNSServiceBrowse`, `DNSServiceRegister`, etc.)
- **Critical design**: Platform support is extensible; new providers added as assembly-level attributes

**Example from code:**
```csharp
// Arkane.ZeroConf/Providers/Bonjour/ZeroconfProvider.cs
[assembly: ZeroconfProvider(typeof(ZeroconfProvider))]
```

### Public Facade Pattern
Wrapper classes like `ServiceBrowser` and `RegisterService` delegate to provider implementations:
- Use `Activator.CreateInstance()` to instantiate the concrete provider class
- Forward all method calls and events to the internal `IServiceBrowser`
- Consumers code against the facade, never the provider directly

**Default domain fallback**: Always pass `domain ?? "local"` to provider.

### Event Delegation Pattern
Events in facades must **always** delegate ADD and REMOVE to the underlying browser:
```csharp
public event ServiceBrowseEventHandler ServiceAdded
{
    add => this.browser.ServiceAdded += value;
    remove => this.browser.ServiceAdded -= value;  // Delegate to same event, not ServiceRemoved
}
```

### Thread Safety
- Use `SemaphoreSlim` (not locks) for async-safe synchronization in Bonjour provider
- `GetEnumerator()` in `ServiceBrowser` acquires semaphore before iterating service table
- Always release in finally block

## Native Interop & Bonjour

### P/Invoke Conventions
- All native calls in `Arkane.ZeroConf/Providers/Bonjour/Native.cs`
- Use `DllImport("c")` or platform-specific DLL names
- Always check returned `ServiceError` and throw `ServiceErrorException` on error:
  ```csharp
  var error = Native.DNSServiceBrowse(...);
  if (error != ServiceError.NoError)
      throw new ServiceErrorException(error);
  ```

### Platform-Specific Initialization
- **Linux**: Uses `setenv()` for `AVAHI_COMPAT_NOWARN`; skips `DNSServiceCreateConnection` (not supported)
- **macOS/Windows**: Call `DNSServiceCreateConnection` to test Bonjour availability
- Guard with `OperatingSystem.IsLinux()`, `OperatingSystem.IsMacOS()`, `OperatingSystem.IsWindows()`

### Event Callbacks
Bonjour callbacks are **async** and managed via Task loops:
- `OnBrowseReply` marshals callback results into the service table
- Callbacks use delegates like `Native.DNSServiceBrowseReply` and `Native.DNSServiceRegisterReply`
- Event handlers invoke on background threads; assume thread safety required

## Code Conventions

### Namespace Structure
```
ArkaneSystems.Arkane.Zeroconf (public APIs)
ArkaneSystems.Arkane.Zeroconf.Providers (factory, attributes)
ArkaneSystems.Arkane.Zeroconf.Providers.Bonjour (platform impl)
ArkaneSystems.Arkane.Zeroconf.Client (CLI utility)
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

### Nullable Handling
Project uses `LangVersion: latest` (C# 14 features available). Null checks:
- Use `ArgumentNullException.ThrowIfNull(param)` for public APIs
- Use `string.IsNullOrWhiteSpace(param)` for strings
- Use `?` null-coalescing for optional domains: `domain ?? "local"`

### Task Management
- `ServiceBrowser.Start(async)` creates background task for Bonjour polling
- Use `Task.Run()` wrapped in `ContinueWith()` for error propagation
- Tasks must set `this.task = null` when complete to prevent "already started" exceptions

## Testing & Build

### Build Command
```pwsh
dotnet build
```

### Project Files
- **Arkane.ZeroConf.csproj**: Targets `net10.0`, signed assembly (`zeroconf.snk`)
- **azclient.csproj**: Console app that references Arkane.ZeroConf

### API Expectations
- Consumers instantiate `ServiceBrowser()` and call `Browse(interfaceIndex, addressProtocol, regtype, domain)`
- Services are enumerable; access via `foreach` or `GetEnumerator()`
- Events: `ServiceAdded`, `ServiceRemoved` with `ServiceBrowseEventArgs` (includes `MoreComing` flag)

## Common Editing Patterns

1. **Adding a new provider**: 
   - Implement `IZeroconfProvider` 
   - Add `[assembly: ZeroconfProvider(typeof(MyProvider))]` attribute
   - Implement three Type properties returning your custom types

2. **Modifying event handling**:
   - Check both add/remove accessors delegate to the **same** event
   - Verify semaphore usage in iterators

3. **Adding native bindings**:
   - Add P/Invoke signature to `Native.cs`
   - Wrap with error checking and `ServiceErrorException`
   - Guard platform-specific calls with OS checks

## Key Files Reference
- **IServiceBrowser.cs**: Public browser interface
- **ServiceBrowser.cs**: Facade wrapping provider
- **Providers/ProviderFactory.cs**: Dynamic provider discovery & selection
- **Providers/Bonjour/ServiceBrowser.cs**: Concrete Bonjour implementation (task-based polling, semaphore-protected service table)
- **Providers/Bonjour/Native.cs**: All P/Invoke signatures
- **ZeroconfProvider.cs**: Bonjour provider registration & initialization

## Troubleshooting

- **"No Zeroconf providers could be found"**: Bonjour daemon not running (Windows/macOS) or Avahi not installed (Linux)
- **ServiceBrowser already started**: Don't call `Browse()` twice without disposing first
- **Missing events**: Verify event delegate delegates to correct underlying event (common copy-paste error)
