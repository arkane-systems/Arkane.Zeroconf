#region header

// Arkane.ZeroConf - IRegisterService.cs
// 

#endregion

#region using

using System ;

#endregion

namespace ArkaneSystems.Arkane.Zeroconf ;

public interface IRegisterService : IService, IDisposable
{
    new string Name { get ; set ; }

    new string RegType { get ; set ; }

    new string ReplyDomain { get ; set ; }

    short Port { get ; set ; }

    ushort UPort { get ; set ; }

    event RegisterServiceEventHandler Response ;

    void Register () ;
}
