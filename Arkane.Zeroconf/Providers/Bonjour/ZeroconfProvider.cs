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
            ServiceRef sd_ref ;
            ServiceError error = Native.DNSServiceCreateConnection (out sd_ref) ;

            if (error != ServiceError.NoError)
                throw new ServiceErrorException (error) ;

            sd_ref.Deallocate () ;
        }
    }

    public class ZeroconfProvider : IZeroconfProvider
    {
        public Type ServiceBrowser { get { return typeof (ServiceBrowser) ; } }

        public Type RegisterService { get { return typeof (RegisterService) ; } }

        public Type TxtRecord { get { return typeof (TxtRecord) ; } }

        public void Initialize () { Zeroconf.Initialize () ; }
    }
}
