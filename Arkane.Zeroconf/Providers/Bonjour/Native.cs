#region header

// Arkane.Zeroconf - Native.cs
// 

#endregion

#region using

using System ;
using System.Runtime.InteropServices ;

#endregion

namespace ArkaneSystems.Arkane.Zeroconf.Providers.Bonjour
{
    public static class Native
    {
        #region Nested type: DNSServiceBrowseReply

        // DNSServiceBrowse

        public delegate void DNSServiceBrowseReply (ServiceRef sdRef,
                                                    ServiceFlags flags,
                                                    uint interfaceIndex,
                                                    ServiceError errorCode,
                                                    string serviceName,
                                                    string regtype,
                                                    string replyDomain,
                                                    IntPtr context) ;

        #endregion

        #region Nested type: DNSServiceQueryRecordReply

        // DNSServiceQueryRecord

        public delegate void DNSServiceQueryRecordReply (ServiceRef sdRef,
                                                         ServiceFlags flags,
                                                         uint interfaceIndex,
                                                         ServiceError errorCode,
                                                         string fullname,
                                                         ServiceType rrtype,
                                                         ServiceClass rrclass,
                                                         ushort rdlen,
                                                         IntPtr rdata,
                                                         uint ttl,
                                                         IntPtr context) ;

        #endregion

        #region Nested type: DNSServiceRegisterReply

        // DNSServiceRegister

        public delegate void DNSServiceRegisterReply (ServiceRef sdRef,
                                                      ServiceFlags flags,
                                                      ServiceError errorCode,
                                                      string name,
                                                      string regtype,
                                                      string domain,
                                                      IntPtr context) ;

        #endregion

        #region Nested type: DNSServiceResolveReply

        // DNSServiceResolve

        public delegate void DNSServiceResolveReply (ServiceRef sdRef,
                                                     ServiceFlags flags,
                                                     uint interfaceIndex,
                                                     ServiceError errorCode,
                                                     string fullname,
                                                     string hosttarget,
                                                     ushort port,
                                                     ushort txtLen,
                                                     IntPtr txtRecord,
                                                     IntPtr context) ;

        #endregion

        // ServiceRef

        [DllImport ("dnssd.dll")]
        public static extern void DNSServiceRefDeallocate (IntPtr sdRef) ;

        [DllImport ("dnssd.dll")]
        public static extern ServiceError DNSServiceProcessResult (IntPtr sdRef) ;

        [DllImport ("dnssd.dll")]
        public static extern int DNSServiceRefSockFD (IntPtr sdRef) ;

        [DllImport ("dnssd.dll")]
        public static extern ServiceError DNSServiceCreateConnection (out ServiceRef sdRef) ;

        [DllImport ("dnssd.dll")]
        public static extern ServiceError DNSServiceBrowse (out ServiceRef sdRef,
                                                            ServiceFlags flags,
                                                            uint interfaceIndex,
                                                            string regtype,
                                                            string domain,
                                                            DNSServiceBrowseReply callBack,
                                                            IntPtr context) ;

        [DllImport ("dnssd.dll")]
        public static extern ServiceError DNSServiceResolve (out ServiceRef sdRef,
                                                             ServiceFlags flags,
                                                             uint interfaceIndex,
                                                             string name,
                                                             string regtype,
                                                             string domain,
                                                             DNSServiceResolveReply callBack,
                                                             IntPtr context) ;

        [DllImport ("dnssd.dll")]
        public static extern ServiceError DNSServiceRegister (out ServiceRef sdRef,
                                                              ServiceFlags flags,
                                                              uint interfaceIndex,
                                                              string name,
                                                              string regtype,
                                                              string domain,
                                                              string host,
                                                              ushort port,
                                                              ushort txtLen,
                                                              byte[] txtRecord,
                                                              DNSServiceRegisterReply callBack,
                                                              IntPtr context) ;

        [DllImport ("dnssd.dll")]
        public static extern ServiceError DNSServiceQueryRecord (out ServiceRef sdRef,
                                                                 ServiceFlags flags,
                                                                 uint interfaceIndex,
                                                                 string fullname,
                                                                 ServiceType rrtype,
                                                                 ServiceClass rrclass,
                                                                 DNSServiceQueryRecordReply callBack,
                                                                 IntPtr context) ;

        // TXT Record Handling

        [DllImport ("dnssd.dll")]
        public static extern void TXTRecordCreate (IntPtr txtRecord, ushort bufferLen, IntPtr buffer) ;

        [DllImport ("dnssd.dll")]
        public static extern void TXTRecordDeallocate (IntPtr txtRecord) ;

        [DllImport ("dnssd.dll")]
        public static extern ServiceError TXTRecordGetItemAtIndex (ushort txtLen,
                                                                   IntPtr txtRecord,
                                                                   ushort index,
                                                                   ushort keyBufLen,
                                                                   byte[] key,
                                                                   out byte valueLen,
                                                                   out IntPtr value) ;

        [DllImport ("dnssd.dll")]
        public static extern ServiceError TXTRecordSetValue (IntPtr txtRecord,
                                                             byte[] key,
                                                             sbyte valueSize,
                                                             byte[] value) ;

        [DllImport ("dnssd.dll")]
        public static extern ServiceError TXTRecordRemoveValue (IntPtr txtRecord, byte[] key) ;

        [DllImport ("dnssd.dll")]
        public static extern ushort TXTRecordGetLength (IntPtr txtRecord) ;

        [DllImport ("dnssd.dll")]
        public static extern IntPtr TXTRecordGetBytesPtr (IntPtr txtRecord) ;

        [DllImport ("dnssd.dll")]
        public static extern ushort TXTRecordGetCount (ushort txtLen, IntPtr txtRecord) ;
    }
}
