# Arkane.Zeroconf Testing Guide

## Overview

This document provides a comprehensive guide to testing the Arkane.Zeroconf library. The test suite includes unit tests, integration tests, platform-specific tests, end-to-end tests, and performance tests.

## Test Projects

### Arkane.Zeroconf.Tests

The main test project targeting .NET 10.0 with the following test categories:

#### 1. **Facade Tests** (`Facades/`)
- `ServiceBrowserTests.cs` - Tests for ServiceBrowser wrapper
- `RegisterServiceTests.cs` - Tests for RegisterService wrapper
- `TxtRecordTests.cs` - Tests for TxtRecord wrapper
- `ZeroconfSupportTests.cs` - Tests for capability checks (`CanBrowse`, `CanPublish`)

**Purpose**: Verify that public facade classes correctly delegate to provider implementations, expose provider capability checks, and handle parameter validation.

#### 2. **Provider Tests** (`Providers/`)
- `ProviderFactoryTests.cs` - Tests for provider discovery and selection
- `WindowsMdnsProviderTests.cs` - Tests for Windows fallback provider capability and lookup-only publish behavior

**Purpose**: Verify provider discovery and runtime fallback behavior.

#### 3. **Integration Tests** (`Integration/`)
- `BonjourBrowserIntegrationTests.cs` - Tests for Bonjour service browsing
- `BonjourRegistrationIntegrationTests.cs` - Tests for Bonjour service registration
- `WindowsMdnsBrowserIntegrationTests.cs` - Tests Windows DNS-SD fallback service discovery

**Purpose**: Verify daemon/API integration for both Bonjour and Windows DNS-SD fallback paths.

**Requires**:
- **Bonjour integration tests**:
  - **Windows**: Bonjour service (Apple Bonjour for Windows)
  - **macOS**: Built-in mDNS support
  - **Linux**: Avahi daemon
- **WindowsMdns integration test**:
  - Windows 11+ host
  - At least one discoverable mDNS service on the local network

`WindowsMdnsBrowserIntegrationTests` probes multiple common service types in parallel and passes if at least one returns a service. Override probe types with:

```bash
# PowerShell
$env:ARKANE_ZEROCONF_TEST_SERVICE_TYPES = "_http._tcp,_ipp._tcp,_printer._tcp"
```

#### 4. **Platform-Specific Tests** (`Platform/`)
- `PlatformSpecificTests.cs` - Tests for platform detection and native interop

#### 5. **End-to-End Tests** (`E2E/`)
- `AzClientE2ETests.cs` - Tests using the azclient CLI tool

#### 6. **Performance Tests** (`Performance/`)
- `PerformanceAndStressTests.cs` - Performance and stress tests

## Running Tests

### Prerequisites

#### For unit/facade/provider tests
```bash
dotnet test Arkane.Zeroconf.Tests --filter "FullyQualifiedName!~Integration"
```

#### For Windows DNS-SD fallback integration test

```bash
dotnet test Arkane.Zeroconf.Tests --filter "FullyQualifiedName~WindowsMdnsBrowserIntegrationTests"
```

Optional service type override:

```bash
# PowerShell
$env:ARKANE_ZEROCONF_TEST_SERVICE_TYPES = "_http._tcp;_ssh._tcp"
dotnet test Arkane.Zeroconf.Tests --filter "FullyQualifiedName~WindowsMdnsBrowserIntegrationTests"
```

#### For Bonjour integration tests
You need an mDNS daemon running:

**Windows**:
1. Download Bonjour from Apple (Bonjour for Windows)
2. Install the service
3. Verify the "Bonjour Service" is running in Services

**macOS**:
- Built-in support, no installation needed

**Linux**:
```bash
# Ubuntu/Debian
sudo apt-get install avahi-daemon

# Fedora/RHEL
sudo dnf install avahi

# Start the daemon
sudo systemctl start avahi-daemon
```

### Common commands

```bash
# Run all tests
dotnet test Arkane.Zeroconf.Tests

# Run only integration tests
dotnet test Arkane.Zeroconf.Tests --filter "Arkane.Zeroconf.Tests.Integration"

# Run only Windows fallback integration test
dotnet test Arkane.Zeroconf.Tests --filter "FullyQualifiedName~WindowsMdnsBrowserIntegrationTests"

# Run with detailed output
dotnet test Arkane.Zeroconf.Tests -v n

# Run with code coverage
dotnet-coverage collect -f cobertura -o coverage.cobertura.xml dotnet test Arkane.Zeroconf.Tests
```

## Troubleshooting

### Windows fallback integration test fails quickly with no discoveries
- Ensure network has discoverable mDNS services.
- Override `ARKANE_ZEROCONF_TEST_SERVICE_TYPES` to match your environment.
- Check local firewall prompts/allow rules for test process network access.

### "No Zeroconf providers could be found"
- Bonjour unavailable and no supported fallback available for current OS.
- On Windows, fallback lookup requires Windows 11+.

### Publishing fails on Windows fallback provider
- Expected behavior (`PlatformNotSupportedException`).
- Check `ZeroconfSupport.CanPublish` before attempting publish.
