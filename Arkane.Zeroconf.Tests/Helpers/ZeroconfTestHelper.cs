#region header

// Arkane.Zeroconf.Tests - ZeroconfTestHelper.cs

#endregion

#region using

using System;

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
  ///   Skips the current test when not running on Windows.
  /// </summary>
  public static void SkipIfNotWindows ()
  {
    if (!OperatingSystem.IsWindows ())
      Assert.Skip ("This test requires Windows.");
  }
}
