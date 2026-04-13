#region header

// Arkane.ZeroConf - RegisterService.cs

#endregion

#region using

using System;

#endregion

namespace ArkaneSystems.Arkane.Zeroconf.Providers.WindowsMdns;

public sealed class RegisterService : IRegisterService
{
  public string Name { get; set; } = string.Empty;

  public string RegType { get; set; } = string.Empty;

  public string ReplyDomain { get; set; } = string.Empty;

  public ITxtRecord? TxtRecord { get; set; }

  public short Port { get; set; }

  public ushort UPort { get => (ushort)this.Port; set => this.Port = (short)value; }

  public event RegisterServiceEventHandler Response { add { } remove { } }

  public void Register ()
    => throw new
         PlatformNotSupportedException ("mDNS publishing is not available on this Windows fallback provider. Check ZeroconfSupport.CanPublish before calling RegisterService.Register().");

  public void Dispose () { }
}
