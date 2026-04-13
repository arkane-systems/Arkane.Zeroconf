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
  /// <summary>
  /// Initializes a new instance of the <see cref="ServiceBrowser"/> class.
  /// </summary>
  /// <remarks>
  /// <para>
  /// This constructor delegates to the available Zeroconf provider (e.g., Bonjour or Windows mDNS).
  /// The underlying provider instance is created via reflection using <see cref="Activator.CreateInstance"/>.
  /// </para>
  /// </remarks>
  /// <exception cref="InvalidOperationException">
  /// Thrown when the Zeroconf provider's <see cref="IServiceBrowser"/> type cannot be instantiated.
  /// This typically indicates that no suitable Zeroconf provider (Bonjour, Avahi, or Windows mDNS) is available on the current system.
  /// Ensure that at least one supported mDNS/Bonjour implementation is installed and properly configured.
  /// </exception>
  public ServiceBrowser ()
    => this.browser = CreateRequiredInstance<IServiceBrowser> (ProviderFactory.SelectedProvider.ServiceBrowser);

  private readonly IServiceBrowser browser;

  public void Dispose () => this.browser.Dispose ();

  public void Browse (uint interfaceIndex, AddressProtocol addressProtocol, string regtype, string domain)
    => this.browser.Browse (interfaceIndex: interfaceIndex,
                            addressProtocol: addressProtocol,
                            regtype: regtype,
                            domain: domain ?? "local");

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

  private static T CreateRequiredInstance<T> (Type type) where T : class
    => Activator.CreateInstance (type) as T ??
       throw new InvalidOperationException ($"Unable to create instance of '{type.FullName}'.");

  public void Browse (uint interfaceIndex, string regtype, string domain)
    => this.Browse (interfaceIndex: interfaceIndex, addressProtocol: AddressProtocol.Any, regtype: regtype, domain: domain);

  public void Browse (AddressProtocol addressProtocol, string regtype, string domain)
    => this.Browse (interfaceIndex: 0, addressProtocol: addressProtocol, regtype: regtype, domain: domain);

  public void Browse (string regtype, string domain)
    => this.Browse (interfaceIndex: 0, addressProtocol: AddressProtocol.Any, regtype: regtype, domain: domain);
}
