#region header

// Arkane.ZeroConf - RegisterService.cs

#endregion

#region using

using System;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

#endregion

namespace ArkaneSystems.Arkane.Zeroconf.Providers.Bonjour;

public sealed class RegisterService : Service, IRegisterService, IDisposable
{
  public RegisterService () { this.SetupCallback (); }

  public RegisterService (string name, string replyDomain, string regtype) : base (name: name,
                                                                                   replyDomain: replyDomain,
                                                                                   regtype: regtype)
  {
    this.SetupCallback ();
  }

  private readonly CancellationTokenSource cts = new ();

  private Native.DNSServiceRegisterReply registerReplyHandler;
  private ServiceRef                     sdRef;
  private Task                           task;
  private bool                           disposed;

  public bool AutoRename { get; set; } = true;

  public event RegisterServiceEventHandler Response;

  public void Register () { this.Register (true); }

  public void Dispose ()
  {
    if (this.disposed)
      return;

    this.disposed = true;

    this.cts.Cancel ();

    if (this.sdRef != ServiceRef.Zero)
    {
      this.sdRef.Deallocate ();
      this.sdRef = ServiceRef.Zero;
    }

    this.cts.Dispose ();
  }

  private void SetupCallback () { this.registerReplyHandler = this.OnRegisterReply; }

  public void Register (bool async)
  {
    if (this.task != null)
      throw new InvalidOperationException ("RegisterService registration already in process");

    if (this.disposed)
      throw new ObjectDisposedException (objectName: nameof (RegisterService));

    if (async)
      this.task = Task.Run (() => this.ProcessRegister ())
                      .ContinueWith (continuationFunction: _ => this.task = null, cancellationToken: this.cts.Token);
    else
      this.ProcessRegister ();
  }

  public void RegisterSync () { this.Register (false); }

  public void ProcessRegister ()
  {
    ushort txtRecLength = 0;
    byte[] txtRec       = null;

    if (this.TxtRecord != null)
    {
      txtRecLength = ((TxtRecord)this.TxtRecord.BaseRecord).RawLength;
      txtRec       = new byte[txtRecLength];
      Marshal.Copy (source: ((TxtRecord)this.TxtRecord.BaseRecord).RawBytes,
                    destination: txtRec,
                    startIndex: 0,
                    length: txtRecLength);
    }

    ServiceError error = Native.DNSServiceRegister (sdRef: out this.sdRef,
                                                    flags: this.AutoRename ? ServiceFlags.None : ServiceFlags.NoAutoRename,
                                                    interfaceIndex: this.InterfaceIndex,
                                                    name: Encoding.UTF8.GetBytes (this.Name),
                                                    regtype: this.RegType,
                                                    domain: this.ReplyDomain,
                                                    host: this.HostTarget,
                                                    port: (ushort)IPAddress.HostToNetworkOrder ((short)this.port),
                                                    txtLen: txtRecLength,
                                                    txtRecord: txtRec,
                                                    callBack: this.registerReplyHandler,
                                                    context: IntPtr.Zero);

    if (error != ServiceError.NoError)
      throw new ServiceErrorException (error);

    this.sdRef.Process ();
  }

  private void OnRegisterReply (ServiceRef   sdRef,
                                ServiceFlags flags,
                                ServiceError errorCode,
                                IntPtr       name,
                                string       regtype,
                                string       domain,
                                IntPtr       context)
  {
    var args = new RegisterServiceEventArgs { Service = this, IsRegistered = false, ServiceError = (ServiceErrorCode)errorCode, };

    if (errorCode == ServiceError.NoError)
    {
      this.Name         = Marshal.PtrToStringUTF8 (name);
      this.RegType      = regtype;
      this.ReplyDomain  = domain;
      args.IsRegistered = true;
    }

    RegisterServiceEventHandler handler = this.Response;
    handler?.Invoke (o: this, args: args);
  }
}
