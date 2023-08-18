#region header

// Arkane.ZeroConf - BrowseService.cs
// 

#endregion

#region using

using System ;
using System.Collections ;
using System.Net ;
using System.Runtime.InteropServices ;
using System.Text ;

#endregion

namespace ArkaneSystems.Arkane.Zeroconf.Providers.Bonjour ;

public sealed class BrowseService : Service, IResolvableService
{
    public BrowseService () { this.SetupCallbacks () ; }

    public BrowseService (string name, string replyDomain, string regtype) : base (name, replyDomain, regtype)
    {
        this.SetupCallbacks () ;
    }

    private Native.DNSServiceQueryRecordReply queryRecordReplyHandler ;

    private Action <bool> resolveAction ;
    private bool          resolvePending ;

    private Native.DNSServiceResolveReply resolveReplyHandler ;

    private IAsyncResult resolveResult ;

    public bool IsResolved { get ; private set ; }

    public event ServiceResolvedEventHandler Resolved ;

    public void Resolve ()
    {
        // If people call this in a ServiceAdded event handler (which they generally do), we need to
        // invoke onto another thread, otherwise we block processing any more results.
        this.resolveResult = this.resolveAction.BeginInvoke (false, null, null) ;
    }

    ~BrowseService ()
    {
        if (this.resolveResult != null)
            this.resolveAction.EndInvoke (this.resolveResult) ;
    }

    private void SetupCallbacks ()
    {
        this.resolveReplyHandler     = this.OnResolveReply ;
        this.queryRecordReplyHandler = this.OnQueryRecordReply ;
        this.resolveAction           = this.Resolve ;
    }

    public void Resolve (bool requery)
    {
        if (this.resolvePending)
            return ;

        this.IsResolved     = false ;
        this.resolvePending = true ;

        if (requery)
            this.InterfaceIndex = 0 ;

        var error = Native.DNSServiceResolve (out var sdRef,
                                              ServiceFlags.None,
                                              this.InterfaceIndex,
                                              Encoding.UTF8.GetBytes (this.Name),
                                              this.RegType,
                                              this.ReplyDomain,
                                              this.resolveReplyHandler,
                                              IntPtr.Zero) ;

        if (error != ServiceError.NoError)
            throw new ServiceErrorException (error) ;

        sdRef.Process () ;
    }

    public void RefreshTxtRecord ()
    {
        // Should probably make this async?

        var error = Native.DNSServiceQueryRecord (out var sdRef,
                                                  ServiceFlags.None,
                                                  0,
                                                  this.fullname,
                                                  ServiceType.TXT,
                                                  ServiceClass.IN,
                                                  this.queryRecordReplyHandler,
                                                  IntPtr.Zero) ;

        if (error != ServiceError.NoError)
            throw new ServiceErrorException (error) ;

        sdRef.Process () ;
    }

    private void OnResolveReply (ServiceRef   sdRef,
                                 ServiceFlags flags,
                                 uint         interfaceIndex,
                                 ServiceError errorCode,
                                 IntPtr       fullname,
                                 string       hosttarget,
                                 ushort       port,
                                 ushort       txtLen,
                                 IntPtr       txtRecord,
                                 IntPtr       contex)
    {
        this.IsResolved     = true ;
        this.resolvePending = false ;

        this.InterfaceIndex = interfaceIndex ;
        this.FullName       = Marshal.PtrToStringUTF8 (fullname) ;
        this.port           = (ushort) IPAddress.NetworkToHostOrder ((short) port) ;
        this.TxtRecord      = new TxtRecord (txtLen, txtRecord) ;
        this.hosttarget     = hosttarget ;

        sdRef.Deallocate () ;

        // Run an A query to resolve the IP address
        ServiceRef sd_ref ;

        if ((this.AddressProtocol == AddressProtocol.Any) || (this.AddressProtocol == AddressProtocol.IPv4))
        {
            var error = Native.DNSServiceQueryRecord (out sd_ref,
                                                      ServiceFlags.None,
                                                      interfaceIndex,
                                                      hosttarget,
                                                      ServiceType.A,
                                                      ServiceClass.IN,
                                                      this.queryRecordReplyHandler,
                                                      IntPtr.Zero) ;

            if (error != ServiceError.NoError)
                throw new ServiceErrorException (error) ;

            sd_ref.Process () ;
        }

        if ((this.AddressProtocol == AddressProtocol.Any) || (this.AddressProtocol == AddressProtocol.IPv6))
        {
            var error = Native.DNSServiceQueryRecord (out sd_ref,
                                                      ServiceFlags.None,
                                                      interfaceIndex,
                                                      hosttarget,
                                                      ServiceType.AAAA,
                                                      ServiceClass.IN,
                                                      this.queryRecordReplyHandler,
                                                      IntPtr.Zero) ;

            if (error != ServiceError.NoError)
                throw new ServiceErrorException (error) ;

            sd_ref.Process () ;
        }

        if (this.hostentry.AddressList != null)
        {
            var handler = this.Resolved ;
            handler?.Invoke (this, new ServiceResolvedEventArgs (this)) ;
        }
    }

    private void OnQueryRecordReply (ServiceRef   sdRef,
                                     ServiceFlags flags,
                                     uint         interfaceIndex,
                                     ServiceError errorCode,
                                     string       fullname,
                                     ServiceType  rrtype,
                                     ServiceClass rrclass,
                                     ushort       rdlen,
                                     IntPtr       rdata,
                                     uint         ttl,
                                     IntPtr       context)
    {
        switch (rrtype)
        {
            case ServiceType.A:
            case ServiceType.AAAA:
                IPAddress address ;

                if (rdlen == 4)
                {
                    // ~4.5 times faster than Marshal.Copy into byte[4]
                    var addressRaw = (uint) (Marshal.ReadByte (rdata, 3) << 24) ;
                    addressRaw |= (uint) (Marshal.ReadByte (rdata, 2) << 16) ;
                    addressRaw |= (uint) (Marshal.ReadByte (rdata, 1) << 8) ;
                    addressRaw |= Marshal.ReadByte (rdata, 0) ;

                    address = new IPAddress (addressRaw) ;
                }
                else if (rdlen == 16)
                {
                    var addressRaw = new byte[rdlen] ;
                    Marshal.Copy (rdata, addressRaw, 0, rdlen) ;
                    address = new IPAddress (addressRaw, interfaceIndex) ;
                }
                else
                {
                    break ;
                }

                if (this.hostentry == null)
                    this.hostentry = new IPHostEntry { HostName = this.hosttarget } ;

                if (this.hostentry.AddressList != null)
                {
                    var list = new ArrayList (this.hostentry.AddressList) { address } ;
                    this.hostentry.AddressList = list.ToArray (typeof (IPAddress)) as IPAddress[] ;
                }
                else
                {
                    this.hostentry.AddressList = new[] { address } ;
                }

                //ServiceResolvedEventHandler handler = this.Resolved ;
                //if (handler != null)
                //    handler (this, new ServiceResolvedEventArgs (this)) ;

                break ;
            case ServiceType.TXT:
                this.TxtRecord?.Dispose () ;

                this.TxtRecord = new TxtRecord (rdlen, rdata) ;
                break ;
        }

        if ((flags & ServiceFlags.MoreComing) != ServiceFlags.MoreComing)
            sdRef.Deallocate () ;
    }
}
