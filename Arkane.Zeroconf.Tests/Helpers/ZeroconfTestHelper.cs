#region header

// Arkane.Zeroconf.Tests - ZeroconfTestHelper.cs

#endregion

#region using

using System;

using ArkaneSystems.Arkane.Zeroconf.Providers.SystemdResolved;

using Xunit;

#endregion

namespace ArkaneSystems.Arkane.Zeroconf.Tests.Helpers;

/// <summary>
///   Shared skip helpers for Zeroconf tests.
/// </summary>
internal static class ZeroconfTestHelper
{
  /// <summary>
  ///   Skips the current test when no Zeroconf provider is available on the running system.
  /// </summary>
  public static void SkipIfNoProvider ()
  {
    if (!ZeroconfSupport.IsAvailable)
      Assert.Skip ("No Zeroconf provider available on this system. Install Avahi (Linux), Bonjour (Windows/macOS), or run on Windows 11+ for the Windows mDNS fallback.");
  }

  /// <summary>
  ///   Skips the current test when the active Zeroconf provider does not support publishing.
  /// </summary>
  public static void SkipIfCannotPublish ()
  {
    if (!ZeroconfSupport.CanPublish)
      Assert.Skip ("mDNS service publishing is not supported by the active Zeroconf provider on this system.");
  }

  /// <summary>
  ///   Skips the current test when not running on Windows.
  /// </summary>
  public static void SkipIfNotWindows ()
  {
    if (!OperatingSystem.IsWindows ())
      Assert.Skip ("This test requires Windows.");
  }

  /// <summary>
  ///   Skips the current test when not running on Linux.
  /// </summary>
  public static void SkipIfNotLinux ()
  {
    if (!OperatingSystem.IsLinux ())
      Assert.Skip ("This test requires Linux.");
  }

  /// <summary>
  ///   Skips the current test when the systemd-resolved D-Bus provider is not available on this system.
  ///   This checks only the systemd-resolved provider directly, regardless of which provider is currently
  ///   selected by the provider factory.
  /// </summary>
  public static void SkipIfSystemdResolvedUnavailable ()
  {
    if (!OperatingSystem.IsLinux () || !new ZeroconfProvider ().IsAvailable ())
      Assert.Skip ("systemd-resolved mDNS is not available on this system. Ensure MulticastDNS is enabled in /etc/systemd/resolved.conf.");
  }
}
