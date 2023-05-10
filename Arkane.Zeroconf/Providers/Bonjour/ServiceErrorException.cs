#region header

// Arkane.ZeroConf - ServiceErrorException.cs
// 

#endregion

#region using

using System ;

#endregion

namespace ArkaneSystems.Arkane.Zeroconf.Providers.Bonjour ;

internal class ServiceErrorException : Exception
{
    internal ServiceErrorException (ServiceError error) : base (error.ToString ()) { }

    internal ServiceErrorException (string error) : base (error) { }
}
