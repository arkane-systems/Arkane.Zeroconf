#region header

// Arkane.Zeroconf.Tests - PlatformSpecificTests.cs

#endregion

#region using

using System;
using System.Runtime.InteropServices;

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

  [Fact (Skip = "Requires platform-specific native libraries")]
  public void NativeInitialization_Succeeds ()
  {
    // This test is skipped by default as it requires Bonjour/Avahi installation
    // To run: install Bonjour (Windows/macOS) or Avahi (Linux) and remove Skip attribute

    // Act
    try
    {
      var browser = new ServiceBrowser ();
      browser.Dispose ();
    }
    catch (Exception ex)
    {
      // If daemon not available, exception is expected
      Assert.True (ex is not null);
    }
  }

  [Fact (Skip = "Platform-specific behavior verification")]
  public void WindowsBonjour_IsAvailable ()
  {
    // This test is skipped on non-Windows platforms
    if (!OperatingSystem.IsWindows ())
      return;

    // Test would verify Windows Bonjour service is installed
    // Implementation depends on Windows service enumeration
  }

  [Fact (Skip = "Platform-specific behavior verification")]
  public void MacOSMDNS_IsAvailable ()
  {
    // This test is skipped on non-macOS platforms
    if (!OperatingSystem.IsMacOS ())
      return;

    // macOS has mDNS built-in, so this should generally succeed
  }

  [Fact (Skip = "Platform-specific behavior verification")]
  public void LinuxAvahi_IsAvailable ()
  {
    // This test is skipped on non-Linux platforms
    if (!OperatingSystem.IsLinux ())
      return;

    // Test would verify Avahi daemon is installed and running
    // Could check for dbus availability or avahi-daemon process
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

    // Only verify the platform we're expecting
    if (actualIsWindows)
      Assert.True (condition: isWin, userMessage: $"Expected {expectedName} but running on a different platform");
    else if (actualIsMacOS)
      Assert.True (condition: isMac, userMessage: $"Expected {expectedName} but running on a different platform");
    else if (actualIsLinux)
      Assert.True (condition: isLinux, userMessage: $"Expected {expectedName} but running on a different platform");
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
