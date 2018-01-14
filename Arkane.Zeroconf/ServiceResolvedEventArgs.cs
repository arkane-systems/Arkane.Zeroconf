#region header

// Arkane.Zeroconf - ServiceResolvedEventArgs.cs
// 

#endregion

#region using

using System ;

#endregion

namespace ArkaneSystems.Arkane.Zeroconf
{
    public class ServiceResolvedEventArgs : EventArgs
    {
        public ServiceResolvedEventArgs (IResolvableService service) => this.Service = service ;

        public IResolvableService Service { get ; }
    }
}
