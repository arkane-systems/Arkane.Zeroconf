#region header

// Arkane.ZeroConf - IServiceBrowser.cs

#endregion

#region using

using System;
using System.Collections.Generic;

using JetBrains.Annotations;

#endregion

namespace ArkaneSystems.Arkane.Zeroconf;

/// <summary>
///   Browses for Zeroconf services on the local network.
/// </summary>
/// <remarks>
///   <para>
///     Browsing behavior depends on the selected provider. Bonjour-based providers support full browse functionality,
///     while the Windows fallback provider supports browse and resolve only.
///   </para>
///   <para>
///     Consumers should check <see cref="ZeroconfSupport.CanBrowse" /> before starting browse operations.
///   </para>
/// </remarks>
[PublicAPI]
public interface IServiceBrowser : IEnumerable<IResolvableService>, IDisposable
{
  /// <summary>
  ///   Occurs when a service is discovered.
  /// </summary>
  event ServiceBrowseEventHandler ServiceAdded;

  /// <summary>
  ///   Occurs when a previously discovered service is no longer available.
  /// </summary>
  event ServiceBrowseEventHandler ServiceRemoved;

  /// <summary>
  ///   Starts browsing for services.
  /// </summary>
  /// <param name="interfaceIndex">The network interface index to browse on, or <c>0</c> for all interfaces.</param>
  /// <param name="addressProtocol">The address protocol to use when resolving services.</param>
  /// <param name="regtype">The Zeroconf registration type to browse for, such as <c>_http._tcp</c>.</param>
  /// <param name="domain">The domain to browse in, typically <c>local</c>.</param>
  void Browse (uint interfaceIndex, AddressProtocol addressProtocol, string regtype, string domain);
}
