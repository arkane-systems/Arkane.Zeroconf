# Arkane.Zeroconf
A Zero Configuration Networking library for .NET.

[![Build status](https://ci.appveyor.com/api/projects/status/2f9y557cd7k5o760?svg=true)](https://ci.appveyor.com/project/cerebrate/arkane-zeroconf)

[![ko-fi](https://www.ko-fi.com/img/githubbutton_sm.svg)](https://ko-fi.com/I3I1VA18)

Basically, this is my forked version of Mono.Zeroconf for my .NET projects, which exists simply because I needed a version with various fixes that have been submitted in pull requests over the last few years, and which the release version of that package doesn't have.  API, etc., are identical to that of Mono.Zeroconf except for the different namespace.

## Provider behavior by platform

- **Bonjour provider** (preferred): lookup and publish support.
- **Windows fallback provider** (`WindowsMdns`): used when Bonjour is unavailable on Windows 11+, and provides **lookup-only** behavior using Windows DNS-SD APIs (`DnsServiceBrowse`/`DnsServiceResolve`).

## Capability checks

Use the capability API before starting operations:

```csharp
if (!ZeroconfSupport.CanBrowse)
    throw new InvalidOperationException("mDNS lookup is not available.");

if (!ZeroconfSupport.CanPublish)
    Console.WriteLine("Publishing is unavailable for the selected provider.");
```

Publishing with the Windows fallback provider throws `PlatformNotSupportedException` by design.

## Windows notes

For full lookup-and-publish behavior on Windows, install Bonjour:

    winget install Apple.Bonjour

When Bonjour is not installed but the OS supports Windows DNS-SD lookup (Windows 11+), browsing and resolving continue to work through the fallback provider.

As per the original readme:

More information about Mono.Zeroconf can be found on its project page 
on the Mono Wiki:

   http://mono-project.com/Mono.Zeroconf
   
General information about Zero Configuration Networking: 

   http://www.zeroconf.org/
   
 Authorship of the majority of the code remains with Aaron Bockover <abockover@novell.com> .
