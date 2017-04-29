#region header

// Arkane.Zeroconf - Service.cs
// 

#endregion

#region using

using System.Net ;

#endregion

namespace ArkaneSystems.Arkane.Zeroconf.Providers.Bonjour
{
    public abstract class Service : IService
    {
        public Service () { }

        public Service (string name, string replyDomain, string regtype)
        {
            this.Name = name ;
            this.ReplyDomain = replyDomain ;
            this.RegType = regtype ;
        }

        protected AddressProtocol address_protocol ;
        protected ServiceFlags flags = ServiceFlags.None ;
        protected string fullname ;
        protected IPHostEntry hostentry ;
        protected string hosttarget ;
        protected uint interface_index ;
        protected string name ;
        protected ushort port ;
        protected string regtype ;
        protected string reply_domain ;

        protected ITxtRecord txt_record ;

        public ServiceFlags Flags { get { return this.flags ; } internal set { this.flags = value ; } }

        public uint InterfaceIndex { get { return this.interface_index ; } set { this.interface_index = value ; } }

        public AddressProtocol AddressProtocol { get { return this.address_protocol ; } set { this.address_protocol = value ; } }

        public string Name { get { return this.name ; } set { this.name = value ; } }

        public string ReplyDomain { get { return this.reply_domain ; } set { this.reply_domain = value ; } }

        public string RegType { get { return this.regtype ; } set { this.regtype = value ; } }

        // Resolved Properties

        public ITxtRecord TxtRecord { get { return this.txt_record ; } set { this.txt_record = value ; } }

        public string FullName { get { return this.fullname ; } internal set { this.fullname = value ; } }

        public string HostTarget { get { return this.hosttarget ; } }

        public IPHostEntry HostEntry { get { return this.hostentry ; } }

        public uint NetworkInterface { get { return this.interface_index ; } }

        public short Port { get { return (short) this.UPort ; } set { this.UPort = (ushort) value ; } }

        public ushort UPort { get { return this.port ; } set { this.port = value ; } }

        public override bool Equals (object o)
        {
            if (!(o is Service))
                return false ;

            return (o as Service).Name == this.Name ;
        }

        public override int GetHashCode () { return this.Name.GetHashCode () ; }
    }
}
