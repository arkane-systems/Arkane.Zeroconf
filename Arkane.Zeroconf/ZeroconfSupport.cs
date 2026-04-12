#region header

// Arkane.ZeroConf - ZeroconfSupport.cs
// 

#endregion

#region using

using ArkaneSystems.Arkane.Zeroconf.Providers ;

#endregion

namespace ArkaneSystems.Arkane.Zeroconf ;

public static class ZeroconfSupport
{
    /// <summary>
    /// Gets the capabilities exposed by the currently selected Zeroconf provider.
    /// </summary>
    public static ZeroconfCapability Capabilities => ProviderFactory.SelectedProvider.Capabilities ;

    /// <summary>
    /// Gets whether the selected provider supports service browsing and lookup.
    /// </summary>
    public static bool CanBrowse => (Capabilities & ZeroconfCapability.Browse) == ZeroconfCapability.Browse ;

    /// <summary>
    /// Gets whether the selected provider supports service publishing.
    /// </summary>
    public static bool CanPublish => (Capabilities & ZeroconfCapability.Publish) == ZeroconfCapability.Publish ;
}
