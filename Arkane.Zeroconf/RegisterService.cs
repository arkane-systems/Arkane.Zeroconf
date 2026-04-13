#region header

// Arkane.ZeroConf - RegisterService.cs

#endregion

#region using

using System;

using ArkaneSystems.Arkane.Zeroconf.Providers;

#endregion

namespace ArkaneSystems.Arkane.Zeroconf;

public class RegisterService : IRegisterService
{
  /// <summary>
  /// Initializes a new instance of the <see cref="RegisterService"/> class.
  /// </summary>
  /// <remarks>
  /// <para>
  /// This constructor delegates to the available Zeroconf provider (e.g., Bonjour or Windows mDNS).
  /// The underlying provider instance is created via reflection using <see cref="Activator.CreateInstance"/>.
  /// </para>
  /// </remarks>
  /// <exception cref="ZeroconfException">
  /// Thrown when the Zeroconf provider's <see cref="IRegisterService"/> type cannot be instantiated.
  /// This typically indicates that no suitable Zeroconf provider (Bonjour, Avahi, or Windows mDNS) is available on the current system.
  /// Ensure that at least one supported mDNS/Bonjour implementation is installed and properly configured.
  /// </exception>
  public RegisterService ()
    => this.registerService = CreateRequiredInstance<IRegisterService> (ProviderFactory
                                                                       .SelectedProvider.RegisterService);

  private readonly IRegisterService registerService;

  public string Name { get => this.registerService.Name; set => this.registerService.Name = value; }

  public string RegType { get => this.registerService.RegType; set => this.registerService.RegType = value; }

  public string ReplyDomain { get => this.registerService.ReplyDomain; set => this.registerService.ReplyDomain = value; }

  public ITxtRecord? TxtRecord { get => this.registerService.TxtRecord; set => this.registerService.TxtRecord = value; }

  public short Port { get => this.registerService.Port; set => this.registerService.Port = value; }

  public ushort UPort { get => this.registerService.UPort; set => this.registerService.UPort = value; }

  public void Register () => this.registerService.Register ();

  public void Dispose () => this.registerService.Dispose ();

  public event RegisterServiceEventHandler Response
  {
    add => this.registerService.Response += value;
    remove => this.registerService.Response -= value;
  }

  private static T CreateRequiredInstance<T> (Type type) where T : class
    => Activator.CreateInstance (type) as T ??
       throw new ZeroconfException ("Unable to create the requested Zeroconf service. Ensure a compatible Zeroconf provider is installed and properly configured.");
}
