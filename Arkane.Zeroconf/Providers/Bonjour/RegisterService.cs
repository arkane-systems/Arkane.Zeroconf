#region header

// Arkane.ZeroConf - RegisterService.cs
// 

#endregion

#region using

using System ;
using System.Net ;
using System.Runtime.InteropServices ;
using System.Text ;
using System.Threading ;
using System.Threading.Tasks ;

#endregion

namespace ArkaneSystems.Arkane.Zeroconf.Providers.Bonjour ;

public sealed class RegisterService : Service, IRegisterService, IDisposable
{
    public RegisterService () { this.SetupCallback () ; }

    public RegisterService (string name, string replyDomain, string regtype) : base (name, replyDomain, regtype)
    {
        this.SetupCallback () ;
    }

    private readonly CancellationTokenSource cts = new() ;

    private Native.DNSServiceRegisterReply registerReplyHandler ;
    private ServiceRef                     sdRef ;
    private Task                           task ;

    public bool AutoRename { get ; set ; } = true ;

    public event RegisterServiceEventHandler Response ;

    public void Register () { this.Register (true) ; }

    public void Dispose ()
    {
        this.cts?.Cancel () ;

        this.cts.Dispose () ;
        this.sdRef.Deallocate () ;
    }

    private void SetupCallback () { this.registerReplyHandler = this.OnRegisterReply ; }

    public void Register (bool async)
    {
        if (this.task != null)
            throw new InvalidOperationException ("RegisterService registration already in process") ;

        if (async)
            this.task = Task.Run (() => this.ProcessRegister ()).ContinueWith (_ => this.task = null, this.cts.Token) ;
        else
            this.ProcessRegister () ;
    }

    public void RegisterSync () { this.Register (false) ; }

    public void ProcessRegister ()
    {
        ushort txtRecLength = 0 ;
        byte[] txtRec       = null ;

        if (this.TxtRecord != null)
        {
            txtRecLength = ((TxtRecord) this.TxtRecord.BaseRecord).RawLength ;
            txtRec       = new byte[txtRecLength] ;
            Marshal.Copy (((TxtRecord) this.TxtRecord.BaseRecord).RawBytes, txtRec, 0, txtRecLength) ;
        }

        var error = Native.DNSServiceRegister (out this.sdRef,
                                               this.AutoRename ? ServiceFlags.None : ServiceFlags.NoAutoRename,
                                               this.InterfaceIndex,
                                               Encoding.UTF8.GetBytes (this.Name),
                                               this.RegType,
                                               this.ReplyDomain,
                                               this.HostTarget,
                                               (ushort) IPAddress.HostToNetworkOrder ((short) this.port),
                                               txtRecLength,
                                               txtRec,
                                               this.registerReplyHandler,
                                               IntPtr.Zero) ;

        if (error != ServiceError.NoError)
            throw new ServiceErrorException (error) ;

        this.sdRef.Process () ;
    }

    private void OnRegisterReply (ServiceRef   sdRef,
                                  ServiceFlags flags,
                                  ServiceError errorCode,
                                  IntPtr       name,
                                  string       regtype,
                                  string       domain,
                                  IntPtr       context)
    {
        var args = new RegisterServiceEventArgs
                   {
                       Service = this, IsRegistered = false, ServiceError = (ServiceErrorCode) errorCode,
                   } ;


        if (errorCode == ServiceError.NoError)
        {
            this.Name         = Marshal.PtrToStringUTF8(name) ;
            this.RegType      = regtype ;
            this.ReplyDomain  = domain ;
            args.IsRegistered = true ;
        }

        var handler = this.Response ;
        handler?.Invoke (this, args) ;
    }
}
