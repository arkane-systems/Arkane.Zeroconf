#region header

// Arkane.ZeroConf - ZeroconfProvider.cs

#endregion

#region using

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

using ArkaneSystems.Arkane.Zeroconf.Providers;
using ArkaneSystems.Arkane.Zeroconf.Providers.Bonjour;

#endregion

[assembly: ZeroconfProvider (providerType: typeof (ZeroconfProvider), priority: 100)]

namespace ArkaneSystems.Arkane.Zeroconf.Providers.Bonjour;

public static class Zeroconf
{
  [DllImport ("c")]
  private static extern void setenv (string key, string value);

  /// <summary>
  /// Initializes the Bonjour/mDNS provider.
  /// </summary>
  /// <remarks>
  /// On Linux, this method configures Avahi compatibility mode and returns early, as Avahi does not support
  /// DNSServiceCreateConnection. This is a platform-specific limitation of the Avahi mDNS implementation.
  /// On macOS and Windows (when Bonjour is installed), creates a shared DNS-SD connection to validate availability.
  /// </remarks>
  /// <exception cref="ServiceErrorException">Thrown when DNSServiceCreateConnection fails (non-Linux platforms).</exception>
  public static void Initialize ()
  {
    if (OperatingSystem.IsLinux ())
    {
      // Using setenv instead of Environment.SetEnvironmentVariable because, as documented:
      // > On non-Windows systems, calls to the SetEnvironmentVariable(String, String, EnvironmentVariableTarget) method with
      // > a value of EnvironmentVariableTarget.Process have no effect on any native libraries that are, or will be, loaded.
      setenv (key: "AVAHI_COMPAT_NOWARN", value: "1");

      // DNSServiceCreateConnection is not supported on Linux
      // See https://github.com/lathiat/avahi/blob/55d783d9d11ced838d73a2757273c5f6958ccd5c/avahi-compat-libdns_sd/unsupported.c#L62-L66
      Debug.WriteLine ("Bonjour initialization on Linux: skipping DNSServiceCreateConnection (Avahi limitation)");
      return;
    }

    ServiceError error = Native.DNSServiceCreateConnection (out ServiceRef sdRef);

    if (error != ServiceError.NoError)
      throw new ServiceErrorException (error);

    sdRef.Deallocate ();
  }
}

/// <summary>
/// Preferred Zeroconf provider backed by Bonjour-compatible DNS-SD APIs.
/// </summary>
/// <remarks>
/// <para>
/// This provider is preferred whenever it is available and supports both browsing and publishing.
/// On Windows, if Bonjour is unavailable, Arkane.Zeroconf may fall back to the <c>WindowsMdns</c> provider for lookup-only behavior.
/// </para>
/// <para>
/// This provider remains the primary implementation across supported platforms, including Avahi-compatible environments on Linux.
/// </para>
/// </remarks>
public class ZeroconfProvider : IZeroconfProvider
{
  public Type ServiceBrowser => typeof (ServiceBrowser);

  public Type RegisterService => typeof (RegisterService);

  public Type TxtRecord => typeof (TxtRecord);

  public ZeroconfCapability Capabilities => ZeroconfCapability.Browse | ZeroconfCapability.Publish;

  /// <summary>
  /// Determines whether the Bonjour provider can be initialized on the current system.
  /// </summary>
  /// <returns><see langword="true"/> when Bonjour-compatible APIs are available; otherwise, <see langword="false"/>.</returns>
  public bool IsAvailable ()
  {
    try
    {
      Zeroconf.Initialize ();

      return true;
    }
    catch (ServiceErrorException) { return false; }
    catch (DllNotFoundException) { return false; }
    catch (EntryPointNotFoundException) { return false; }
  }

  /// <summary>
  /// Initializes the Bonjour provider.
  /// </summary>
  public void Initialize () { Zeroconf.Initialize (); }
}
