#region header

// Arkane.ZeroConf - ZeroconfProvider.cs

#endregion

#region using

using System;

using ArkaneSystems.Arkane.Zeroconf.Providers;
using ArkaneSystems.Arkane.Zeroconf.Providers.WindowsMdns;

#endregion

[assembly: ZeroconfProvider (providerType: typeof (ZeroconfProvider), priority: 10)]

namespace ArkaneSystems.Arkane.Zeroconf.Providers.WindowsMdns;

/// <summary>
///   Windows DNS-SD fallback provider for Zeroconf browsing and resolving.
/// </summary>
/// <remarks>
///   <para>
///     This provider is intended as a simple fallback on Windows 11 and later when the Bonjour provider is unavailable.
///     It is not a general-purpose alternative priority system; Bonjour remains the preferred provider whenever it can be
///     initialized.
///   </para>
///   <para>
///     The Windows fallback provider supports lookup operations only. Publishing is not supported.
///     Consumers should check <see cref="ZeroconfSupport.CanPublish" /> before attempting to register services.
///   </para>
/// </remarks>
public class ZeroconfProvider : IZeroconfProvider
{
  public Type ServiceBrowser => typeof (ServiceBrowser);

  public Type RegisterService => typeof (RegisterService);

  public Type TxtRecord => typeof (TxtRecord);

  public ZeroconfCapability Capabilities => ZeroconfCapability.Browse;

  /// <summary>
  ///   Determines whether the Windows DNS-SD fallback provider can be used on the current operating system.
  /// </summary>
  /// <returns><see langword="true" /> on Windows 11 or later; otherwise, <see langword="false" />.</returns>
  public bool IsAvailable () => OperatingSystem.IsWindowsVersionAtLeast (major: 10, minor: 0, build: 22000);

  /// <summary>
  ///   Initializes the Windows DNS-SD fallback provider.
  /// </summary>
  /// <remarks>
  ///   No explicit initialization is required for this provider.
  /// </remarks>
  public void Initialize () { }
}
