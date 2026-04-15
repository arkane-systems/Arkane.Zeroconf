#region header

// Arkane.Zeroconf - ServiceBrowseEventArgs.cs

#endregion

#region using

using System;

using JetBrains.Annotations;

#endregion

namespace ArkaneSystems.Arkane.Zeroconf;

/// <summary>
///   Provides data for service browse events.
/// </summary>
/// <remarks>
///   Initializes a new instance of the <see cref="ServiceBrowseEventArgs" /> class.
/// </remarks>
/// <param name="service">The service associated with the browse event.</param>
[PublicAPI]
public class ServiceBrowseEventArgs (IResolvableService service) : EventArgs
{
  /// <summary>
  ///   Gets the service associated with the event.
  /// </summary>
  public IResolvableService Service { get; } = service;
}
