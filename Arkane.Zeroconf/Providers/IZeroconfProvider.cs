#region header

// Arkane.ZeroConf - IZeroconfProvider.cs
// 

#endregion

#region using

using System ;

#endregion

namespace ArkaneSystems.Arkane.Zeroconf.Providers ;

public interface IZeroconfProvider
{
    Type ServiceBrowser { get ; }

    Type RegisterService { get ; }

    Type TxtRecord { get ; }

    ZeroconfCapability Capabilities => ZeroconfCapability.Browse | ZeroconfCapability.Publish ;

    bool IsAvailable () => true ;

    void Initialize () ;
}
