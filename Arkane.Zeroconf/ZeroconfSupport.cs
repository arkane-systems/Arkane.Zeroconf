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
///   <para>
///     On Linux, if Avahi is unavailable, the library falls back to <c>systemd-resolved</c> when
///     MulticastDNS is enabled in <c>/etc/systemd/resolved.conf</c>.  The systemd-resolved provider
///     supports both browsing and publishing, but publishing requires polkit authorization; if
///     authorization is denied, <see cref="CanPublish" /> will be <see langword="false" />.
///   </para>
/// </remarks>
public static class ZeroconfSupport
{
  /// <summary>
  ///   Gets the capabilities exposed by the currently selected Zeroconf provider.
  /// </summary>
  public static ZeroconfCapability Capabilities => ProviderFactory.SelectedProvider.Capabilities;

  /// <summary>
  ///   Gets whether any Zeroconf provider is available on the current system.
  /// </summary>
  /// <remarks>
  ///   Returns <see langword="false" /> when no compatible provider (Bonjour, Avahi, or Windows DNS-SD) could be
  ///   initialized. Use this to guard calls to <see cref="Capabilities" />, <see cref="CanBrowse" />, and
  ///   <see cref="CanPublish" /> when running in environments where provider availability is uncertain.
  /// </remarks>
  public static bool IsAvailable => ProviderFactory.HasAnyProvider;

  /// <summary>
  ///   Gets whether the selected provider supports service browsing and lookup.
  /// </summary>
  public static bool CanBrowse => IsAvailable && (Capabilities & ZeroconfCapability.Browse) == ZeroconfCapability.Browse;

  /// <summary>
  ///   Gets whether the selected provider supports service publishing.
  /// </summary>
  /// <remarks>
  ///   <para>
  ///     This value is <see langword="false" /> when the Windows fallback provider is active (browse-only).
  ///   </para>
  ///   <para>
  ///     On Linux with the systemd-resolved fallback provider, this value is <see langword="false" />
  ///     when the calling process is not authorized to register services via polkit
  ///     (i.e. when running without elevated privileges and polkit denies the D-Bus call).
  ///     Run the process with <c>sudo</c>, or grant the polkit rule, to obtain publish capability.
  ///   </para>
  /// </remarks>
  public static bool CanPublish => IsAvailable && (Capabilities & ZeroconfCapability.Publish) == ZeroconfCapability.Publish;
}
