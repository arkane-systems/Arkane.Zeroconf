#region header

// Arkane.ZeroConf - NativeWindows.cs
// 

#endregion

#region using

using System ;
using System.Runtime.InteropServices ;

using static ArkaneSystems.Arkane.Zeroconf.Providers.Bonjour.Native ;

#endregion

namespace ArkaneSystems.Arkane.Zeroconf.Providers.Bonjour ;

internal static class NativeWindows
{
    private const string dllName = "dnssd.dll" ;

    [DllImport (NativeWindows.dllName)]
    public static extern void DNSServiceRefDeallocate (IntPtr sdRef) ;

    [DllImport (NativeWindows.dllName)]
    public static extern ServiceError DNSServiceProcessResult (IntPtr sdRef) ;

    [DllImport (NativeWindows.dllName)]
    public static extern int DNSServiceRefSockFD (IntPtr sdRef) ;

    [DllImport (NativeWindows.dllName)]
    public static extern ServiceError DNSServiceCreateConnection (out ServiceRef sdRef) ;

    [DllImport (NativeWindows.dllName)]
    public static extern ServiceError DNSServiceBrowse (out ServiceRef        sdRef,
                                                        ServiceFlags          flags,
                                                        uint                  interfaceIndex,
                                                        string                regtype,
                                                        string                domain,
                                                        DNSServiceBrowseReply callBack,
                                                        IntPtr                context) ;

    [DllImport (NativeWindows.dllName)]
    public static extern ServiceError DNSServiceResolve (out ServiceRef         sdRef,
                                                         ServiceFlags           flags,
                                                         uint                   interfaceIndex,
                                                         byte[]                 name,
                                                         string                 regtype,
                                                         string                 domain,
                                                         DNSServiceResolveReply callBack,
                                                         IntPtr                 context) ;

    [DllImport (NativeWindows.dllName)]
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

    [DllImport (NativeWindows.dllName)]
    public static extern ServiceError DNSServiceQueryRecord (out ServiceRef             sdRef,
                                                             ServiceFlags               flags,
                                                             uint                       interfaceIndex,
                                                             string                     fullname,
                                                             ServiceType                rrtype,
                                                             ServiceClass               rrclass,
                                                             DNSServiceQueryRecordReply callBack,
                                                             IntPtr                     context) ;

    // TXT Record Handling

    [DllImport (NativeWindows.dllName)]
    public static extern void TXTRecordCreate (IntPtr txtRecord, ushort bufferLen, IntPtr buffer) ;

    [DllImport (NativeWindows.dllName)]
    public static extern void TXTRecordDeallocate (IntPtr txtRecord) ;

    [DllImport (NativeWindows.dllName)]
    public static extern ServiceError TXTRecordGetItemAtIndex (ushort     txtLen,
                                                               IntPtr     txtRecord,
                                                               ushort     index,
                                                               ushort     keyBufLen,
                                                               byte[]     key,
                                                               out byte   valueLen,
                                                               out IntPtr value) ;

    [DllImport (NativeWindows.dllName)]
    public static extern ServiceError TXTRecordSetValue (IntPtr txtRecord,
                                                         byte[] key,
                                                         sbyte  valueSize,
                                                         byte[] value) ;

    [DllImport (NativeWindows.dllName)]
    public static extern ServiceError TXTRecordRemoveValue (IntPtr txtRecord, byte[] key) ;

    [DllImport (NativeWindows.dllName)]
    public static extern ushort TXTRecordGetLength (IntPtr txtRecord) ;

    [DllImport (NativeWindows.dllName)]
    public static extern IntPtr TXTRecordGetBytesPtr (IntPtr txtRecord) ;

    [DllImport (NativeWindows.dllName)]
    public static extern ushort TXTRecordGetCount (ushort txtLen, IntPtr txtRecord) ;
}
