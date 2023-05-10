#region header

// Arkane.ZeroConf - ZeroconfProviderAttribute.cs
// 

#endregion

#region using

using System ;

#endregion

namespace ArkaneSystems.Arkane.Zeroconf.Providers ;

[AttributeUsage (AttributeTargets.Assembly)]
public class ZeroconfProviderAttribute : Attribute
{
    public ZeroconfProviderAttribute (Type providerType) => this.ProviderType = providerType ;

    public Type ProviderType { get ; }
}
