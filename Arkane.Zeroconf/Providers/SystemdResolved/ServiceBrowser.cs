#region header

// Arkane.ZeroConf - ServiceBrowser.cs

#endregion

#region using

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

#endregion

namespace ArkaneSystems.Arkane.Zeroconf.Providers.SystemdResolved;

public sealed class ServiceBrowser : IServiceBrowser
{
  private static readonly TimeSpan DefaultPollingInterval = TimeSpan.FromSeconds (5);

  private static readonly object   pollingIntervalLock = new ();
  private static          TimeSpan pollingInterval      = DefaultPollingInterval;

  private readonly ConcurrentDictionary<string, BrowseService> services = new (StringComparer.OrdinalIgnoreCase);
  private          AddressProtocol                             addressProtocol;

  private bool                     _disposed;
  private CancellationTokenSource? cts;
  private string?                  domain;

  private uint   interfaceIndex;
  private Task?  pollingTask;
  private string regtype = string.Empty;

  /// <summary>
  /// Gets or sets the interval between systemd-resolved mDNS polling cycles.
  /// Default is 5 seconds.
  /// </summary>
  /// <remarks>
  /// <para>
  /// This property is thread-safe and can be configured before using the systemd-resolved service browser
  /// to tune discovery responsiveness and polling frequency.
  /// </para>
  /// <para>
  /// Changes to this property take effect on the next polling delay cycle.
  /// </para>
  /// </remarks>
  public static TimeSpan PollingInterval
  {
    get
    {
      lock (pollingIntervalLock)
        return pollingInterval;
    }
    set
    {
      lock (pollingIntervalLock)
        pollingInterval = value;
    }
  }

  public event ServiceBrowseEventHandler? ServiceAdded;

  public event ServiceBrowseEventHandler? ServiceRemoved;

  public void Browse (uint interfaceIndex, AddressProtocol addressProtocol, string regtype, string domain)
  {
    ArgumentNullException.ThrowIfNull (regtype);

    this.interfaceIndex  = interfaceIndex;
    this.addressProtocol = addressProtocol;
    this.regtype         = regtype;
    this.domain          = domain;

    this.StartPolling ();
  }

  public IEnumerator<IResolvableService> GetEnumerator ()
    => this.services.Values.Cast<IResolvableService> ().ToList ().GetEnumerator ();

  IEnumerator IEnumerable.GetEnumerator () => this.GetEnumerator ();

  public void Dispose ()
  {
    if (this._disposed)
      return;

    this._disposed = true;

    try { this.cts?.Cancel (); }
    catch { /* swallow exceptions from cancellation callbacks */ }

    try { this.pollingTask?.Wait (); }
    catch (OperationCanceledException) { }
    catch (AggregateException ex) when (ex.InnerException is OperationCanceledException)
    {
      // Expected when polling is cancelled
    }

    this.cts?.Dispose ();
    this.cts = null;

    foreach (BrowseService service in this.services.Values)
    {
      if (service.TxtRecord is IDisposable disposable)
      {
        try { disposable.Dispose (); }
        catch (Exception ex)
        {
          System.Diagnostics.Debug.WriteLine ($"Error disposing TxtRecord: {ex.Message}");
        }
      }
    }

    this.services.Clear ();
  }

  private void StartPolling ()
  {
    if (this.pollingTask != null)
      throw new InvalidOperationException ("ServiceBrowser is already started");

    this.cts = new CancellationTokenSource ();
    this.pollingTask = Task.Run (function: () => this.PollAsync (this.cts.Token), cancellationToken: this.cts.Token)
                           .ContinueWith (continuationFunction: _ => this.pollingTask = null, scheduler: TaskScheduler.Default);
  }

  private async Task PollAsync (CancellationToken cancellationToken)
  {
    while (!cancellationToken.IsCancellationRequested)
    {
      IReadOnlyList<SystemdResolvedClient.ServiceInstance> discovered;

      try
      {
        discovered = await Task.Run (function: () => SystemdResolvedClient.BrowseInstancesAsync (
                                       regtype: this.regtype,
                                       domain: this.domain ?? "local",
                                       cancellationToken: cancellationToken),
                                     cancellationToken: cancellationToken)
                               .ConfigureAwait (false);
      }
      catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
      {
        return;
      }
      catch (Exception ex)
      {
        System.Diagnostics.Debug.WriteLine ($"systemd-resolved browse error: {ex.Message}");
        discovered = Array.Empty<SystemdResolvedClient.ServiceInstance> ();
      }

      var discoveredKeys = new HashSet<string> (collection: discovered.Select (s => s.FullName),
                                                comparer: StringComparer.OrdinalIgnoreCase);

      foreach (SystemdResolvedClient.ServiceInstance discoveredService in discovered)
      {
        var service = new BrowseService (name: discoveredService.Name,
                                         fullName: discoveredService.FullName,
                                         regType: discoveredService.RegType,
                                         replyDomain: discoveredService.Domain,
                                         interfaceIndex: this.interfaceIndex,
                                         addressProtocol: this.addressProtocol);

        if (this.services.TryAdd (key: discoveredService.FullName, value: service))
          this.ServiceAdded?.Invoke (o: this, args: new ServiceBrowseEventArgs (service));
      }

      foreach (string existing in this.services.Keys.ToList ())
      {
        if (discoveredKeys.Contains (existing))
          continue;

        if (this.services.TryRemove (key: existing, value: out BrowseService? removed) && (removed != null))
          this.ServiceRemoved?.Invoke (o: this, args: new ServiceBrowseEventArgs (removed));
      }

      TimeSpan delay;
      lock (pollingIntervalLock)
        delay = pollingInterval;

      await Task.Delay (delay: delay, cancellationToken: cancellationToken).ConfigureAwait (false);
    }
  }
}
