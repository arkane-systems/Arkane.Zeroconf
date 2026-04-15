#region header

// Arkane.ZeroConf - ServiceResolvedEventHandler.cs

#endregion

#region using

using JetBrains.Annotations;

#endregion

namespace ArkaneSystems.Arkane.Zeroconf;

/// <summary>
/// Represents the method that handles service resolution events.
/// </summary>
/// <param name="o">The event source.</param>
/// <param name="args">The event data describing the resolved service.</param>
[PublicAPI]
public delegate void ServiceResolvedEventHandler (object? o, ServiceResolvedEventArgs args);
