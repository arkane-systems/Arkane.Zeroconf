#region header

// Arkane.Zeroconf.Tests - PlatformSpecificTests.cs

#endregion

#region using

using System;
using System.Runtime.InteropServices;

using ArkaneSystems.Arkane.Zeroconf.Providers.SystemdResolved;
using ArkaneSystems.Arkane.Zeroconf.Tests.Helpers;

using Xunit;

#endregion

namespace ArkaneSystems.Arkane.Zeroconf.Tests.Platform;

/// <summary>
///   Platform-specific tests for native mDNS availability and initialization.
///   Tests verify platform detection and that at least one provider is functional.
/// </summary>
public class PlatformSpecificTests
{
  [Fact]
  public void CurrentPlatform_IsSupported ()
  {
    // Assert - at least one platform should be active
    bool isWindows = OperatingSystem.IsWindows ();
    bool isMacOS   = OperatingSystem.IsMacOS ();
    bool isLinux   = OperatingSystem.IsLinux ();

    Assert.True (condition: isWindows || isMacOS || isLinux, userMessage: "Should be running on a supported platform");
  }

  [Fact]
  public void NativeInitialization_Succeeds ()
  {
    if (!ZeroconfSupport.IsAvailable)
      Assert.Skip ("No Zeroconf provider available on this system. " +
                   "Install Avahi or enable systemd-resolved mDNS (Linux), Bonjour (Windows/macOS), or run on Windows 11+ for the Windows mDNS fallback.");

    try
    {
      using var browser = new ServiceBrowser ();
    }
    catch (InvalidOperationException ex)
    {
      Assert.Fail ($"Native zeroconf initialization failed. " +
                   $"Ensure a platform dependency is installed and running " +
                   $"(Windows/macOS: Bonjour; Linux: Avahi or systemd-resolved with MulticastDNS=yes). " +
                   $"Details: {ex.Message}");
    }
    catch (DllNotFoundException ex)
    {
      Assert.Fail ($"Native zeroconf library was not found. " +
                   $"Install a platform dependency (Windows/macOS: Bonjour; Linux: Avahi). " +
                   $"Details: {ex.Message}");
    }
    catch (EntryPointNotFoundException ex)
    {
      Assert.Fail ($"Native zeroconf entry point was not found. " +
                   $"Verify a compatible platform dependency installation (Windows/macOS: Bonjour; Linux: Avahi). " +
                   $"Details: {ex.Message}");
    }
  }

  [Fact]
  public void WindowsMDNS_IsAvailable ()
  {
    if (!OperatingSystem.IsWindows ())
      return;

    Assert.True (condition: ZeroconfSupport.CanBrowse,
                 userMessage: "Windows zeroconf browsing is unavailable. " +
                              "Install Bonjour, or run on Windows 11+ where the built-in mDNS fallback is supported.");
  }

  [Fact]
  public void MacOSMDNS_IsAvailable ()
  {
    if (!OperatingSystem.IsMacOS ())
      return;

    Assert.True (condition: ZeroconfSupport.CanBrowse,
                 userMessage: "macOS zeroconf browsing is unavailable. Ensure mDNSResponder is running.");
  }

  [Fact]
  public void LinuxMDNS_IsAvailable ()
  {
    if (!OperatingSystem.IsLinux ())
      return;

    if (!ZeroconfSupport.IsAvailable)
      Assert.Skip ("No mDNS provider is available on this Linux system. " +
                   "Install libavahi-compat-libdnssd1 and start the Avahi daemon, " +
                   "or enable MulticastDNS in /etc/systemd/resolved.conf.");

    Assert.True (condition: ZeroconfSupport.CanBrowse,
                 userMessage: "Linux mDNS browsing is unavailable despite a provider being detected.");
  }

  [Fact]
  public void LinuxSystemdResolved_IsAvailable ()
  {
    if (!OperatingSystem.IsLinux ())
      return;

    if (!new ZeroconfProvider ().IsAvailable ())
      Assert.Skip ("systemd-resolved mDNS is not available on this Linux system. " +
                   "Enable MulticastDNS in /etc/systemd/resolved.conf to activate this provider.");

    // systemd-resolved is available; the active provider may be Avahi (preferred) or
    // systemd-resolved itself — either way, browsing must be supported.
    Assert.True (condition: ZeroconfSupport.CanBrowse,
                 userMessage: "systemd-resolved reports mDNS available, but browsing is not supported by the active provider.");
  }

  [Theory]
  [InlineData (true,  false, false, "Windows")]
  [InlineData (false, true,  false, "macOS")]
  [InlineData (false, false, true,  "Linux")]
  public void Platform_Correctly_Identified (bool isWin, bool isMac, bool isLinux, string expectedName)
  {
    // Arrange
    bool actualIsWindows = OperatingSystem.IsWindows ();
    bool actualIsMacOS   = OperatingSystem.IsMacOS ();
    bool actualIsLinux   = OperatingSystem.IsLinux ();

    bool rowMatchesCurrentPlatform = (actualIsWindows && isWin) ||
                                     (actualIsMacOS   && isMac) ||
                                     (actualIsLinux   && isLinux);

    if (!rowMatchesCurrentPlatform)
      return;

    Assert.Equal (expected: expectedName,
                  actual: actualIsWindows
                            ? "Windows"
                            : actualIsMacOS
                              ? "macOS"
                              : "Linux");
  }

  [Fact]
  public void RuntimeInformation_IsAvailable ()
  {
    // Act
    string description = RuntimeInformation.RuntimeIdentifier;

    // Assert
    Assert.NotEmpty (description);
  }
}

