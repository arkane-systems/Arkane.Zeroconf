#region header

// Arkane.ZeroConf - ServiceBrowser.cs
// 

#endregion

#region using

using System ;
using System.Collections ;
using System.Collections.Generic ;

using ArkaneSystems.Arkane.Zeroconf.Providers ;

#endregion

namespace ArkaneSystems.Arkane.Zeroconf ;

public class ServiceBrowser : IServiceBrowser
{
    public ServiceBrowser () =>
        this.browser = (IServiceBrowser) Activator.CreateInstance (ProviderFactory.SelectedProvider.ServiceBrowser) ;

    private readonly IServiceBrowser browser ;

    public void Dispose () { this.browser.Dispose () ; }

    public void Browse (uint interfaceIndex, AddressProtocol addressProtocol, string regtype, string domain)
    {
        this.browser.Browse (interfaceIndex, addressProtocol, regtype, domain ?? "local") ;
    }

    public IEnumerator <IResolvableService> GetEnumerator () => this.browser.GetEnumerator () ;

    IEnumerator IEnumerable.GetEnumerator () => this.browser.GetEnumerator () ;

    public event ServiceBrowseEventHandler ServiceAdded
    {
        add => this.browser.ServiceAdded += value ;
        remove => this.browser.ServiceRemoved -= value ;
    }

    public event ServiceBrowseEventHandler ServiceRemoved
    {
        add => this.browser.ServiceRemoved += value ;
        remove => this.browser.ServiceRemoved -= value ;
    }

    public void Browse (uint interfaceIndex, string regtype, string domain)
    {
        this.Browse (interfaceIndex, AddressProtocol.Any, regtype, domain) ;
    }

    public void Browse (AddressProtocol addressProtocol, string regtype, string domain)
    {
        this.Browse (0, addressProtocol, regtype, domain) ;
    }

    public void Browse (string regtype, string domain) { this.Browse (0, AddressProtocol.Any, regtype, domain) ; }
}
