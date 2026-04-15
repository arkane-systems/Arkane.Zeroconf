#region header

// Arkane.Zeroconf - BrowseService.cs

#endregion

#region using

using System.Net;
using System.Threading;
using System.Threading.Tasks;

#endregion

namespace ArkaneSystems.Arkane.Zeroconf.Providers.WindowsMdns;

public sealed class BrowseService (
  string          name,
  string          fullName,
  string          regType,
  string          replyDomain,
  uint            interfaceIndex,
  AddressProtocol addressProtocol)
  : IResolvableService
{
  public string Name { get; } = name;

  public string RegType { get; } = regType;

  public string ReplyDomain { get; } = replyDomain;

  public ITxtRecord? TxtRecord { get; set; }

  public string FullName { get; } = fullName;

  public IPHostEntry? HostEntry { get; private set; }

  public string? HostTarget { get; private set; }

  public uint NetworkInterface { get; } = interfaceIndex;

  public AddressProtocol AddressProtocol { get; } = addressProtocol;

  public short Port => (short)this.UPort;

  public ushort UPort { get; private set; }

  public event ServiceResolvedEventHandler? Resolved;

  public void Resolve () => this.ResolveAsync ().ConfigureAwait (false).GetAwaiter ().GetResult ();

  public async Task ResolveAsync (CancellationToken cancellationToken = default)
  {
    cancellationToken.ThrowIfCancellationRequested ();

    MdnsClient.MdnsResolveResult result =
      await Task.Run (function: () => MdnsClient.Resolve (service: new MdnsClient.MdnsServiceInfo
                                                                   {
                                                                     Name           = this.Name,
                                                                     FullName       = this.FullName,
                                                                     RegType        = this.RegType,
                                                                     ReplyDomain    = this.ReplyDomain,
                                                                     InterfaceIndex = this.NetworkInterface,
                                                                   },
                                                          addressProtocol: this.AddressProtocol,
                                                          cancellationToken: cancellationToken),
                      cancellationToken: cancellationToken)
                .ConfigureAwait (false);

    if (!result.IsResolved)
      return;

    this.HostTarget = result.HostName;
    this.UPort      = result.Port;
    this.HostEntry  = new IPHostEntry { HostName = result.HostName ?? string.Empty, AddressList = result.Addresses };
    this.TxtRecord  = WindowsMdns.TxtRecord.FromEntries (result.TxtEntries);

    this.Resolved?.Invoke (o: this, args: new ServiceResolvedEventArgs (this));
  }
}
