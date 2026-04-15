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

  public void Resolve () => _ = Task.Run (() => this.ResolveAsync ());

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
