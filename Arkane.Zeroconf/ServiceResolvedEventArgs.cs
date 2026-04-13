#region header

// Arkane.ZeroConf - ServiceResolvedEventArgs.cs

#endregion

#region using

using System;

#endregion

namespace ArkaneSystems.Arkane.Zeroconf;

/// <summary>
///   Provides data for service resolution events.
/// </summary>
public class ServiceResolvedEventArgs : EventArgs
{
  /// <summary>
  ///   Initializes a new instance of the <see cref="ServiceResolvedEventArgs" /> class.
  /// </summary>
  /// <param name="service">The resolved service.</param>
  public ServiceResolvedEventArgs (IResolvableService service) => this.Service = service;

  /// <summary>
  ///   Gets the resolved service.
  /// </summary>
  public IResolvableService Service { get; }
}
