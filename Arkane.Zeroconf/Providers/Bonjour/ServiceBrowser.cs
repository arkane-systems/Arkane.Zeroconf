#region header

// Arkane.Zeroconf - ServiceBrowser.cs

#endregion

#region using

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

#endregion

namespace ArkaneSystems.Arkane.Zeroconf.Providers.Bonjour;

public class ServiceBrowseEventArgs (BrowseService service, bool moreComing) : Arkane.Zeroconf.ServiceBrowseEventArgs (service)
{
  public bool MoreComing { get; } = moreComing;
}

public class ServiceBrowser : IServiceBrowser, IDisposable
{
  public ServiceBrowser () => this.browseReplyHandler = this.OnBrowseReply;

  private readonly Native.DNSServiceBrowseReply           browseReplyHandler;
  private readonly Dictionary<string, IResolvableService> serviceTable          = new ();
  private readonly SemaphoreSlim                          serviceTableSemaphore = new (initialCount: 1, maxCount: 1);
  private readonly CancellationTokenSource                stopTokenSource       = new ();

  private AddressProtocol addressProtocol;
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
    ObjectDisposedException.ThrowIf (condition: this.disposed, instance: this);

    this.Configure (interfaceIndex: interfaceIndex, addressProtocol: addressProtocol, regtype: regtype, domain: domain);
    this.StartAsync ();
  }

  public void Dispose ()
  {
    if (this.disposed)
      return;

    this.Stop ();
    this.stopTokenSource.Dispose ();

    // Dispose TxtRecord objects held by services before clearing the dictionary
    try
    {
      _ = this.serviceTableSemaphore.Wait (TimeSpan.FromSeconds (1));

      try
      {
        foreach (IResolvableService service in this.serviceTable.Values)
        {
          if (service.TxtRecord is IDisposable disposable)
            try { disposable.Dispose (); }
            catch (Exception ex) { Debug.WriteLine ($"Error disposing TxtRecord: {ex.Message}"); }
        }

        this.serviceTable.Clear ();
      }
      finally { _ = this.serviceTableSemaphore.Release (); }
    }
    catch (OperationCanceledException)
    {
      // Semaphore wait was cancelled or timed out, clear anyway
      this.serviceTable.Clear ();
    }

    this.serviceTableSemaphore.Dispose ();
    this.disposed = true;

    GC.SuppressFinalize (this);
  }

  public IEnumerator<IResolvableService> GetEnumerator ()
  {
    this.serviceTableSemaphore.Wait ();

    try
    {
      foreach (IResolvableService service in this.serviceTable.Values)
        yield return service;
    }
    finally { _ = this.serviceTableSemaphore.Release (); }
  }

  IEnumerator IEnumerable.GetEnumerator () => this.GetEnumerator ();

  public void Configure (uint interfaceIndex, AddressProtocol addressProtocol, string regtype, string domain)
  {
    this.interfaceIndex  = interfaceIndex;
    this.addressProtocol = addressProtocol;
    this.regtype         = regtype;
    this.domain          = domain;

    ArgumentNullException.ThrowIfNull (regtype);
  }

  private void Start (bool async)
  {
    if (this.task is { IsCompleted: false })
      throw new InvalidOperationException ("ServiceBrowser is already started");

    ObjectDisposedException.ThrowIf (condition: this.disposed, instance: this);

    if (async)
      this.task = Task.Run (action: () => this.ProcessStart (this.stopTokenSource.Token),
                            cancellationToken: this.stopTokenSource.Token);
    else
      this.ProcessStart (this.stopTokenSource.Token);
  }

  public void Start () => this.Start (false);

  public void StartAsync () => this.Start (true);

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

    if (this.task is { IsCompleted: false })
    {
      try { _ = this.task.Wait (TimeSpan.FromSeconds (2)); }
      catch (AggregateException ex) when (ex.InnerException is OperationCanceledException or ServiceErrorException) { }

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
                    AddressProtocol = this.addressProtocol,
                  };

    var args = new ServiceBrowseEventArgs (service: service,
                                           moreComing: (flags & ServiceFlags.MoreComing) != 0);

    if ((flags & ServiceFlags.Add) != 0)
    {
      this.serviceTableSemaphore.Wait ();

      try { this.serviceTable[name] = service; }
      finally { _                   = this.serviceTableSemaphore.Release (); }

      ServiceBrowseEventHandler? handler = this.ServiceAdded;
      handler?.Invoke (o: this, args: args);
    }
    else
    {
      this.serviceTableSemaphore.Wait ();

      try { _     = this.serviceTable.Remove (name); }
      finally { _ = this.serviceTableSemaphore.Release (); }

      ServiceBrowseEventHandler? handler = this.ServiceRemoved;
      handler?.Invoke (o: this, args: args);
    }
  }
}
