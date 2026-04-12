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

**Purpose**: Verify that public facade classes correctly delegate to provider implementations and handle parameter validation.

**Coverage**:
- Property getters/setters
- Method delegation
- Event subscription/unsubscription
- Enumeration
- Disposal

#### 2. **Provider Tests** (`Providers/`)
- `ProviderFactoryTests.cs` - Tests for provider discovery and selection

**Purpose**: Verify that the provider factory correctly discovers and instantiates providers.

**Coverage**:
- Provider discovery
- Default provider selection
- Provider override capability
- Type validation

#### 3. **Value Object Tests** (`ValueObjects/`)
- `ValueObjectTests.cs` - Tests for enums and record types

**Purpose**: Verify immutable types and value objects behave correctly.

**Coverage**:
- Enum values (AddressProtocol, ServiceFlags)
- ServiceType and ServiceClass
- TxtRecordItem
- ServiceError and ServiceErrorException

#### 4. **Integration Tests** (`Integration/`)
- `BonjourBrowserIntegrationTests.cs` - Tests for service browsing
- `BonjourRegistrationIntegrationTests.cs` - Tests for service registration

**Purpose**: Verify that the library correctly interacts with the Bonjour/mDNS daemon.

**Requires**: Bonjour daemon installation:
- **Windows**: Bonjour service (Apple Bonjour for Windows)
- **macOS**: Built-in (no installation required)
- **Linux**: Avahi daemon

**Coverage**:
- Service discovery
- Event firing
- Service enumeration
- Service registration
- Property persistence

#### 5. **Platform-Specific Tests** (`Platform/`)
- `PlatformSpecificTests.cs` - Tests for platform detection and native interop

**Purpose**: Verify platform-specific behavior and daemon availability.

**Coverage**:
- Platform detection (Windows/macOS/Linux)
- Native initialization
- Platform-specific configurations

#### 6. **End-to-End Tests** (`E2E/`)
- `AzClientE2ETests.cs` - Tests using the azclient CLI tool

**Purpose**: Verify the library works correctly through the CLI interface.

**Requires**: Built azclient executable and mDNS daemon

**Coverage**:
- CLI argument parsing
- Service browsing via CLI
- Protocol filtering
- Help/error messages

#### 7. **Performance Tests** (`Performance/`)
- `PerformanceAndStressTests.cs` - Performance and stress tests

**Purpose**: Verify the library performs well under load and doesn't leak resources.

**Coverage**:
- TxtRecord performance with many items
- ServiceBrowser enumeration efficiency
- Multiple instance creation
- Event subscription under load
- Concurrent operations
- Property access performance

## Running Tests

### Prerequisites

#### For Unit & Facade Tests (No special requirements)
```bash
dotnet test Arkane.Zeroconf.Tests
```

#### For Integration Tests
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

#### For E2E Tests
1. Build azclient first:
```bash
dotnet build azclient
```

2. mDNS daemon should be running (same as integration tests)

### Running Specific Test Categories

```bash
# Run all tests
dotnet test Arkane.Zeroconf.Tests

# Run only facade tests
dotnet test Arkane.Zeroconf.Tests --filter "Category=Facades"

# Run only integration tests
dotnet test Arkane.Zeroconf.Tests --filter "Arkane.Zeroconf.Tests.Integration"

# Run only performance tests
dotnet test Arkane.Zeroconf.Tests --filter "Arkane.Zeroconf.Tests.Performance"

# Run specific test class
dotnet test Arkane.Zeroconf.Tests --filter "FullyQualifiedName~ServiceBrowserTests"

# Run with detailed output
dotnet test Arkane.Zeroconf.Tests -v n

# Run with code coverage
dotnet-coverage collect -f cobertura -o coverage.cobertura.xml dotnet test Arkane.Zeroconf.Tests
```

### Visual Studio Test Explorer

In Visual Studio:
1. Open Test Explorer (Test → Test Explorer or Ctrl+E, T)
2. Build the solution
3. Tests will appear in Test Explorer
4. Right-click tests to run, debug, or run with coverage
5. Use filter field to find specific tests

## Test Results Interpretation

### Skipped Tests

Some tests are marked with `[Fact(Skip = "...")]`:

- **Platform-Specific Tests** - Skipped by default as they test platform-specific behavior
- **E2E Tests** - Skipped by default as they require azclient to be built
- **Daemon-Dependent Tests** - May be skipped if mDNS daemon not available

To run skipped tests:
1. Remove the `Skip` attribute
2. Ensure prerequisites are met
3. Run the test

### Expected Failures

Integration tests may fail if:
- mDNS daemon not installed or running
- Firewall blocking mDNS traffic (UDP port 5353)
- No services are currently advertised on the network

This is expected behavior on systems without proper mDNS configuration.

## Manual Testing with azclient

### Browse Services

```bash
# Browse all workstations
./azclient -t _workstation._tcp

# Browse HTTP services with verbose output
./azclient -t _http._tcp -v

# Browse with IPv4 only
./azclient -t _http._tcp -a ipv4

# Browse with IPv6 only
./azclient -t _http._tcp -a ipv6

# Browse on specific interface
./azclient -t _http._tcp -i 1

# Browse with resolve
./azclient -t _http._tcp -r

# Publish a service
./azclient -p "My Service:_http._tcp:8080"
```

