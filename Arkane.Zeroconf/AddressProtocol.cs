#region header

// Arkane.ZeroConf - AddressProtocol.cs

#endregion

// ReSharper disable InconsistentNaming
namespace ArkaneSystems.Arkane.Zeroconf;

/// <summary>
///   Specifies which IP address families should be used when resolving services.
/// </summary>
public enum AddressProtocol
{
  /// <summary>
  ///   Use any supported address family.
  /// </summary>
  Any,

  /// <summary>
  ///   Use IPv4 addresses only.
  /// </summary>
  IPv4,

  /// <summary>
  ///   Use IPv6 addresses only.
  /// </summary>
  IPv6,
}
