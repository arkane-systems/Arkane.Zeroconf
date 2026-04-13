# Arkane.Zeroconf API Guide

This guide documents the public API exposed by `Arkane.Zeroconf` and provides top-level usage examples.

## Overview

`Arkane.Zeroconf` provides a provider-agnostic API for:

- browsing for Zeroconf / mDNS services
- resolving discovered services to host and port information
- publishing services
- working with TXT record metadata

The public API is designed so consumers work with the facade types in the root namespace:

- `ServiceBrowser`
- `RegisterService`
- `TxtRecord`
- `ZeroconfSupport`

## Provider behavior by platform

Arkane.Zeroconf uses a simple provider selection model:

- **Bonjour** is preferred whenever it is available.
- On **Windows 11+**, if Bonjour is unavailable, the library falls back to the **WindowsMdns** provider.
- This is a fallback mechanism, not a general-purpose configurable provider priority system.

Platform behavior:

- **Bonjour provider**: supports browsing, resolving, and publishing.
- **Windows fallback provider**: supports browsing and resolving only.
- **Linux**: Bonjour-compatible behavior relies on Avahi compatibility mode when available.

## Capability checks

Check capabilities before starting operations:

```csharp
using ArkaneSystems.Arkane.Zeroconf;

if (!ZeroconfSupport.CanBrowse)
    throw new InvalidOperationException("mDNS lookup is not available.");

if (!ZeroconfSupport.CanPublish)
    Console.WriteLine("Publishing is unavailable for the selected provider.");
```

`ZeroconfSupport.Capabilities` returns a bitwise combination of `ZeroconfCapability` values.

## Top-level examples

### Browse for services

```csharp
using ArkaneSystems.Arkane.Zeroconf;

using var browser = new ServiceBrowser();

browser.ServiceAdded += (sender, args) =>
{
    Console.WriteLine($"Found: {args.Service.Name} ({args.Service.RegType})");
};

browser.ServiceRemoved += (sender, args) =>
{
    Console.WriteLine($"Removed: {args.Service.Name}");
};

browser.Browse(regtype: "_http._tcp", domain: "local");
```

### Browse with explicit interface and protocol

```csharp
using ArkaneSystems.Arkane.Zeroconf;

using var browser = new ServiceBrowser();

browser.Browse(
    interfaceIndex: 0,
    addressProtocol: AddressProtocol.Any,
    regtype: "_workstation._tcp",
    domain: "local");
```

### Resolve a discovered service

```csharp
using ArkaneSystems.Arkane.Zeroconf;

using var browser = new ServiceBrowser();

browser.ServiceAdded += (sender, args) =>
{
    IResolvableService service = args.Service;
    service.Resolve();

    if (service.HostEntry?.AddressList != null)
    {
        Console.WriteLine($"Resolved {service.Name} to {service.HostEntry.HostName}:{service.Port}");
    }
};

browser.Browse(regtype: "_http._tcp", domain: "local");
```

### Resolve asynchronously

```csharp
using ArkaneSystems.Arkane.Zeroconf;

using var browser = new ServiceBrowser();
using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

browser.ServiceAdded += async (sender, args) =>
{
    await args.Service.ResolveAsync(cts.Token);

    if (args.Service.HostEntry?.AddressList != null)
    {
        Console.WriteLine($"Resolved {args.Service.FullName}");
    }
};

browser.Browse(regtype: "_http._tcp", domain: "local");
```

### Publish a service

```csharp
using ArkaneSystems.Arkane.Zeroconf;

if (!ZeroconfSupport.CanPublish)
    throw new PlatformNotSupportedException("Publishing is not supported by the active provider.");

using var service = new RegisterService
{
    Name = "Example Service",
    RegType = "_http._tcp",
    ReplyDomain = "local",
    Port = 8080
};

service.Response += (sender, args) =>
{
    Console.WriteLine($"Registered: {args.IsRegistered}, Error: {args.ServiceError}");
};

service.Register();
```

### Publish with a TXT record

