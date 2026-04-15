#region header

// Arkane.ZeroConf - IResolvableService.cs

#endregion

#region using

using System.Net;
using System.Threading;
using System.Threading.Tasks;

using JetBrains.Annotations;

#endregion

namespace ArkaneSystems.Arkane.Zeroconf;

/// <summary>
///   Represents a browsed Zeroconf service that can be resolved to host and port information.
/// </summary>
/// <remarks>
///   <para>
///     Resolution populates network-specific data such as host names, addresses, and port information.
///     Some providers may require an explicit call to <see cref="Resolve" /> or <see cref="ResolveAsync" /> before
///     these properties are available.
///   </para>
///   <para>
///     On Windows fallback browsing, resolution is supported but publishing is not.
///   </para>
/// </remarks>
[PublicAPI]
public interface IResolvableService : IService
{
  /// <summary>
  ///   Gets the fully qualified service name.
  /// </summary>
  string FullName { get; }

  /// <summary>
  ///   Gets the resolved host entry for the service, if available.
  /// </summary>
  IPHostEntry? HostEntry { get; }

  /// <summary>
  ///   Gets the target host name returned by service resolution, if available.
  /// </summary>
  string? HostTarget { get; }

  /// <summary>
  ///   Gets the network interface index associated with the service.
  /// </summary>
  uint NetworkInterface { get; }

  /// <summary>
  ///   Gets the address protocol associated with the service lookup.
  /// </summary>
  AddressProtocol AddressProtocol { get; }

  /// <summary>
  ///   Gets the resolved service port.
  /// </summary>
  short Port { get; }

  /// <summary>
  ///   Occurs when the service has been resolved.
  /// </summary>
  event ServiceResolvedEventHandler Resolved;

  /// <summary>
  ///   Resolves the service synchronously.
  /// </summary>
  void Resolve ();

  /// <summary>
  ///   Resolves the service asynchronously.
  /// </summary>
  /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
  /// <returns>A task that completes when resolution finishes.</returns>
  /// <remarks>
  ///   The default implementation delegates to <see cref="Resolve" /> after checking cancellation.
  ///   Provider-specific implementations may override this method with true asynchronous behavior.
  /// </remarks>
  Task ResolveAsync (CancellationToken cancellationToken = default)
  {
    cancellationToken.ThrowIfCancellationRequested ();
    this.Resolve ();

    return Task.CompletedTask;
  }
}
