#region header

// Arkane.Zeroconf - ServiceResolvedEventArgs.cs

#endregion

#region using

using System;

using JetBrains.Annotations;

#endregion

namespace ArkaneSystems.Arkane.Zeroconf;

/// <summary>
///   Provides data for service resolution events.
/// </summary>
/// <remarks>
///   Initializes a new instance of the <see cref="ServiceResolvedEventArgs" /> class.
/// </remarks>
/// <param name="service">The resolved service.</param>
[PublicAPI]
public class ServiceResolvedEventArgs (IResolvableService service) : EventArgs
{
  /// <summary>
  ///   Gets the resolved service.
  /// </summary>
  public IResolvableService Service { get; } = service;
}
