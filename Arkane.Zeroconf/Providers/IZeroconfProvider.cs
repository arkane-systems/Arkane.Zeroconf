#region header

// Arkane.ZeroConf - IZeroconfProvider.cs

#endregion

#region using

using System;

#endregion

namespace ArkaneSystems.Arkane.Zeroconf.Providers;

/// <summary>
///   Defines a platform-specific Zeroconf provider.
/// </summary>
/// <remarks>
///   Providers are discovered through <c>ZeroconfProviderAttribute</c> and selected by the internal provider factory.
///   Bonjour is preferred when available. On Windows 11 and later, the library may fall back to the Windows lookup-only
///   provider when Bonjour is unavailable.
/// </remarks>
public interface IZeroconfProvider
{
  /// <summary>
  ///   Gets the concrete service browser type exposed by the provider.
  /// </summary>
  Type ServiceBrowser { get; }

  /// <summary>
  ///   Gets the concrete register-service type exposed by the provider.
  /// </summary>
  Type RegisterService { get; }

  /// <summary>
  ///   Gets the concrete TXT record type exposed by the provider.
  /// </summary>
  Type TxtRecord { get; }

  /// <summary>
  ///   Gets the capabilities supported by the provider.
  /// </summary>
  /// <remarks>
  ///   The default implementation returns both <see cref="ZeroconfCapability.Browse" /> and
  ///   <see cref="ZeroconfCapability.Publish" />. Providers that support only a subset, such as the
  ///   Windows fallback provider, should override this property.
  /// </remarks>
  ZeroconfCapability Capabilities => ZeroconfCapability.Browse | ZeroconfCapability.Publish;

  /// <summary>
  ///   Determines whether the provider is available on the current platform and runtime environment.
  /// </summary>
  /// <returns><see langword="true" /> when the provider can be used; otherwise, <see langword="false" />.</returns>
  /// <remarks>
  ///   The default implementation returns <see langword="true" />. Providers should override this method when availability
  ///   depends on operating system support, native libraries, or runtime initialization checks.
  /// </remarks>
  bool IsAvailable () => true;

  /// <summary>
  ///   Initializes the provider before use.
  /// </summary>
  /// <remarks>
  ///   Providers can use this method to perform runtime checks, allocate shared native state, or configure
  ///   platform-specific prerequisites.
  /// </remarks>
  void Initialize ();
}
