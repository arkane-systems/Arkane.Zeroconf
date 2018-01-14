#region header

// Arkane.Zeroconf - ZeroconfProvider.cs
// 

#endregion

#region using

using System ;

using ArkaneSystems.Arkane.Zeroconf.Providers ;
using ArkaneSystems.Arkane.Zeroconf.Providers.Bonjour ;

#endregion

[assembly: ZeroconfProvider (typeof (ZeroconfProvider))]

namespace ArkaneSystems.Arkane.Zeroconf.Providers.Bonjour
{
    public static class Zeroconf
    {
        public static void Initialize ()
        {
            ServiceError error = Native.DNSServiceCreateConnection (out ServiceRef sdRef) ;

            if (error != ServiceError.NoError)
                throw new ServiceErrorException (error) ;

            sdRef.Deallocate () ;
        }
    }

    public class ZeroconfProvider : IZeroconfProvider
    {
        public Type ServiceBrowser => typeof (ServiceBrowser) ;

        public Type RegisterService => typeof (RegisterService) ;

        public Type TxtRecord => typeof (TxtRecord) ;

        public void Initialize () { Zeroconf.Initialize () ; }
    }
}
