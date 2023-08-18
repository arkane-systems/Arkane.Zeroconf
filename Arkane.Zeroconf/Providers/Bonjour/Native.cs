#region header

// Arkane.ZeroConf - Native.cs
// 

#endregion

#region using

using System ;

// ReSharper disable InconsistentNaming

#endregion

namespace ArkaneSystems.Arkane.Zeroconf.Providers.Bonjour ;

public static class Native
{
    #region Delegates

    // DNSServiceBrowse

    public delegate void DNSServiceBrowseReply (ServiceRef   sdRef,
                                                ServiceFlags flags,
                                                uint         interfaceIndex,
                                                ServiceError errorCode,
                                                IntPtr       serviceName,
                                                string       regtype,
                                                string       replyDomain,
                                                IntPtr       context) ;

    // DNSServiceQueryRecord

    public delegate void DNSServiceQueryRecordReply (ServiceRef   sdRef,
                                                     ServiceFlags flags,
                                                     uint         interfaceIndex,
                                                     ServiceError errorCode,
                                                     string       fullname,
                                                     ServiceType  rrtype,
                                                     ServiceClass rrclass,
                                                     ushort       rdlen,
                                                     IntPtr       rdata,
                                                     uint         ttl,
                                                     IntPtr       context) ;

    // DNSServiceRegister

    public delegate void DNSServiceRegisterReply (ServiceRef   sdRef,
                                                  ServiceFlags flags,
                                                  ServiceError errorCode,
                                                  IntPtr       name,
                                                  string       regtype,
                                                  string       domain,
                                                  IntPtr       context) ;

    // DNSServiceResolve

    public delegate void DNSServiceResolveReply (ServiceRef   sdRef,
                                                 ServiceFlags flags,
                                                 uint         interfaceIndex,
                                                 ServiceError errorCode,
                                                 IntPtr       fullname,
                                                 string       hosttarget,
                                                 ushort       port,
                                                 ushort       txtLen,
                                                 IntPtr       txtRecord,
                                                 IntPtr       context) ;

    #endregion

    public static void DNSServiceRefDeallocate (IntPtr sdRef)
    {
        if (OperatingSystem.IsWindows())
            NativeWindows.DNSServiceRefDeallocate (sdRef) ;
        else if (OperatingSystem.IsMacOS())
            NativeMacOS.DNSServiceRefDeallocate (sdRef) ;
        else
            NativeLinux.DNSServiceRefDeallocate (sdRef) ;
    }

    public static ServiceError DNSServiceProcessResult (IntPtr sdRef)
    {
        if (OperatingSystem.IsWindows())
            return NativeWindows.DNSServiceProcessResult (sdRef) ;
        if (OperatingSystem.IsMacOS())
            return NativeMacOS.DNSServiceProcessResult (sdRef) ;
        return NativeLinux.DNSServiceProcessResult (sdRef) ;
    }

    public static int DNSServiceRefSockFD (IntPtr sdRef)
    {
        if (OperatingSystem.IsWindows())
            return NativeWindows.DNSServiceRefSockFD (sdRef) ;
        if (OperatingSystem.IsMacOS())
            return NativeMacOS.DNSServiceRefSockFD (sdRef) ;
        return NativeLinux.DNSServiceRefSockFD (sdRef) ;
    }

    public static ServiceError DNSServiceCreateConnection (out ServiceRef sdRef)
    {
        if (OperatingSystem.IsWindows())
            return NativeWindows.DNSServiceCreateConnection (out sdRef) ;
        if (OperatingSystem.IsMacOS())
            return NativeMacOS.DNSServiceCreateConnection (out sdRef) ;
        return NativeLinux.DNSServiceCreateConnection (out sdRef) ;
    }

