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

public class ZeroconfProvider : IZeroconfProvider
{
  public Type ServiceBrowser => typeof (ServiceBrowser);

  public Type RegisterService => typeof (RegisterService);

  public Type TxtRecord => typeof (TxtRecord);

  public ZeroconfCapability Capabilities => ZeroconfCapability.Browse;

  public bool IsAvailable () => OperatingSystem.IsWindowsVersionAtLeast (major: 10, minor: 0, build: 22000);

  public void Initialize () { }
}
