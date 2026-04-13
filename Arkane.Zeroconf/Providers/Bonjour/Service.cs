#region header

// Arkane.ZeroConf - Service.cs

#endregion

#region using

using System.Net;

#endregion

namespace ArkaneSystems.Arkane.Zeroconf.Providers.Bonjour;

public abstract class Service : IService
{
  public Service () { }

  public Service (string name, string replyDomain, string regtype)
  {
    this.Name        = name;
    this.ReplyDomain = replyDomain;
    this.RegType     = regtype;
  }

  protected AddressProtocol addressProtocol;
  protected ServiceFlags    flags    = ServiceFlags.None;
  protected string          fullName = string.Empty;
  protected IPHostEntry?    hostEntry;
  protected string?         hostTarget;
  protected uint            interfaceIndex;
  protected string          name = string.Empty;
  protected ushort          port;
  protected string          regType      = string.Empty;
  protected string          replyDomain = string.Empty;

  protected ITxtRecord? txtRecord;

  public ServiceFlags Flags { get => this.flags; internal set => this.flags = value; }

  public uint InterfaceIndex { get => this.interfaceIndex; set => this.interfaceIndex = value; }

  public AddressProtocol AddressProtocol { get => this.addressProtocol; set => this.addressProtocol = value; }

  public string Name { get => this.name; set => this.name = value ?? string.Empty; }

  public string ReplyDomain { get => this.replyDomain; set => this.replyDomain = value ?? string.Empty; }

  public string RegType { get => this.regType; set => this.regType = value ?? string.Empty; }

  // Resolved Properties

  public ITxtRecord? TxtRecord { get => this.txtRecord; set => this.txtRecord = value; }

  public string FullName { get => this.fullName; internal set => this.fullName = value ?? string.Empty; }

  public string? HostTarget => this.hostTarget;

  public IPHostEntry? HostEntry => this.hostEntry;

  public uint NetworkInterface => this.interfaceIndex;

  public short Port { get => (short)this.UPort; set => this.UPort = (ushort)value; }

  public ushort UPort { get => this.port; set => this.port = value; }

  public override bool Equals (object? o)
  {
    if (o is not Service service)
      return false;

    return service.Name == this.Name;
  }

  public override int GetHashCode () => this.Name.GetHashCode ();
}
