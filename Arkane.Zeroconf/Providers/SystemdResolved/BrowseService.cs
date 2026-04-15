#region header

// Arkane.ZeroConf - BrowseService.cs

#endregion

#region using

using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

#endregion

namespace ArkaneSystems.Arkane.Zeroconf.Providers.SystemdResolved;

/// <summary>
///   Represents a service discovered by the systemd-resolved mDNS browser that can be resolved
///   to full host, port, address, and TXT record information.
/// </summary>
/// <remarks>
///   <para>
///     Resolution is performed asynchronously via <see cref="ResolveAsync" /> using the
///     <c>org.freedesktop.resolve1.Manager.ResolveService</c> D-Bus call.
///   </para>
///   <para>
///     The fire-and-forget <see cref="Resolve" /> overload starts resolution in the background
///     and fires the <see cref="Resolved" /> event on completion; any D-Bus or timeout errors
///     are swallowed and logged via <see cref="System.Diagnostics.Debug" />.
///   </para>
///   <para>
///     Properties <see cref="HostEntry" />, <see cref="HostTarget" />, <see cref="UPort" />,
///     and <see cref="IResolvableService.TxtRecord" /> remain <see langword="null" /> / zero
///     until a successful resolution completes.
///   </para>
/// </remarks>
public sealed class BrowseService : IResolvableService
{
  public BrowseService (string          name,
                        string          fullName,
                        string          regType,
                        string          replyDomain,
                        uint            interfaceIndex,
                        AddressProtocol addressProtocol)
  {
    this.Name             = name;
    this.FullName         = fullName;
    this.RegType          = regType;
    this.ReplyDomain      = replyDomain;
    this.NetworkInterface = interfaceIndex;
    this.AddressProtocol  = addressProtocol;
  }

  public string Name { get; }

  public string RegType { get; }

  public string ReplyDomain { get; }

  public ITxtRecord? TxtRecord { get; set; }

  public string FullName { get; }

  public IPHostEntry? HostEntry { get; private set; }

  public string? HostTarget { get; private set; }

  public uint NetworkInterface { get; }

  public AddressProtocol AddressProtocol { get; }

  public short Port => (short)this.UPort;

  public ushort UPort { get; private set; }

  public event ServiceResolvedEventHandler? Resolved;

  /// <summary>
  ///   Starts resolution in the background.  Fires the <see cref="Resolved" /> event on success.
  ///   D-Bus and timeout errors are swallowed and logged via <see cref="System.Diagnostics.Debug" />.
  /// </summary>
  public void Resolve () => _ = Task.Run (() => this.ResolveAsync ());

  /// <summary>
  ///   Resolves this service instance via <c>org.freedesktop.resolve1.Manager.ResolveService</c>
  ///   and populates <see cref="HostEntry" />, <see cref="HostTarget" />, <see cref="UPort" />,
  ///   and <see cref="IResolvableService.TxtRecord" />.
  /// </summary>
  /// <param name="cancellationToken">
  ///   Token to cancel the operation.  When the token is already cancelled on entry the method
  ///   throws <see cref="OperationCanceledException" /> immediately.  Internal D-Bus timeouts
  ///   are treated as non-fatal failures and are not propagated to the caller.
  /// </param>
  /// <exception cref="OperationCanceledException">
  ///   The <paramref name="cancellationToken" /> was cancelled before the operation completed.
  /// </exception>
  public async Task ResolveAsync (CancellationToken cancellationToken = default)
  {
    cancellationToken.ThrowIfCancellationRequested ();

    try
    {
      ResolveManagerProxy.ServiceResolveResult? result =
        await SystemdResolvedClient.ResolveInstanceAsync (name: this.Name,
                                                          regtype: this.RegType,
                                                          domain: this.ReplyDomain ?? "local",
                                                          family: 0,
                                                          cancellationToken: cancellationToken)
                                   .ConfigureAwait (false);

      if (result == null)
        return;

      this.HostTarget = result.HostName;
      this.UPort      = result.Port;
      this.HostEntry  = new IPHostEntry { HostName = result.HostName ?? string.Empty, AddressList = result.Addresses };
      this.TxtRecord  = SystemdResolved.TxtRecord.FromEntries (result.TxtItems);

      this.Resolved?.Invoke (o: this, args: new ServiceResolvedEventArgs (this));
    }
    catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
    {
      throw;
    }
    catch (Exception ex)
    {
      System.Diagnostics.Debug.WriteLine ($"systemd-resolved service resolve failed for '{this.Name}': {ex.Message}");
    }
  }
}
