#region header

// Arkane.ZeroConf - BrowseService.cs

#endregion

#region using

using System.Net;
using System.Threading;
using System.Threading.Tasks;

#endregion

namespace ArkaneSystems.Arkane.Zeroconf.Providers.WindowsMdns;

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
