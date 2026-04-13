#region header

// Arkane.ZeroConf - ServiceBrowseEventArgs.cs

#endregion

#region using

using System;

#endregion

namespace ArkaneSystems.Arkane.Zeroconf;

/// <summary>
///   Provides data for service browse events.
/// </summary>
public class ServiceBrowseEventArgs : EventArgs
{
  /// <summary>
  ///   Initializes a new instance of the <see cref="ServiceBrowseEventArgs" /> class.
  /// </summary>
  /// <param name="service">The service associated with the browse event.</param>
  public ServiceBrowseEventArgs (IResolvableService service) => this.Service = service;

  /// <summary>
  ///   Gets the service associated with the event.
  /// </summary>
  public IResolvableService Service { get; }
}
