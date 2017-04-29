#region header

// Arkane.Zeroconf - BrowseService.cs
// 

#endregion

#region using

using System ;
using System.Collections ;
using System.Net ;
using System.Runtime.InteropServices ;

#endregion

namespace ArkaneSystems.Arkane.Zeroconf.Providers.Bonjour
{
    public sealed class BrowseService : Service, IResolvableService
    {
        public BrowseService () { this.SetupCallbacks () ; }

        public BrowseService (string name, string replyDomain, string regtype) : base (name, replyDomain, regtype)
        {
            this.SetupCallbacks () ;
        }

        private Native.DNSServiceQueryRecordReply query_record_reply_handler ;
        private bool resolve_pending ;

        private Native.DNSServiceResolveReply resolve_reply_handler ;

        public bool IsResolved { get ; private set ; }

        public event ServiceResolvedEventHandler Resolved ;

        public void Resolve () { this.Resolve (false) ; }

        private void SetupCallbacks ()
        {
            this.resolve_reply_handler = this.OnResolveReply ;
            this.query_record_reply_handler = this.OnQueryRecordReply ;
        }

        public void Resolve (bool requery)
        {
            if (this.resolve_pending)
                return ;

            this.IsResolved = false ;
            this.resolve_pending = true ;

            if (requery)
                this.InterfaceIndex = 0 ;

            ServiceRef sd_ref ;
            ServiceError error = Native.DNSServiceResolve (out sd_ref,
                                                           ServiceFlags.None,
                                                           this.InterfaceIndex,
                                                           this.Name,
                                                           this.RegType,
                                                           this.ReplyDomain,
                                                           this.resolve_reply_handler,
                                                           IntPtr.Zero) ;

            if (error != ServiceError.NoError)
                throw new ServiceErrorException (error) ;

            sd_ref.Process () ;
        }

        public void RefreshTxtRecord ()
        {
            // Should probably make this async?

            ServiceRef sd_ref ;
            ServiceError error = Native.DNSServiceQueryRecord (out sd_ref,
                                                               ServiceFlags.None,
                                                               0,
                                                               this.fullname,
                                                               ServiceType.TXT,
                                                               ServiceClass.IN,
                                                               this.query_record_reply_handler,
                                                               IntPtr.Zero) ;

            if (error != ServiceError.NoError)
                throw new ServiceErrorException (error) ;

            sd_ref.Process () ;
        }

        private void OnResolveReply (ServiceRef sdRef,
                                     ServiceFlags flags,
                                     uint interfaceIndex,
                                     ServiceError errorCode,
                                     string fullname,
                                     string hosttarget,
                                     ushort port,
                                     ushort txtLen,
                                     IntPtr txtRecord,
                                     IntPtr contex)
        {
            this.IsResolved = true ;
            this.resolve_pending = false ;

            this.InterfaceIndex = interfaceIndex ;
            this.FullName = fullname ;
            this.port = port ;
            this.TxtRecord = new TxtRecord (txtLen, txtRecord) ;

            sdRef.Deallocate () ;

            // Run an A query to resolve the IP address
            ServiceRef sd_ref ;

            if ((this.AddressProtocol == AddressProtocol.Any) || (this.AddressProtocol == AddressProtocol.IPv4))
            {
                ServiceError error = Native.DNSServiceQueryRecord (out sd_ref,
                                                                   ServiceFlags.None,
                                                                   interfaceIndex,
                                                                   hosttarget,
                                                                   ServiceType.A,
                                                                   ServiceClass.IN,
                                                                   this.query_record_reply_handler,
                                                                   IntPtr.Zero) ;

                if (error != ServiceError.NoError)
                    throw new ServiceErrorException (error) ;

                sd_ref.Process () ;
            }

            if ((this.AddressProtocol == AddressProtocol.Any) || (this.AddressProtocol == AddressProtocol.IPv6))
            {
                ServiceError error = Native.DNSServiceQueryRecord (out sd_ref,
                                                                   ServiceFlags.None,
                                                                   interfaceIndex,
                                                                   hosttarget,
                                                                   ServiceType.AAAA,
                                                                   ServiceClass.IN,
                                                                   this.query_record_reply_handler,
                                                                   IntPtr.Zero) ;

                if (error != ServiceError.NoError)
                    throw new ServiceErrorException (error) ;

                sd_ref.Process () ;
            }
        }

        private void OnQueryRecordReply (ServiceRef sdRef,
                                         ServiceFlags flags,
                                         uint interfaceIndex,
                                         ServiceError errorCode,
                                         string fullname,
                                         ServiceType rrtype,
                                         ServiceClass rrclass,
                                         ushort rdlen,
                                         IntPtr rdata,
                                         uint ttl,
                                         IntPtr context)
        {
            switch (rrtype)
            {
                case ServiceType.A:
                    IPAddress address ;

                    if (rdlen == 4)
                    {
                        // ~4.5 times faster than Marshal.Copy into byte[4]
                        var address_raw = (uint) (Marshal.ReadByte (rdata, 3) << 24) ;
                        address_raw |= (uint) (Marshal.ReadByte (rdata, 2) << 16) ;
                        address_raw |= (uint) (Marshal.ReadByte (rdata, 1) << 8) ;
                        address_raw |= Marshal.ReadByte (rdata, 0) ;

                        address = new IPAddress (address_raw) ;
                    }
                    else if (rdlen == 16)
                    {
                        var address_raw = new byte[rdlen] ;
                        Marshal.Copy (rdata, address_raw, 0, rdlen) ;
                        address = new IPAddress (address_raw, interfaceIndex) ;
                    }
                    else
                    {
                        break ;
                    }

                    if (this.hostentry == null)
                    {
                        this.hostentry = new IPHostEntry () ;
                        this.hostentry.HostName = this.hosttarget ;
                    }

                    if (this.hostentry.AddressList != null)
                    {
                        var list = new ArrayList (this.hostentry.AddressList) ;
                        list.Add (address) ;
                        this.hostentry.AddressList = list.ToArray (typeof (IPAddress)) as IPAddress[] ;
                    }
                    else
                    {
                        this.hostentry.AddressList = new[] {address} ;
                    }

                    ServiceResolvedEventHandler handler = this.Resolved ;
                    if (handler != null)
                        handler (this, new ServiceResolvedEventArgs (this)) ;

                    break ;
                case ServiceType.TXT:
                    if (this.TxtRecord != null)
                        this.TxtRecord.Dispose () ;

                    this.TxtRecord = new TxtRecord (rdlen, rdata) ;
                    break ;
                default:
                    break ;
            }

            sdRef.Deallocate () ;
        }
    }
}
