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

namespace ArkaneSystems.Arkane.Zeroconf.Providers.WindowsMdns;

public sealed class ServiceBrowser : IServiceBrowser
{
  private readonly ConcurrentDictionary<string, BrowseService> services = new (StringComparer.OrdinalIgnoreCase);
  private          AddressProtocol                             addressProtocol;

  private CancellationTokenSource? cts;
  private string?                  domain;

  private uint   interfaceIndex;
  private Task?  pollingTask;
  private string regtype = string.Empty;

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
    this.cts?.Cancel ();

    try { this.pollingTask?.Wait (); }
    catch (OperationCanceledException) { }
    catch (AggregateException ex) when (ex.InnerException is OperationCanceledException)
    {
      // Expected when polling is cancelled
    }

    this.cts?.Dispose ();

    // Dispose TxtRecord objects held by services before clearing the dictionary
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
      IReadOnlyList<MdnsClient.MdnsServiceInfo> discovered =
        await Task.Run (function: () => MdnsClient.Browse (regtype: this.regtype,
                                                           domain: this.domain ?? "local",
                                                           addressProtocol: this.addressProtocol,
                                                           cancellationToken: cancellationToken),
                        cancellationToken: cancellationToken)
                  .ConfigureAwait (false);

      var discoveredKeys = new HashSet<string> (collection: discovered.Select (service => service.FullName),
                                                comparer: StringComparer.OrdinalIgnoreCase);

      foreach (MdnsClient.MdnsServiceInfo discoveredService in discovered)
      {
        // Use TryAdd directly to avoid non-atomic check-then-act pattern
        var service = new BrowseService (name: discoveredService.Name,
                                         fullName: discoveredService.FullName,
                                         regType: discoveredService.RegType,
                                         replyDomain: discoveredService.ReplyDomain,
                                         interfaceIndex: this.interfaceIndex,
                                         addressProtocol: this.addressProtocol);

        if (this.services.TryAdd (key: discoveredService.FullName, value: service))
          this.ServiceAdded?.Invoke (o: this, args: new ServiceBrowseEventArgs (service));
      }

      // Snapshot keys before iteration to prevent InvalidOperationException from concurrent modification
      foreach (string existing in this.services.Keys.ToList ())
      {
        if (discoveredKeys.Contains (existing))
          continue;

        if (this.services.TryRemove (key: existing, value: out BrowseService? removed) && (removed != null))
          this.ServiceRemoved?.Invoke (o: this, args: new ServiceBrowseEventArgs (removed));
      }

      await Task.Delay (delay: TimeSpan.FromSeconds (5), cancellationToken: cancellationToken).ConfigureAwait (false);
    }
  }
}
