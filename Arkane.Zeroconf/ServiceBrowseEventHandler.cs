#region header

// Arkane.ZeroConf - ServiceBrowseEventHandler.cs

#endregion

namespace ArkaneSystems.Arkane.Zeroconf;

/// <summary>
///   Represents the method that handles service browse events.
/// </summary>
/// <param name="o">The event source.</param>
/// <param name="args">The event data describing the discovered or removed service.</param>
public delegate void ServiceBrowseEventHandler (object? o, ServiceBrowseEventArgs args);
