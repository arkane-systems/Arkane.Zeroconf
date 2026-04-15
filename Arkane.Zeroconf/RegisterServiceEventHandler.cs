#region header

// Arkane.ZeroConf - RegisterServiceEventHandler.cs

#endregion

#region using

using JetBrains.Annotations;

#endregion

namespace ArkaneSystems.Arkane.Zeroconf;

/// <summary>
/// Represents the method that handles service registration response events.
/// </summary>
/// <param name="o">The event source.</param>
/// <param name="args">The event data describing the registration result.</param>
[PublicAPI]
public delegate void RegisterServiceEventHandler (object? o, RegisterServiceEventArgs args);