### Common Service Types

- `_http._tcp` - HTTP services
- `_https._tcp` - HTTPS services
- `_ssh._tcp` - SSH services
- `_workstation._tcp` - Network workstations
- `_sftp-ssh._tcp` - SFTP services
- `_device-info._tcp` - Device information

## System mDNS Tools

### Windows

#### Using nslookup
```bash
nslookup -type=PTR _http._tcp.local
```

#### Using mdns-repeater (if available)
```bash
mdns-repeater eth0
```

### macOS

#### Using dns-sd (built-in)
```bash
# Browse services
dns-sd -B _http._tcp local

# List services
dns-sd -G ipv4

# Get service info
dns-sd -L "My Service" _http._tcp local
```

### Linux

#### Using avahi-browse
```bash
# Browse all services
avahi-browse -a

# Browse specific type
avahi-browse -t _http._tcp

# Verbose output
avahi-browse -t _http._tcp -v

# Include terminated services
avahi-browse -t _http._tcp -t
```

#### Using avahi-publish (publish services)
```bash
# Publish a service
avahi-publish -s "My Service" _http._tcp 8080

# Publish with TXT records
avahi-publish -s "My Service" _http._tcp 8080 -H myhost "path=/api"
```

## Troubleshooting

### Tests timeout
- **Cause**: mDNS daemon not responding
- **Solution**: Check daemon is running, restart it, or skip daemon-dependent tests

### "No Zeroconf providers could be found"
- **Cause**: No compatible mDNS provider found
- **Solution**: Install Bonjour (Windows/macOS) or Avahi (Linux)

### Services not discovered
- **Cause**: No services advertised or firewall blocking
- **Solution**: Check network connectivity, firewall rules, or advertise test services

### High memory usage in performance tests
- This is expected behavior. Tests create many instances intentionally.
- Memory is released after test completes

### Permission denied errors on Linux
- **Cause**: Avahi daemon requires specific permissions
- **Solution**: Add user to avahi group or run with sudo

```bash
sudo usermod -a -G avahi-autoipd $USER
# Log out and log back in
```

## CI/CD Integration

### GitHub Actions Example

```yaml
name: Tests

on: [push, pull_request]

jobs:
  test:
    runs-on: ${{ matrix.os }}
    strategy:
      matrix:
        os: [ubuntu-latest, windows-latest, macos-latest]

    steps:
      - uses: actions/checkout@v3

      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '10.0.x'

      - name: Install Avahi (Linux)
        if: runner.os == 'Linux'
        run: |
          sudo apt-get update
          sudo apt-get install -y avahi-daemon
          sudo systemctl start avahi-daemon

      - name: Build
        run: dotnet build

      - name: Run Tests
        run: dotnet test --verbosity normal

      - name: Upload Coverage
        uses: codecov/codecov-action@v3
        with:
          files: ./coverage.cobertura.xml
```

## Code Coverage

Generate coverage report:

```bash
dotnet-coverage collect -f cobertura -o coverage.cobertura.xml dotnet test
```

Then view with:
- ReportGenerator: `dotnet tool install dotnet-reportgenerator-globaltool` then `reportgenerator`
- Codecov.io: Push to GitHub for automated coverage reports
- SonarCloud: Integrate with CI/CD

## Best Practices

1. **Run full test suite before committing**: `dotnet test`
2. **Enable daemon before integration testing**: Ensure mDNS daemon is running
3. **Check CI results**: Always verify tests pass on all platforms
4. **Use verbose mode for debugging**: `dotnet test -v d`
5. **Profile before optimizing**: Use performance tests as baseline
6. **Keep tests isolated**: Each test should be independent
7. **Clean up resources**: Always dispose IDisposable objects
8. **Mock external dependencies**: Don't rely on network in unit tests

## Adding New Tests

When adding new functionality:

1. **Create test file** in appropriate folder:
   - `Facades/` for public wrapper classes
   - `Providers/` for provider implementation
   - `Integration/` for mDNS daemon interaction
   - `Performance/` for performance-sensitive code

2. **Follow naming convention**: `[Feature]Tests.cs`

3. **Use xUnit**: Project uses xUnit, follow existing patterns

4. **Add XML documentation**: Describe test purpose

5. **Handle platform differences**: Use `OperatingSystem.IsXxx()` guards

6. **Consider resource cleanup**: Use `IDisposable` in test class if needed

Example:

```csharp
[Theory]
[InlineData("value1")]
[InlineData("value2")]
public void MyNewFeature_WithValue_DoesExpectedThing(string value)
{
    // Arrange
    var instance = new MyClass();

    // Act
    var result = instance.MyMethod(value);

    // Assert
    Assert.Equal(expected, result);

    instance.Dispose();
}
```

## References

- [Bonjour Specification](https://tools.ietf.org/html/rfc6762)
- [mDNS Specification](https://tools.ietf.org/html/rfc6763)
- [xUnit Documentation](https://xunit.net/)
- [Avahi Documentation](http://avahi.org/)