    public static ServiceError DNSServiceBrowse (out ServiceRef        sdRef,
                                                 ServiceFlags          flags,
                                                 uint                  interfaceIndex,
                                                 string                regtype,
                                                 string                domain,
                                                 DNSServiceBrowseReply callBack,
                                                 IntPtr                context)
    {
        if (OperatingSystem.IsWindows())
            return NativeWindows.DNSServiceBrowse (out sdRef, flags, interfaceIndex, regtype, domain, callBack, context) ;
        if (OperatingSystem.IsMacOS())
            return NativeMacOS.DNSServiceBrowse (out sdRef, flags, interfaceIndex, regtype, domain, callBack, context) ;
        return NativeLinux.DNSServiceBrowse (out sdRef, flags, interfaceIndex, regtype, domain, callBack, context) ;
    }

    public static ServiceError DNSServiceResolve (out ServiceRef         sdRef,
                                                  ServiceFlags           flags,
                                                  uint                   interfaceIndex,
                                                  byte[]                 name,
                                                  string                 regtype,
                                                  string                 domain,
                                                  DNSServiceResolveReply callBack,
                                                  IntPtr                 context)
    {
        if (OperatingSystem.IsWindows())
            return NativeWindows.DNSServiceResolve (out sdRef,
                                                flags,
                                                interfaceIndex,
                                                name,
                                                regtype,
                                                domain,
                                                callBack,
                                                context) ;
        if (OperatingSystem.IsMacOS())
            return NativeMacOS.DNSServiceResolve (out sdRef, flags, interfaceIndex, name, regtype, domain, callBack, context) ;
        return NativeLinux.DNSServiceResolve (out sdRef, flags, interfaceIndex, name, regtype, domain, callBack, context) ;
    }

    public static ServiceError DNSServiceRegister (out ServiceRef          sdRef,
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
                                                   IntPtr                  context)
    {
        if (OperatingSystem.IsWindows())
            return NativeWindows.DNSServiceRegister (out sdRef,
                                                     flags,
                                                     interfaceIndex,
                                                     name,
                                                     regtype,
                                                     domain,
                                                     host,
                                                     port,
                                                     txtLen,
                                                     txtRecord,
                                                     callBack,
                                                     context) ;
        if (OperatingSystem.IsMacOS())
            return NativeMacOS.DNSServiceRegister (out sdRef,
                                                 flags,
                                                 interfaceIndex,
                                                 name,
                                                 regtype,
                                                 domain,
                                                 host,
                                                 port,
                                                 txtLen,
                                                 txtRecord,
                                                 callBack,
                                                 context) ;
        return NativeLinux.DNSServiceRegister (out sdRef,
                                             flags,
                                             interfaceIndex,
                                             name,
                                             regtype,
                                             domain,
                                             host,
                                             port,
                                             txtLen,
                                             txtRecord,
                                             callBack,
                                             context) ;
    }

    public static ServiceError DNSServiceQueryRecord (out ServiceRef             sdRef,
                                                      ServiceFlags               flags,
                                                      uint                       interfaceIndex,
                                                      string                     fullname,
                                                      ServiceType                rrtype,
                                                      ServiceClass               rrclass,
                                                      DNSServiceQueryRecordReply callBack,
                                                      IntPtr                     context)
    {
        if (OperatingSystem.IsWindows())
            return NativeWindows.DNSServiceQueryRecord (out sdRef,
                                                        flags,
                                                        interfaceIndex,
                                                        fullname,
                                                        rrtype,
                                                        rrclass,
                                                        callBack,
                                                        context) ;
        if (OperatingSystem.IsMacOS())
            return NativeMacOS.DNSServiceQueryRecord (out sdRef,
                                                    flags,
                                                    interfaceIndex,
                                                    fullname,
                                                    rrtype,
                                                    rrclass,
                                                    callBack,
                                                    context) ;
        return NativeLinux.DNSServiceQueryRecord (out sdRef,
                                                flags,
                                                interfaceIndex,
                                                fullname,
                                                rrtype,
                                                rrclass,
                                                callBack,
                                                context) ;
    }

    // TXT Record Handling

