#region header

// Arkane.Zeroconf.Tests - PlatformSpecificTests.cs

#endregion

#region using

using System;
using System.Runtime.InteropServices;

using ArkaneSystems.Arkane.Zeroconf.Tests.Helpers;

using Xunit;

#endregion

namespace ArkaneSystems.Arkane.Zeroconf.Tests.Platform;

/// <summary>
///   Platform-specific tests for Bonjour native interop.
///   These tests verify platform detection and basic native functionality.
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
      Assert.Skip ("No Zeroconf provider available on this system. Install Avahi (Linux), Bonjour (Windows/macOS), or run on Windows 11+ for the Windows mDNS fallback.");

    try
    {
      using var browser = new ServiceBrowser ();
    }
    catch (InvalidOperationException ex)
    {
      Assert.Fail ($"Native zeroconf initialization failed. Ensure platform dependency is installed and running (Windows/macOS: Bonjour, Linux: Avahi). Details: {ex.Message}");
    }
    catch (DllNotFoundException ex)
    {
      Assert.Fail ($"Native zeroconf library was not found. Install platform dependency (Windows/macOS: Bonjour, Linux: Avahi). Details: {ex.Message}");
    }
    catch (EntryPointNotFoundException ex)
    {
      Assert.Fail ($"Native zeroconf entry point was not found. Verify a compatible platform dependency installation (Windows/macOS: Bonjour, Linux: Avahi). Details: {ex.Message}");
    }
  }

  [Fact]
  public void WindowsBonjour_IsAvailable ()
  {
    if (!OperatingSystem.IsWindows ())
      return;

    Assert.True (condition: ZeroconfSupport.CanBrowse,
                 userMessage: "Windows zeroconf browsing is unavailable. Ensure Bonjour is installed and running.");
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
  public void LinuxAvahi_IsAvailable ()
  {
    if (!OperatingSystem.IsLinux ())
      return;

    if (!ZeroconfSupport.IsAvailable)
      Assert.Skip ("Avahi (libdns_sd) is not available on this Linux system. Install libavahi-compat-libdnssd1 and ensure the Avahi daemon is running.");

    Assert.True (condition: ZeroconfSupport.CanBrowse,
                 userMessage: "Linux zeroconf browsing is unavailable. Ensure Avahi is installed and running.");
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
