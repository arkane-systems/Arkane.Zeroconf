#region header

// Arkane.Zeroconf - RegisterService.cs
// 

#endregion

#region using

using System ;
using System.Net ;
using System.Runtime.InteropServices ;
using System.Text;
using System.Threading ;

#endregion

namespace ArkaneSystems.Arkane.Zeroconf.Providers.Bonjour
{
    public sealed class RegisterService : Service, IRegisterService, IDisposable
    {
        public RegisterService () { this.SetupCallback () ; }

        public RegisterService (string name, string replyDomain, string regtype) : base (name, replyDomain, regtype)
        {
            this.SetupCallback () ;
        }

        private Native.DNSServiceRegisterReply registerReplyHandler ;
        private ServiceRef sdRef ;
        private Thread thread ;

        public bool AutoRename { get ; set ; } = true ;

        public event RegisterServiceEventHandler Response ;

        public void Register () { this.Register (true) ; }

        public void Dispose ()
        {
            if (this.thread != null)
            {
                this.thread.Abort () ;
                this.thread = null ;
            }

            this.sdRef.Deallocate () ;
        }

        private void SetupCallback () { this.registerReplyHandler = this.OnRegisterReply ; }

        public void Register (bool async)
        {
            if (this.thread != null)
                throw new InvalidOperationException ("RegisterService registration already in process") ;

            if (async)
            {
                this.thread = new Thread (this.ThreadedRegister) {IsBackground = true} ;
                this.thread.Start () ;
            }
            else
            {
                this.ProcessRegister () ;
            }
        }

        public void RegisterSync () { this.Register (false) ; }

        private void ThreadedRegister ()
        {
            try
            {
                this.ProcessRegister () ;
            }
            catch (ThreadAbortException)
            {
                Thread.ResetAbort () ;
            }

            this.thread = null ;
        }

        public void ProcessRegister ()
        {
            ushort txtRecLength = 0 ;
            byte[] txtRec = null ;

            if (this.TxtRecord != null)
            {
                txtRecLength = ((TxtRecord) this.TxtRecord.BaseRecord).RawLength ;
                txtRec = new byte[txtRecLength] ;
                Marshal.Copy (((TxtRecord) this.TxtRecord.BaseRecord).RawBytes, txtRec, 0, txtRecLength) ;
            }

            ServiceError error = Native.DNSServiceRegister (out this.sdRef,
                                                            this.AutoRename ? ServiceFlags.None : ServiceFlags.NoAutoRename,
                                                            this.InterfaceIndex,
                                                            Encoding.UTF8.GetBytes(this.Name),
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

        private void OnRegisterReply (ServiceRef sdRef,
                                      ServiceFlags flags,
                                      ServiceError errorCode,
                                      IntPtr name,
                                      string regtype,
                                      string domain,
                                      IntPtr context)
        {
            var args = new RegisterServiceEventArgs
                       {
                           Service      = this,
                           IsRegistered = false,
                           ServiceError = (ServiceErrorCode) errorCode
                       } ;


            if (errorCode == ServiceError.NoError)
            {
                this.Name = Native.Utf8toString(name) ;
                this.RegType = regtype ;
                this.ReplyDomain = domain ;
                args.IsRegistered = true ;
            }

            RegisterServiceEventHandler handler = this.Response ;
            handler?.Invoke (this, args) ;
        }
    }
}
