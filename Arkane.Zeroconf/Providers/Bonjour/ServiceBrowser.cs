#region header

// Arkane.ZeroConf - ServiceBrowser.cs

#endregion

#region using

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

#endregion

namespace ArkaneSystems.Arkane.Zeroconf.Providers.Bonjour;

public class ServiceBrowseEventArgs : Arkane.Zeroconf.ServiceBrowseEventArgs
{
  public ServiceBrowseEventArgs (BrowseService service, bool moreComing) : base (service) => this.MoreComing = moreComing;

  public bool MoreComing { get; }
}

public class ServiceBrowser : IServiceBrowser, IDisposable
{
  public ServiceBrowser () => this.browseReplyHandler = this.OnBrowseReply;

  private readonly Native.DNSServiceBrowseReply           browseReplyHandler;
  private readonly Dictionary<string, IResolvableService> serviceTable          = new ();
  private readonly SemaphoreSlim                          serviceTableSemaphore = new (initialCount: 1, maxCount: 1);
  private readonly CancellationTokenSource                stopTokenSource       = new ();

  private AddressProtocol address_protocol;
  private bool            disposed;
  private string?         domain;
  private uint            interfaceIndex;
  private string?         regtype;

  private ServiceRef sdRef = ServiceRef.Zero;

  private Task? task;

  public event ServiceBrowseEventHandler? ServiceAdded;

  public event ServiceBrowseEventHandler? ServiceRemoved;

  public void Browse (uint interfaceIndex, AddressProtocol addressProtocol, string regtype, string domain)
  {
    if (this.disposed)
      throw new ObjectDisposedException (objectName: nameof (ServiceBrowser));

    this.Configure (interfaceIndex: interfaceIndex, addressProtocol: addressProtocol, regtype: regtype, domain: domain);
    this.StartAsync ();
  }

  public void Dispose ()
  {
    if (this.disposed)
      return;

    this.Stop ();
    this.stopTokenSource.Dispose ();
    this.serviceTableSemaphore.Dispose ();
    this.disposed = true;
  }

  public IEnumerator<IResolvableService> GetEnumerator ()
  {
    this.serviceTableSemaphore.Wait ();

    try
    {
      foreach (IResolvableService service in this.serviceTable.Values)
        yield return service;
    }
    finally { this.serviceTableSemaphore.Release (); }
  }

  IEnumerator IEnumerable.GetEnumerator () => this.GetEnumerator ();

  public void Configure (uint interfaceIndex, AddressProtocol addressProtocol, string regtype, string domain)
  {
    this.interfaceIndex   = interfaceIndex;
    this.address_protocol = addressProtocol;
    this.regtype          = regtype;
    this.domain           = domain;

    if (regtype == null)
      throw new ArgumentNullException ("regtype");
  }

  private void Start (bool async)
  {
    if ((this.task != null) && !this.task.IsCompleted)
      throw new InvalidOperationException ("ServiceBrowser is already started");

    if (this.disposed)
      throw new ObjectDisposedException (objectName: nameof (ServiceBrowser));

    if (async)
      this.task = Task.Run (action: () => this.ProcessStart (this.stopTokenSource.Token),
                            cancellationToken: this.stopTokenSource.Token);
    else
      this.ProcessStart (this.stopTokenSource.Token);
  }

  public void Start () { this.Start (false); }

  public void StartAsync () { this.Start (true); }

  private void ProcessStart (CancellationToken cancellationToken)
  {
    ServiceError error = Native.DNSServiceBrowse (sdRef: out this.sdRef,
                                                  flags: ServiceFlags.None,
                                                  interfaceIndex: this.interfaceIndex,
                                                  regtype: this.regtype ?? string.Empty,
                                                  domain: this.domain   ?? string.Empty,
                                                  callBack: this.browseReplyHandler,
                                                  context: IntPtr.Zero);

    if (error != ServiceError.NoError)
      throw new ServiceErrorException (error);

    try { this.sdRef.Process (cancellationToken); }
    catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { }
  }

  public void Stop ()
  {
    this.stopTokenSource.Cancel ();

    if (this.sdRef != ServiceRef.Zero)
    {
      this.sdRef.Deallocate ();
      this.sdRef = ServiceRef.Zero;
    }

    if (this.task != null)
    {
      try { this.task.Wait (TimeSpan.FromSeconds (2)); }
      catch (AggregateException ex) when (ex.InnerException is OperationCanceledException ||
                                          ex.InnerException is ServiceErrorException) { }

      this.task = null;
    }
  }

  private void OnBrowseReply (ServiceRef   sdRef,
                              ServiceFlags flags,
                              uint         interfaceIndex,
                              ServiceError errorCode,
                              IntPtr       serviceName,
                              string       regtype,
                              string       replyDomain,
                              IntPtr       context)
  {
    string name = Marshal.PtrToStringUTF8 (serviceName) ?? string.Empty;

    var service = new BrowseService
                  {
                    Flags           = flags,
                    Name            = name,
                    RegType         = regtype,
                    ReplyDomain     = replyDomain,
                    InterfaceIndex  = interfaceIndex,
                    AddressProtocol = this.address_protocol,
                  };

    var args = new ServiceBrowseEventArgs (service: service,
                                           moreComing: (flags & ServiceFlags.MoreComing) != 0);

    if ((flags & ServiceFlags.Add) != 0)
    {
      this.serviceTableSemaphore.Wait ();

      try { this.serviceTable[name] = service; }
      finally { this.serviceTableSemaphore.Release (); }

      ServiceBrowseEventHandler? handler = this.ServiceAdded;
      handler?.Invoke (o: this, args: args);
    }
    else
    {
      this.serviceTableSemaphore.Wait ();

      try { this.serviceTable.Remove (name); }
      finally { this.serviceTableSemaphore.Release (); }

      ServiceBrowseEventHandler? handler = this.ServiceRemoved;
      handler?.Invoke (o: this, args: args);
    }
  }
}
