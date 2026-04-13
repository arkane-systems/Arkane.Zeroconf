#region header

// Arkane.ZeroConf - ServiceClass.cs

#endregion

/// <summary>
/// Bonjour DNS record class constants used by the provider interop layer.
/// </summary>
/// <remarks>
/// These values mirror the native Bonjour DNS-SD constants used when issuing record queries.
/// The enum is kept provider-internal because it supports Bonjour interop implementation details,
/// not the public Arkane.Zeroconf API surface.
/// </remarks>
// ReSharper disable InconsistentNaming
namespace ArkaneSystems.Arkane.Zeroconf.Providers.Bonjour;

internal enum ServiceClass : ushort
{
  IN = 1, /* Internet */
}
