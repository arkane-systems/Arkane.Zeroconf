#region header

// Arkane.Zeroconf - RegisterService.cs
// 

#endregion

#region using

using System ;

using ArkaneSystems.Arkane.Zeroconf.Providers ;

#endregion

namespace ArkaneSystems.Arkane.Zeroconf
{
    public class RegisterService : IRegisterService
    {
        public RegisterService ()
        {
            this.register_service = (IRegisterService) Activator.CreateInstance (
                                                                                 ProviderFactory.SelectedProvider.RegisterService) ;
        }

        private readonly IRegisterService register_service ;

        public string Name { get { return this.register_service.Name ; } set { this.register_service.Name = value ; } }

        public string RegType { get { return this.register_service.RegType ; } set { this.register_service.RegType = value ; } }

        public string ReplyDomain
        {
            get { return this.register_service.ReplyDomain ; }
            set { this.register_service.ReplyDomain = value ; }
        }

        public ITxtRecord TxtRecord
        {
            get { return this.register_service.TxtRecord ; }
            set { this.register_service.TxtRecord = value ; }
        }

        public short Port { get { return this.register_service.Port ; } set { this.register_service.Port = value ; } }

        public ushort UPort { get { return this.register_service.UPort ; } set { this.register_service.UPort = value ; } }

        public void Register () { this.register_service.Register () ; }

        public void Dispose () { this.register_service.Dispose () ; }

        public event RegisterServiceEventHandler Response
        {
            add { this.register_service.Response += value ; }
            remove { this.register_service.Response -= value ; }
        }
    }
}
