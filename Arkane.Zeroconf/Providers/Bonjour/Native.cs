#region header

// Arkane.Zeroconf - Native.cs

#endregion

#region using

using System;
using System.Runtime.InteropServices;

// ReSharper disable InconsistentNaming

#endregion

namespace ArkaneSystems.Arkane.Zeroconf.Providers.Bonjour;

internal static class Native
{
  #region Nested type: DNSServiceBrowseReply

  // DNSServiceBrowse

  [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
  public delegate void DNSServiceBrowseReply (ServiceRef   sdRef,
                                              ServiceFlags flags,
                                              uint         interfaceIndex,
                                              ServiceError errorCode,
                                              IntPtr       serviceName,
                                              string       regtype,
                                              string       replyDomain,
                                              IntPtr       context);

  #endregion

  #region Nested type: DNSServiceQueryRecordReply

  // DNSServiceQueryRecord

  [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
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
                                                   IntPtr       context);

  #endregion

  #region Nested type: DNSServiceRegisterReply

  // DNSServiceRegister

  [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
  public delegate void DNSServiceRegisterReply (ServiceRef   sdRef,
                                                ServiceFlags flags,
                                                ServiceError errorCode,
                                                IntPtr       name,
                                                string       regtype,
                                                string       domain,
                                                IntPtr       context);

  #endregion

  #region Nested type: DNSServiceResolveReply

  // DNSServiceResolve

  [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
  public delegate void DNSServiceResolveReply (ServiceRef   sdRef,
                                               ServiceFlags flags,
                                               uint         interfaceIndex,
                                               ServiceError errorCode,
                                               IntPtr       fullname,
                                               string       hosttarget,
                                               ushort       port,
                                               ushort       txtLen,
                                               IntPtr       txtRecord,
                                               IntPtr       context);

  #endregion

  public static void DNSServiceRefDeallocate (IntPtr sdRef)
  {
    if (OperatingSystem.IsWindows ())
      NativeWindows.DNSServiceRefDeallocate (sdRef);
    else if (OperatingSystem.IsMacOS ())
      NativeMacOS.DNSServiceRefDeallocate (sdRef);
    else
      NativeLinux.DNSServiceRefDeallocate (sdRef);
  }

  public static ServiceError DNSServiceProcessResult (IntPtr sdRef)
    => OperatingSystem.IsWindows ()
         ? NativeWindows.DNSServiceProcessResult (sdRef)
         : OperatingSystem.IsMacOS ()
           ? NativeMacOS.DNSServiceProcessResult (sdRef)
           : NativeLinux.DNSServiceProcessResult (sdRef);

  public static int DNSServiceRefSockFD (IntPtr sdRef)
    => OperatingSystem.IsWindows ()
         ? NativeWindows.DNSServiceRefSockFD (sdRef)
         : OperatingSystem.IsMacOS ()
           ? NativeMacOS.DNSServiceRefSockFD (sdRef)
           : NativeLinux.DNSServiceRefSockFD (sdRef);

  public static ServiceError DNSServiceCreateConnection (out ServiceRef sdRef)
    => OperatingSystem.IsWindows ()
         ? NativeWindows.DNSServiceCreateConnection (out sdRef)
         : OperatingSystem.IsMacOS ()
           ? NativeMacOS.DNSServiceCreateConnection (out sdRef)
           : NativeLinux.DNSServiceCreateConnection (out sdRef);

  public static ServiceError DNSServiceBrowse (out ServiceRef        sdRef,
                                               ServiceFlags          flags,
                                               uint                  interfaceIndex,
                                               string                regtype,
                                               string                domain,
                                               DNSServiceBrowseReply callBack,
                                               IntPtr                context)
    => OperatingSystem.IsWindows ()
         ? NativeWindows.DNSServiceBrowse (sdRef: out sdRef,
                                           flags: flags,
                                           interfaceIndex: interfaceIndex,
                                           regtype: regtype,
                                           domain: domain,
                                           callBack: callBack,
                                           context: context)
         : OperatingSystem.IsMacOS ()
           ? NativeMacOS.DNSServiceBrowse (sdRef: out sdRef,
                                           flags: flags,
                                           interfaceIndex: interfaceIndex,
                                           regtype: regtype,
                                           domain: domain,
                                           callBack: callBack,
                                           context: context)
           : NativeLinux.DNSServiceBrowse (sdRef: out sdRef,
                                           flags: flags,
                                           interfaceIndex: interfaceIndex,
                                           regtype: regtype,
                                           domain: domain,
                                           callBack: callBack,
                                           context: context);

  public static ServiceError DNSServiceResolve (out ServiceRef         sdRef,
                                                ServiceFlags           flags,
                                                uint                   interfaceIndex,
                                                byte[]                 name,
                                                string                 regtype,
                                                string                 domain,
                                                DNSServiceResolveReply callBack,
                                                IntPtr                 context)
    => OperatingSystem.IsWindows ()
         ? NativeWindows.DNSServiceResolve (sdRef: out sdRef,
                                            flags: flags,
                                            interfaceIndex: interfaceIndex,
                                            name: name,
                                            regtype: regtype,
                                            domain: domain,
                                            callBack: callBack,
                                            context: context)
         : OperatingSystem.IsMacOS ()
           ? NativeMacOS.DNSServiceResolve (sdRef: out sdRef,
                                            flags: flags,
                                            interfaceIndex: interfaceIndex,
                                            name: name,
                                            regtype: regtype,
                                            domain: domain,
                                            callBack: callBack,
                                            context: context)
           : NativeLinux.DNSServiceResolve (sdRef: out sdRef,
                                            flags: flags,
                                            interfaceIndex: interfaceIndex,
                                            name: name,
                                            regtype: regtype,
                                            domain: domain,
                                            callBack: callBack,
                                            context: context);

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
    => OperatingSystem.IsWindows ()
         ? NativeWindows.DNSServiceRegister (sdRef: out sdRef,
                                             flags: flags,
                                             interfaceIndex: interfaceIndex,
                                             name: name,
                                             regtype: regtype,
                                             domain: domain,
                                             host: host,
                                             port: port,
                                             txtLen: txtLen,
                                             txtRecord: txtRecord,
                                             callBack: callBack,
                                             context: context)
         : OperatingSystem.IsMacOS ()
           ? NativeMacOS.DNSServiceRegister (sdRef: out sdRef,
                                             flags: flags,
                                             interfaceIndex: interfaceIndex,
                                             name: name,
                                             regtype: regtype,
                                             domain: domain,
                                             host: host,
                                             port: port,
                                             txtLen: txtLen,
                                             txtRecord: txtRecord,
                                             callBack: callBack,
                                             context: context)
           : NativeLinux.DNSServiceRegister (sdRef: out sdRef,
                                             flags: flags,
                                             interfaceIndex: interfaceIndex,
                                             name: name,
                                             regtype: regtype,
                                             domain: domain,
                                             host: host,
                                             port: port,
                                             txtLen: txtLen,
                                             txtRecord: txtRecord,
                                             callBack: callBack,
                                             context: context);

  public static ServiceError DNSServiceQueryRecord (out ServiceRef             sdRef,
                                                    ServiceFlags               flags,
                                                    uint                       interfaceIndex,
                                                    string                     fullname,
                                                    ServiceType                rrtype,
                                                    ServiceClass               rrclass,
                                                    DNSServiceQueryRecordReply callBack,
                                                    IntPtr                     context)
    => OperatingSystem.IsWindows ()
         ? NativeWindows.DNSServiceQueryRecord (sdRef: out sdRef,
                                                flags: flags,
                                                interfaceIndex: interfaceIndex,
                                                fullname: fullname,
                                                rrtype: rrtype,
                                                rrclass: rrclass,
                                                callBack: callBack,
                                                context: context)
         : OperatingSystem.IsMacOS ()
           ? NativeMacOS.DNSServiceQueryRecord (sdRef: out sdRef,
                                                flags: flags,
                                                interfaceIndex: interfaceIndex,
                                                fullname: fullname,
                                                rrtype: rrtype,
                                                rrclass: rrclass,
                                                callBack: callBack,
                                                context: context)
           : NativeLinux.DNSServiceQueryRecord (sdRef: out sdRef,
                                                flags: flags,
                                                interfaceIndex: interfaceIndex,
                                                fullname: fullname,
                                                rrtype: rrtype,
                                                rrclass: rrclass,
                                                callBack: callBack,
                                                context: context);

  // TXT Record Handling

  public static void TXTRecordCreate (IntPtr txtRecord, ushort bufferLen, IntPtr buffer)
  {
    if (OperatingSystem.IsWindows ())
      NativeWindows.TXTRecordCreate (txtRecord: txtRecord, bufferLen: bufferLen, buffer: buffer);
    else if (OperatingSystem.IsMacOS ())
      NativeMacOS.TXTRecordCreate (txtRecord: txtRecord, bufferLen: bufferLen, buffer: buffer);
    else
      NativeLinux.TXTRecordCreate (txtRecord: txtRecord, bufferLen: bufferLen, buffer: buffer);
  }

  public static void TXTRecordDeallocate (IntPtr txtRecord)
  {
    if (OperatingSystem.IsWindows ())
      NativeWindows.TXTRecordDeallocate (txtRecord);
    else if (OperatingSystem.IsMacOS ())
      NativeMacOS.TXTRecordDeallocate (txtRecord);
    else
      NativeLinux.TXTRecordDeallocate (txtRecord);
  }

  public static ServiceError TXTRecordGetItemAtIndex (ushort     txtLen,
                                                      IntPtr     txtRecord,
                                                      ushort     index,
                                                      ushort     keyBufLen,
                                                      byte[]     key,
                                                      out byte   valueLen,
                                                      out IntPtr value)
    => OperatingSystem.IsWindows ()
         ? NativeWindows.TXTRecordGetItemAtIndex (txtLen: txtLen,
                                                  txtRecord: txtRecord,
                                                  index: index,
                                                  keyBufLen: keyBufLen,
                                                  key: key,
                                                  valueLen: out valueLen,
                                                  value: out value)
         : OperatingSystem.IsMacOS ()
           ? NativeMacOS.TXTRecordGetItemAtIndex (txtLen: txtLen,
                                                  txtRecord: txtRecord,
                                                  index: index,
                                                  keyBufLen: keyBufLen,
                                                  key: key,
                                                  valueLen: out valueLen,
                                                  value: out value)
           : NativeLinux.TXTRecordGetItemAtIndex (txtLen: txtLen,
                                                  txtRecord: txtRecord,
                                                  index: index,
                                                  keyBufLen: keyBufLen,
                                                  key: key,
                                                  valueLen: out valueLen,
                                                  value: out value);

  public static ServiceError TXTRecordSetValue (IntPtr txtRecord,
                                                byte[] key,
                                                sbyte  valueSize,
                                                byte[] value)
    => OperatingSystem.IsWindows ()
         ? NativeWindows.TXTRecordSetValue (txtRecord: txtRecord, key: key, valueSize: valueSize, value: value)
         : OperatingSystem.IsMacOS ()
           ? NativeMacOS.TXTRecordSetValue (txtRecord: txtRecord, key: key, valueSize: valueSize, value: value)
           : NativeLinux.TXTRecordSetValue (txtRecord: txtRecord, key: key, valueSize: valueSize, value: value);

  public static ServiceError TXTRecordRemoveValue (IntPtr txtRecord, byte[] key)
    => OperatingSystem.IsWindows ()
         ? NativeWindows.TXTRecordRemoveValue (txtRecord: txtRecord, key: key)
         : OperatingSystem.IsMacOS ()
           ? NativeMacOS.TXTRecordRemoveValue (txtRecord: txtRecord, key: key)
           : NativeLinux.TXTRecordRemoveValue (txtRecord: txtRecord, key: key);

  public static ushort TXTRecordGetLength (IntPtr txtRecord)
    => OperatingSystem.IsWindows ()
         ? NativeWindows.TXTRecordGetLength (txtRecord)
         : OperatingSystem.IsMacOS ()
           ? NativeMacOS.TXTRecordGetLength (txtRecord)
           : NativeLinux.TXTRecordGetLength (txtRecord);

  public static IntPtr TXTRecordGetBytesPtr (IntPtr txtRecord)
    => OperatingSystem.IsWindows ()
         ? NativeWindows.TXTRecordGetBytesPtr (txtRecord)
         : OperatingSystem.IsMacOS ()
           ? NativeMacOS.TXTRecordGetBytesPtr (txtRecord)
           : NativeLinux.TXTRecordGetBytesPtr (txtRecord);

  public static ushort TXTRecordGetCount (ushort txtLen, IntPtr txtRecord)
    => OperatingSystem.IsWindows ()
         ? NativeWindows.TXTRecordGetCount (txtLen: txtLen, txtRecord: txtRecord)
         : OperatingSystem.IsMacOS ()
           ? NativeMacOS.TXTRecordGetCount (txtLen: txtLen, txtRecord: txtRecord)
           : NativeLinux.TXTRecordGetCount (txtLen: txtLen, txtRecord: txtRecord);
}
