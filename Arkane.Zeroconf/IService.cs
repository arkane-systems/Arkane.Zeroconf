#region header

// Arkane.ZeroConf - IService.cs

#endregion

namespace ArkaneSystems.Arkane.Zeroconf;

/// <summary>
///   Represents a discovered or registered Zeroconf service.
/// </summary>
/// <remarks>
///   Implementations expose the service identity shared by both browse and registration scenarios.
///   Additional resolution data, such as host addresses and port information, is available through
///   <see cref="IResolvableService" /> when the underlying provider supports lookup operations.
/// </remarks>
public interface IService
{
  /// <summary>
  ///   Gets the user-visible service instance name.
  /// </summary>
  string Name { get; }

  /// <summary>
  ///   Gets the Zeroconf registration type, such as <c>_http._tcp</c>.
  /// </summary>
  string RegType { get; }

  /// <summary>
  ///   Gets the reply domain for the service, typically <c>local</c>.
  /// </summary>
  string ReplyDomain { get; }

  /// <summary>
  ///   Gets or sets the TXT record associated with the service.
  /// </summary>
  /// <remarks>
  ///   TXT records may be <see langword="null" /> when no metadata is available or when the underlying provider
  ///   has not resolved the service metadata yet.
  /// </remarks>
  ITxtRecord? TxtRecord { get; set; }
}
