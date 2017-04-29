#region header

// Arkane.Zeroconf - RegisterService.cs
// 

#endregion

#region using

using System ;
using System.Net ;
using System.Runtime.InteropServices ;
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

        private Native.DNSServiceRegisterReply register_reply_handler ;
        private ServiceRef sd_ref ;
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

            this.sd_ref.Deallocate () ;
        }

        private void SetupCallback () { this.register_reply_handler = this.OnRegisterReply ; }

        public void Register (bool async)
        {
            if (this.thread != null)
                throw new InvalidOperationException ("RegisterService registration already in process") ;

            if (async)
            {
                this.thread = new Thread (this.ThreadedRegister) ;
                this.thread.IsBackground = true ;
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
            ushort txt_rec_length = 0 ;
            byte[] txt_rec = null ;

            if (this.TxtRecord != null)
            {
                txt_rec_length = ((TxtRecord) this.TxtRecord.BaseRecord).RawLength ;
                txt_rec = new byte[txt_rec_length] ;
                Marshal.Copy (((TxtRecord) this.TxtRecord.BaseRecord).RawBytes, txt_rec, 0, txt_rec_length) ;
            }

            ServiceError error = Native.DNSServiceRegister (out this.sd_ref,
                                                            this.AutoRename ? ServiceFlags.None : ServiceFlags.NoAutoRename,
                                                            this.InterfaceIndex,
                                                            this.Name,
                                                            this.RegType,
                                                            this.ReplyDomain,
                                                            this.HostTarget,
                                                            (ushort) IPAddress.HostToNetworkOrder ((short) this.port),
                                                            txt_rec_length,
                                                            txt_rec,
                                                            this.register_reply_handler,
                                                            IntPtr.Zero) ;

            if (error != ServiceError.NoError)
                throw new ServiceErrorException (error) ;

            this.sd_ref.Process () ;
        }

        private void OnRegisterReply (ServiceRef sdRef,
                                      ServiceFlags flags,
                                      ServiceError errorCode,
                                      string name,
                                      string regtype,
                                      string domain,
                                      IntPtr context)
        {
            var args = new RegisterServiceEventArgs () ;

            args.Service = this ;
            args.IsRegistered = false ;
            args.ServiceError = (ServiceErrorCode) errorCode ;

            if (errorCode == ServiceError.NoError)
            {
                this.Name = name ;
                this.RegType = regtype ;
                this.ReplyDomain = domain ;
                args.IsRegistered = true ;
            }

            RegisterServiceEventHandler handler = this.Response ;
            if (handler != null)
                handler (this, args) ;
        }
    }
}