```csharp
using ArkaneSystems.Arkane.Zeroconf;

var txtRecord = new TxtRecord();
txtRecord.Add("version", "1.0");
txtRecord.Add("path", "/api");

using var service = new RegisterService
{
    Name = "Example Service",
    RegType = "_http._tcp",
    ReplyDomain = "local",
    Port = 8080,
    TxtRecord = txtRecord
};

service.Register();
```

### Read TXT records from a resolved service

```csharp
using ArkaneSystems.Arkane.Zeroconf;

browser.ServiceAdded += (sender, args) =>
{
    args.Service.Resolve();

    ITxtRecord? record = args.Service.TxtRecord;
    if (record is { Count: > 0 })
    {
        for (int i = 0; i < record.Count; i++)
        {
            TxtRecordItem item = record.GetItemAt(i);
            Console.WriteLine($"{item.Key} = {item.ValueString}");
        }
    }
};
```

## Public API reference

### `ServiceBrowser`

Facade for browsing Zeroconf services.

Key members:

- `Browse(uint interfaceIndex, AddressProtocol addressProtocol, string regtype, string domain)`
- convenience overloads on the concrete facade class
- `ServiceAdded`
- `ServiceRemoved`
- enumeration over discovered `IResolvableService` instances

Notes:

- `domain` defaults to `"local"` in the facade overloads that accept nullable values.
- Browsing requires `ZeroconfSupport.CanBrowse == true`.

### `IResolvableService`

Represents a browsed service that can be resolved.

Key members:

- `Name`
- `RegType`
- `ReplyDomain`
- `FullName`
- `HostEntry`
- `HostTarget`
- `NetworkInterface`
- `AddressProtocol`
- `Port`
- `Resolve()`
- `ResolveAsync(CancellationToken)`
- `Resolved`

Notes:

- Some properties remain unset until resolution completes.
- On Windows fallback, resolution is supported even though publishing is not.

### `RegisterService`

Facade for publishing services.

Key members:

- `Name`
- `RegType`
- `ReplyDomain`
- `Port`
- `UPort`
- `TxtRecord`
- `Register()`
- `Response`

Notes:

- Publishing requires `ZeroconfSupport.CanPublish == true`.
- On Windows fallback, attempting to publish throws `PlatformNotSupportedException`.

### `TxtRecord`

Facade for service TXT record metadata.

Key members:

- indexer: `this[string key]`
- `Count`
- `Add(string, string)`
- `Add(string, byte[])`
- `Add(TxtRecordItem)`
- `Remove(string)`
- `GetItemAt(int)`
- enumeration support

### `TxtRecordItem`

Represents a single TXT record entry.

Key members:

- `Key`
- `ValueRaw`
- `ValueString`

## Events

### `ServiceBrowseEventHandler`

Used by:

- `ServiceBrowser.ServiceAdded`
- `ServiceBrowser.ServiceRemoved`

Event args:

- `ServiceBrowseEventArgs.Service`

### `ServiceResolvedEventHandler`

Used by:

- `IResolvableService.Resolved`

Event args:

- `ServiceResolvedEventArgs.Service`

### `RegisterServiceEventHandler`

Used by:

- `RegisterService.Response`

Event args:

- `RegisterServiceEventArgs.Service`
- `RegisterServiceEventArgs.IsRegistered`
- `RegisterServiceEventArgs.ServiceError`

## Exceptions and capability behavior

Common exception patterns:

- `ZeroconfException`
  - thrown when a facade cannot create the requested provider-backed service
- `ArgumentNullException`
  - thrown for required null arguments
- `PlatformNotSupportedException`
  - thrown when publishing is attempted with a lookup-only provider

Provider-specific behavior should be checked through `ZeroconfSupport` before starting operations.

## Windows notes

If you want full browse-and-publish behavior on Windows, install Bonjour:

```powershell
winget install Apple.Bonjour
```

If Bonjour is not available, Windows 11+ can still browse and resolve services through the fallback provider.

## Linux notes

On Linux, Bonjour-compatible support depends on Avahi compatibility mode being available.
Browsing and resolving can work through the Bonjour provider in Avahi-backed environments.

## Related documentation

- `README.md` - project overview and provider behavior
- XML documentation in the public API source files - API contract details