    public static void TXTRecordCreate (IntPtr txtRecord, ushort bufferLen, IntPtr buffer)
    {
        if (OperatingSystem.IsWindows())
            NativeWindows.TXTRecordCreate (txtRecord, bufferLen, buffer) ;
        else if (OperatingSystem.IsMacOS())
            NativeMacOS.TXTRecordCreate (txtRecord, bufferLen, buffer) ;
        else
            NativeLinux.TXTRecordCreate (txtRecord, bufferLen, buffer) ;
    }

    public static void TXTRecordDeallocate (IntPtr txtRecord)
    {
        if (OperatingSystem.IsWindows())
            NativeWindows.TXTRecordDeallocate (txtRecord) ;
        else if (OperatingSystem.IsMacOS())
            NativeMacOS.TXTRecordDeallocate (txtRecord) ;
        else
            NativeLinux.TXTRecordDeallocate (txtRecord) ;
    }

    public static ServiceError TXTRecordGetItemAtIndex (ushort     txtLen,
                                                        IntPtr     txtRecord,
                                                        ushort     index,
                                                        ushort     keyBufLen,
                                                        byte[]     key,
                                                        out byte   valueLen,
                                                        out IntPtr value)
    {
        if (OperatingSystem.IsWindows())
            return NativeWindows.TXTRecordGetItemAtIndex (txtLen, txtRecord, index, keyBufLen, key, out valueLen, out value) ;
        if (OperatingSystem.IsMacOS())
            return NativeMacOS.TXTRecordGetItemAtIndex (txtLen, txtRecord, index, keyBufLen, key, out valueLen, out value) ;
        return NativeLinux.TXTRecordGetItemAtIndex (txtLen, txtRecord, index, keyBufLen, key, out valueLen, out value) ;
    }

    public static ServiceError TXTRecordSetValue (IntPtr txtRecord,
                                                  byte[] key,
                                                  sbyte  valueSize,
                                                  byte[] value)
    {
        if (OperatingSystem.IsWindows())
            return NativeWindows.TXTRecordSetValue (txtRecord, key, valueSize, value) ;
        if (OperatingSystem.IsMacOS())
            return NativeMacOS.TXTRecordSetValue (txtRecord, key, valueSize, value) ;
        return NativeLinux.TXTRecordSetValue (txtRecord, key, valueSize, value) ;
    }

    public static ServiceError TXTRecordRemoveValue (IntPtr txtRecord, byte[] key)
    {
        if (OperatingSystem.IsWindows())
            return NativeWindows.TXTRecordRemoveValue (txtRecord, key) ;
        if (OperatingSystem.IsMacOS())
            return NativeMacOS.TXTRecordRemoveValue (txtRecord, key) ;
        return NativeLinux.TXTRecordRemoveValue (txtRecord, key) ;
    }

    public static ushort TXTRecordGetLength (IntPtr txtRecord)
    {
        if (OperatingSystem.IsWindows())
            return NativeWindows.TXTRecordGetLength (txtRecord) ;
        if (OperatingSystem.IsMacOS())
            return NativeMacOS.TXTRecordGetLength (txtRecord) ;
        return NativeLinux.TXTRecordGetLength (txtRecord) ;
    }

    public static IntPtr TXTRecordGetBytesPtr (IntPtr txtRecord)
    {
        if (OperatingSystem.IsWindows())
            return NativeWindows.TXTRecordGetBytesPtr (txtRecord) ;
        if (OperatingSystem.IsMacOS())
            return NativeMacOS.TXTRecordGetBytesPtr (txtRecord) ;
        return NativeLinux.TXTRecordGetBytesPtr (txtRecord) ;
    }

    public static ushort TXTRecordGetCount (ushort txtLen, IntPtr txtRecord)
    {
        if (OperatingSystem.IsWindows())
            return NativeWindows.TXTRecordGetCount (txtLen, txtRecord) ;
        if (OperatingSystem.IsMacOS())
            return NativeMacOS.TXTRecordGetCount (txtLen, txtRecord) ;
        return NativeLinux.TXTRecordGetCount (txtLen, txtRecord) ;
    }
}
