#region header

// Arkane.ZeroConf - ServiceBrowseEventArgs.cs
// 

#endregion

#region using

using System ;

#endregion

namespace ArkaneSystems.Arkane.Zeroconf ;

public class ServiceBrowseEventArgs : EventArgs
{
    public ServiceBrowseEventArgs (IResolvableService service) => this.Service = service ;

    public IResolvableService Service { get ; }
}
