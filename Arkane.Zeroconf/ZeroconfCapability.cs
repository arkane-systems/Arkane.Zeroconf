#region header

// Arkane.ZeroConf - ZeroconfCapability.cs
// 

#endregion

#region using

using System ;

#endregion

namespace ArkaneSystems.Arkane.Zeroconf ;

[Flags]
public enum ZeroconfCapability
{
    None = 0,
    Browse = 1,
    Publish = 2,
}
