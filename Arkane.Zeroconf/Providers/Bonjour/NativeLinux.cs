#region header

// Arkane.ZeroConf - NativeLinux.cs
//

#endregion

#region using

using System ;
using System.Runtime.InteropServices ;

using static ArkaneSystems.Arkane.Zeroconf.Providers.Bonjour.Native ;

#endregion

namespace ArkaneSystems.Arkane.Zeroconf.Providers.Bonjour ;

internal static class NativeLinux
{
    private const string dllName = "libdns_sd.so.1" ;

    [DllImport (dllName)]
    public static extern void DNSServiceRefDeallocate (IntPtr sdRef) ;

    [DllImport (dllName)]
    public static extern ServiceError DNSServiceProcessResult (IntPtr sdRef) ;

    [DllImport (dllName)]
    public static extern int DNSServiceRefSockFD (IntPtr sdRef) ;

    [DllImport (dllName)]
    public static extern ServiceError DNSServiceCreateConnection (out ServiceRef sdRef) ;

    [DllImport (dllName)]
    public static extern ServiceError DNSServiceBrowse (out ServiceRef        sdRef,
        ServiceFlags          flags,
        uint                  interfaceIndex,
        string                regtype,
        string                domain,
        DNSServiceBrowseReply callBack,
        IntPtr                context) ;

    [DllImport (dllName)]
    public static extern ServiceError DNSServiceResolve (out ServiceRef         sdRef,
        ServiceFlags           flags,
        uint                   interfaceIndex,
        byte[]                 name,
        string                 regtype,
        string                 domain,
        DNSServiceResolveReply callBack,
        IntPtr                 context) ;

    [DllImport (dllName)]
    public static extern ServiceError DNSServiceRegister (out ServiceRef          sdRef,
        ServiceFlags            flags,
        uint                    interfaceIndex,
        byte[]                  name,
        string                  regtype,
        string                  domain,
        string                  host,
        ushort                  port,
        ushort                  txtLen,
        byte[]                  txtRecord,
        DNSServiceRegisterReply callBack,
        IntPtr                  context) ;

    [DllImport (dllName)]
    public static extern ServiceError DNSServiceQueryRecord (out ServiceRef             sdRef,
        ServiceFlags               flags,
        uint                       interfaceIndex,
        string                     fullname,
        ServiceType                rrtype,
        ServiceClass               rrclass,
        DNSServiceQueryRecordReply callBack,
        IntPtr                     context) ;

    // TXT Record Handling

    [DllImport (dllName)]
    public static extern void TXTRecordCreate (IntPtr txtRecord, ushort bufferLen, IntPtr buffer) ;

    [DllImport (dllName)]
    public static extern void TXTRecordDeallocate (IntPtr txtRecord) ;

    [DllImport (dllName)]
    public static extern ServiceError TXTRecordGetItemAtIndex (ushort     txtLen,
        IntPtr     txtRecord,
        ushort     index,
        ushort     keyBufLen,
        byte[]     key,
        out byte   valueLen,
        out IntPtr value) ;

    [DllImport (dllName)]
    public static extern ServiceError TXTRecordSetValue (IntPtr txtRecord,
        byte[] key,
        sbyte  valueSize,
        byte[] value) ;

    [DllImport (dllName)]
    public static extern ServiceError TXTRecordRemoveValue (IntPtr txtRecord, byte[] key) ;

    [DllImport (dllName)]
    public static extern ushort TXTRecordGetLength (IntPtr txtRecord) ;

    [DllImport (dllName)]
    public static extern IntPtr TXTRecordGetBytesPtr (IntPtr txtRecord) ;

    [DllImport (dllName)]
    public static extern ushort TXTRecordGetCount (ushort txtLen, IntPtr txtRecord) ;
}