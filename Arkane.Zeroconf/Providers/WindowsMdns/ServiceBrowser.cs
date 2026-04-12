#region header

// Arkane.ZeroConf - ServiceBrowser.cs
// 

#endregion

#region using

using System ;
using System.Collections ;
using System.Collections.Concurrent ;
using System.Collections.Generic ;
using System.Linq ;
using System.Threading ;
using System.Threading.Tasks ;

#endregion

namespace ArkaneSystems.Arkane.Zeroconf.Providers.WindowsMdns ;

public sealed class ServiceBrowser : IServiceBrowser
{
    private readonly ConcurrentDictionary <string, BrowseService> services = new (StringComparer.OrdinalIgnoreCase) ;

    private CancellationTokenSource cts ;
    private Task pollingTask ;

    private uint interfaceIndex ;
    private AddressProtocol addressProtocol ;
    private string regtype ;
    private string domain ;

    public event ServiceBrowseEventHandler ServiceAdded ;

    public event ServiceBrowseEventHandler ServiceRemoved ;

    public void Browse (uint interfaceIndex, AddressProtocol addressProtocol, string regtype, string domain)
    {
        ArgumentNullException.ThrowIfNull (regtype) ;

        this.interfaceIndex = interfaceIndex ;
        this.addressProtocol = addressProtocol ;
        this.regtype = regtype ;
        this.domain = domain ;

        this.StartPolling () ;
    }

    public IEnumerator <IResolvableService> GetEnumerator () => this.services.Values.Cast <IResolvableService> ().ToList ().GetEnumerator () ;

    IEnumerator IEnumerable.GetEnumerator () => this.GetEnumerator () ;

    public void Dispose ()
    {
        this.cts?.Cancel () ;

        try
        {
            this.pollingTask?.Wait () ;
        }
        catch (AggregateException)
        {
        }

        this.cts?.Dispose () ;
        this.services.Clear () ;
    }

    private void StartPolling ()
    {
        if (this.pollingTask != null)
            throw new InvalidOperationException ("ServiceBrowser is already started") ;

        this.cts = new CancellationTokenSource () ;
        this.pollingTask = Task.Run (() => this.PollAsync (this.cts.Token), this.cts.Token)
                               .ContinueWith (_ => this.pollingTask = null, TaskScheduler.Default) ;
    }

    private async Task PollAsync (CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var discovered = await Task.Run (() => MdnsClient.Browse (this.regtype, this.domain, this.addressProtocol, cancellationToken), cancellationToken)
                                       .ConfigureAwait (false) ;

            var discoveredKeys = new HashSet <string> (discovered.Select (service => service.FullName), StringComparer.OrdinalIgnoreCase) ;

            foreach (var discoveredService in discovered)
            {
                if (this.services.ContainsKey (discoveredService.FullName))
                    continue ;

                var service = new BrowseService (discoveredService.Name,
                                                 discoveredService.FullName,
                                                 discoveredService.RegType,
                                                 discoveredService.ReplyDomain,
                                                 this.interfaceIndex,
                                                 this.addressProtocol) ;

                if (this.services.TryAdd (discoveredService.FullName, service))
                    this.ServiceAdded?.Invoke (this, new Arkane.Zeroconf.ServiceBrowseEventArgs (service)) ;
            }

            foreach (var existing in this.services.Keys)
            {
                if (discoveredKeys.Contains (existing))
                    continue ;

                if (this.services.TryRemove (existing, out var removed))
                    this.ServiceRemoved?.Invoke (this, new Arkane.Zeroconf.ServiceBrowseEventArgs (removed)) ;
            }

            await Task.Delay (TimeSpan.FromSeconds (5), cancellationToken).ConfigureAwait (false) ;
        }
    }
}
