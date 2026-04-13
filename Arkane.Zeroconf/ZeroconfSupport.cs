#region header

// Arkane.ZeroConf - ZeroconfSupport.cs

#endregion

#region using

using ArkaneSystems.Arkane.Zeroconf.Providers;

#endregion

namespace ArkaneSystems.Arkane.Zeroconf;

/// <summary>
///   Provides information about the capabilities of the currently selected Zeroconf provider.
/// </summary>
/// <remarks>
///   <para>
///     Capability values reflect the provider selected by the internal provider factory at runtime.
///   </para>
///   <para>
///     On Windows, if Bonjour is unavailable but the operating system supports Windows DNS-SD lookup,
///     the fallback provider exposes browse support but not publish support.
///   </para>
/// </remarks>
public static class ZeroconfSupport
{
  /// <summary>
  ///   Gets the capabilities exposed by the currently selected Zeroconf provider.
  /// </summary>
  public static ZeroconfCapability Capabilities => ProviderFactory.SelectedProvider.Capabilities;

  /// <summary>
  ///   Gets whether the selected provider supports service browsing and lookup.
  /// </summary>
  public static bool CanBrowse => (Capabilities & ZeroconfCapability.Browse) == ZeroconfCapability.Browse;

  /// <summary>
  ///   Gets whether the selected provider supports service publishing.
  /// </summary>
  /// <remarks>
  ///   This value is <see langword="false" /> when the Windows fallback provider is active.
  /// </remarks>
  public static bool CanPublish => (Capabilities & ZeroconfCapability.Publish) == ZeroconfCapability.Publish;
}
