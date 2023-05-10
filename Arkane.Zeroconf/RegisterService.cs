#region header

// Arkane.ZeroConf - RegisterService.cs
// 

#endregion

#region using

using System ;

using ArkaneSystems.Arkane.Zeroconf.Providers ;

#endregion

namespace ArkaneSystems.Arkane.Zeroconf ;

public class RegisterService : IRegisterService
{
    public RegisterService () => this.registerService =
                                     (IRegisterService) Activator.CreateInstance (ProviderFactory
                                                                                 .SelectedProvider.RegisterService) ;

    private readonly IRegisterService registerService ;

    public string Name { get => this.registerService.Name ; set => this.registerService.Name = value ; }

    public string RegType { get => this.registerService.RegType ; set => this.registerService.RegType = value ; }

    public string ReplyDomain { get => this.registerService.ReplyDomain ; set => this.registerService.ReplyDomain = value ; }

    public ITxtRecord TxtRecord { get => this.registerService.TxtRecord ; set => this.registerService.TxtRecord = value ; }

    public short Port { get => this.registerService.Port ; set => this.registerService.Port = value ; }

    public ushort UPort { get => this.registerService.UPort ; set => this.registerService.UPort = value ; }

    public void Register () { this.registerService.Register () ; }

    public void Dispose () { this.registerService.Dispose () ; }

    public event RegisterServiceEventHandler Response
    {
        add => this.registerService.Response += value ;
        remove => this.registerService.Response -= value ;
    }
}
