#region header

// Arkane.ZeroConf - IRegisterService.cs

#endregion

#region using

using System;

using JetBrains.Annotations;

#endregion

namespace ArkaneSystems.Arkane.Zeroconf;

/// <summary>
///   Represents a Zeroconf service that can be published on the local network.
/// </summary>
/// <remarks>
///   <para>
///     Publishing support depends on the selected provider. Bonjour-based providers support publishing,
///     while the Windows fallback provider does not.
///   </para>
///   <para>
///     Consumers should check <see cref="ZeroconfSupport.CanPublish" /> before attempting to register services.
///   </para>
/// </remarks>
[PublicAPI]
public interface IRegisterService : IService, IDisposable
{
  /// <summary>
  ///   Gets or sets the user-visible service instance name.
  /// </summary>
  new string Name { get; set; }

  /// <summary>
  ///   Gets or sets the Zeroconf registration type, such as <c>_http._tcp</c>.
  /// </summary>
  new string RegType { get; set; }

  /// <summary>
  ///   Gets or sets the reply domain for the service.
  /// </summary>
  new string ReplyDomain { get; set; }

  /// <summary>
  ///   Gets or sets the signed service port.
  /// </summary>
  short Port { get; set; }

  /// <summary>
  ///   Gets or sets the unsigned service port.
  /// </summary>
  ushort UPort { get; set; }

  /// <summary>
  ///   Occurs when the provider reports a registration response.
  /// </summary>
  event RegisterServiceEventHandler Response;

  /// <summary>
  ///   Registers the service on the local network.
  /// </summary>
  void Register ();
}
