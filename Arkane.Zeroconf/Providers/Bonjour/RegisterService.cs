#region header

// Arkane.Zeroconf - RegisterService.cs

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
  public RegisterService () => this.SetupCallback ();

  public RegisterService (string name, string replyDomain, string regtype) : base (name: name,
                                                                                   replyDomain: replyDomain,
                                                                                   regtype: regtype)
    => this.SetupCallback ();

  private readonly CancellationTokenSource cts      = new ();
  private readonly Lock                    syncRoot = new ();
  private          bool                    disposed;

  private Native.DNSServiceRegisterReply? registerReplyHandler;
  private ServiceRef                      sdRef;
  private Task?                           task;

  public bool AutoRename { get; set; } = true;

  public event RegisterServiceEventHandler? Response;

  public void Register () => this.Register (true);

  public void Dispose ()
  {
    if (this.disposed)
      return;

    this.disposed = true;

    this.cts.Cancel ();

    Task? registrationTask;

    lock (this.syncRoot)
      registrationTask = this.task;

    if (registrationTask != null)
      try { _ = registrationTask.Wait (TimeSpan.FromSeconds (1)); }
      catch (AggregateException aggregateException) when (aggregateException.InnerException is OperationCanceledException) { }
      catch (OperationCanceledException) { }

    lock (this.syncRoot)
    {
      if (this.sdRef != ServiceRef.Zero)
      {
        this.sdRef.Deallocate ();
        this.sdRef = ServiceRef.Zero;
      }
    }

    this.cts.Dispose ();
  }

  private void SetupCallback () => this.registerReplyHandler = this.OnRegisterReply;

  public void Register (bool async)
  {
    lock (this.syncRoot)
    {
      if (this.task != null)
        throw new InvalidOperationException ("RegisterService registration already in process");

      ObjectDisposedException.ThrowIf (condition: this.disposed, instance: this);

      if (async)
        this.task = Task.Run (() =>
                              {
                                try { this.ProcessRegister (); }
                                catch (OperationCanceledException) when (this.cts.IsCancellationRequested) { }
                                finally
                                {
                                  lock (this.syncRoot)
                                    this.task = null;
                                }
                              });
    }

    if (!async)
      this.ProcessRegister ();
  }

  public void RegisterSync () => this.Register (false);

  public void ProcessRegister ()
  {
    this.cts.Token.ThrowIfCancellationRequested ();

    ushort txtRecLength = 0;
    byte[] txtRec       = [];

    if (this.TxtRecord != null)
    {
      txtRecLength = ((TxtRecord)this.TxtRecord.BaseRecord).RawLength;
      txtRec       = new byte[txtRecLength];
      Marshal.Copy (source: ((TxtRecord)this.TxtRecord.BaseRecord).RawBytes,
                    destination: txtRec,
                    startIndex: 0,
                    length: txtRecLength);
    }

    ServiceError error = Native.DNSServiceRegister (sdRef: out ServiceRef localSdRef,
                                                    flags: this.AutoRename ? ServiceFlags.None : ServiceFlags.NoAutoRename,
                                                    interfaceIndex: this.InterfaceIndex,
                                                    name: Encoding.UTF8.GetBytes (this.Name),
                                                    regtype: this.RegType,
                                                    domain: this.ReplyDomain,
                                                    host: this.HostTarget ?? string.Empty,
                                                    port: (ushort)IPAddress.HostToNetworkOrder ((short)this.port),
                                                    txtLen: txtRecLength,
                                                    txtRecord: txtRec,
                                                    callBack: this.registerReplyHandler ??
                                                              throw new
                                                                InvalidOperationException ("Register callback is not initialized."),
                                                    context: IntPtr.Zero);

    if (error != ServiceError.NoError)
      throw new ServiceErrorException (error);

    lock (this.syncRoot)
    {
      if (this.disposed)
      {
        localSdRef.Deallocate ();

        return;
      }

      this.sdRef = localSdRef;
    }

    try { localSdRef.Process (this.cts.Token); }
    finally
    {
      lock (this.syncRoot)
      {
        if (this.sdRef == localSdRef)
          this.sdRef = ServiceRef.Zero;
      }
    }
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
      this.Name         = Marshal.PtrToStringUTF8 (name) ?? string.Empty;
      this.RegType      = regtype;
      this.ReplyDomain  = domain;
      args.IsRegistered = true;
    }

    RegisterServiceEventHandler? handler = this.Response;
    handler?.Invoke (o: this, args: args);
  }
}
