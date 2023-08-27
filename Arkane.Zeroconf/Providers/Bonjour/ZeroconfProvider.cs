#region header

// Arkane.ZeroConf - ZeroconfProvider.cs
// 

#endregion

#region using

using System ;
using System.Runtime.InteropServices ;
using ArkaneSystems.Arkane.Zeroconf.Providers ;
using ArkaneSystems.Arkane.Zeroconf.Providers.Bonjour ;

#endregion

[assembly: ZeroconfProvider (typeof (ZeroconfProvider))]

namespace ArkaneSystems.Arkane.Zeroconf.Providers.Bonjour ;

public static class Zeroconf
{
    [DllImport("c")]
    private static extern void setenv(string key, string value) ;

    public static void Initialize ()
    {
        if (OperatingSystem.IsLinux())
        {
            // Using setenv instead of Environment.SetEnvironmentVariable because, as documented:
            // > On non-Windows systems, calls to the SetEnvironmentVariable(String, String, EnvironmentVariableTarget) method with
            // > a value of EnvironmentVariableTarget.Process have no effect on any native libraries that are, or will be, loaded.
            setenv("AVAHI_COMPAT_NOWARN", "1");

            // DNSServiceCreateConnection is not supported on Linux
            // See https://github.com/lathiat/avahi/blob/55d783d9d11ced838d73a2757273c5f6958ccd5c/avahi-compat-libdns_sd/unsupported.c#L62-L66
            return;
        }

        var error = Native.DNSServiceCreateConnection (out var sdRef) ;

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
