#region header

// Arkane.ZeroConf - Service.cs
// 

#endregion

#region using

using System.Net ;

#endregion

namespace ArkaneSystems.Arkane.Zeroconf.Providers.Bonjour ;

public abstract class Service : IService
{
    public Service () { }

    public Service (string name, string replyDomain, string regtype)
    {
        this.Name        = name ;
        this.ReplyDomain = replyDomain ;
        this.RegType     = regtype ;
    }

    protected AddressProtocol address_protocol ;
    protected ServiceFlags    flags = ServiceFlags.None ;
    protected string          fullname = string.Empty ;
    protected IPHostEntry?    hostentry ;
    protected string?         hosttarget ;
    protected uint            interface_index ;
    protected string          name = string.Empty ;
    protected ushort          port ;
    protected string          regtype = string.Empty ;
    protected string          reply_domain = string.Empty ;

    protected ITxtRecord? txt_record ;

    public ServiceFlags Flags { get => this.flags ; internal set => this.flags = value ; }

    public uint InterfaceIndex { get => this.interface_index ; set => this.interface_index = value ; }

    public AddressProtocol AddressProtocol { get => this.address_protocol ; set => this.address_protocol = value ; }

    public string Name { get => this.name ; set => this.name = value ?? string.Empty ; }

    public string ReplyDomain { get => this.reply_domain ; set => this.reply_domain = value ?? string.Empty ; }

    public string RegType { get => this.regtype ; set => this.regtype = value ?? string.Empty ; }

    // Resolved Properties

    public ITxtRecord? TxtRecord { get => this.txt_record ; set => this.txt_record = value ; }

    public string FullName { get => this.fullname ; internal set => this.fullname = value ?? string.Empty ; }

    public string? HostTarget => this.hosttarget ;

    public IPHostEntry? HostEntry => this.hostentry ;

    public uint NetworkInterface => this.interface_index ;

    public short Port { get => (short) this.UPort ; set => this.UPort = (ushort) value ; }

    public ushort UPort { get => this.port ; set => this.port = value ; }

    public override bool Equals (object? o)
    {
        if (o is not Service service)
            return false ;

        return service.Name == this.Name ;
    }

    public override int GetHashCode () => this.Name.GetHashCode () ;
}
