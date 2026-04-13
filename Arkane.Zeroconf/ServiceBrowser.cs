#region header

// Arkane.ZeroConf - ServiceBrowser.cs

#endregion

#region using

using System;
using System.Collections;
using System.Collections.Generic;

using ArkaneSystems.Arkane.Zeroconf.Providers;

#endregion

namespace ArkaneSystems.Arkane.Zeroconf;

public class ServiceBrowser : IServiceBrowser
{
  public ServiceBrowser ()
    => this.browser = (IServiceBrowser)Activator.CreateInstance (ProviderFactory.SelectedProvider.ServiceBrowser);

  private readonly IServiceBrowser browser;

  public void Dispose () { this.browser.Dispose (); }

  public void Browse (uint interfaceIndex, AddressProtocol addressProtocol, string regtype, string domain)
  {
    this.browser.Browse (interfaceIndex: interfaceIndex,
                         addressProtocol: addressProtocol,
                         regtype: regtype,
                         domain: domain ?? "local");
  }

  public IEnumerator<IResolvableService> GetEnumerator () => this.browser.GetEnumerator ();

  IEnumerator IEnumerable.GetEnumerator () => this.browser.GetEnumerator ();

  public event ServiceBrowseEventHandler ServiceAdded
  {
    add => this.browser.ServiceAdded += value;
    remove => this.browser.ServiceAdded -= value;
  }

  public event ServiceBrowseEventHandler ServiceRemoved
  {
    add => this.browser.ServiceRemoved += value;
    remove => this.browser.ServiceRemoved -= value;
  }

  public void Browse (uint interfaceIndex, string regtype, string domain)
  {
    this.Browse (interfaceIndex: interfaceIndex, addressProtocol: AddressProtocol.Any, regtype: regtype, domain: domain);
  }

  public void Browse (AddressProtocol addressProtocol, string regtype, string domain)
  {
    this.Browse (interfaceIndex: 0, addressProtocol: addressProtocol, regtype: regtype, domain: domain);
  }

  public void Browse (string regtype, string domain)
  {
    this.Browse (interfaceIndex: 0, addressProtocol: AddressProtocol.Any, regtype: regtype, domain: domain);
  }
}
