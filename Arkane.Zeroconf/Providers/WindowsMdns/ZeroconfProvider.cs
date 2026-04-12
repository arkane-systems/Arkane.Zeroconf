#region header

// Arkane.ZeroConf - ZeroconfProvider.cs
// 

#endregion

#region using

using System ;
using ArkaneSystems.Arkane.Zeroconf.Providers ;

#endregion

[assembly: ZeroconfProvider (typeof (ArkaneSystems.Arkane.Zeroconf.Providers.WindowsMdns.ZeroconfProvider), priority: 10)]

namespace ArkaneSystems.Arkane.Zeroconf.Providers.WindowsMdns ;

public class ZeroconfProvider : IZeroconfProvider
{
    public Type ServiceBrowser => typeof (ServiceBrowser) ;

    public Type RegisterService => typeof (RegisterService) ;

    public Type TxtRecord => typeof (TxtRecord) ;

    public ZeroconfCapability Capabilities => ZeroconfCapability.Browse ;

    public bool IsAvailable () => OperatingSystem.IsWindowsVersionAtLeast (10, 0, 22000) ;

    public void Initialize () { }
}
