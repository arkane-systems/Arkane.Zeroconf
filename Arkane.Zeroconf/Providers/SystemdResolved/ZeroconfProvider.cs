#region header

// Arkane.Zeroconf - ZeroconfProvider.cs

#endregion

#region using

using System;
using System.Threading.Tasks;

using ArkaneSystems.Arkane.Zeroconf.Providers;
using ArkaneSystems.Arkane.Zeroconf.Providers.SystemdResolved;

using JetBrains.Annotations;

using Tmds.DBus.Protocol;

#endregion

[assembly: ZeroconfProvider (providerType: typeof (ZeroconfProvider), priority: 50)]

namespace ArkaneSystems.Arkane.Zeroconf.Providers.SystemdResolved;

/// <summary>
///   systemd-resolved D-Bus provider for Zeroconf browsing and service registration.
/// </summary>
/// <remarks>
///   <para>
///     This provider is intended as a Linux fallback when the Bonjour/Avahi provider is unavailable.
///     It communicates with <c>org.freedesktop.resolve1.Manager</c> via D-Bus using the system bus.
///   </para>
///   <para>
///     When the system's <c>MulticastDNS</c> mode is set to <c>yes</c>, both browsing and publishing are supported.
///     When set to <c>resolve</c>, only browsing is supported.
///   </para>
/// </remarks>
[UsedImplicitly (ImplicitUseTargetFlags.WithMembers)]
[PublicAPI]
public class ZeroconfProvider : IZeroconfProvider
{
  /// <inheritdoc />
  public Type ServiceBrowser => typeof (ServiceBrowser);

  /// <inheritdoc />
  public Type RegisterService => typeof (RegisterService);

  /// <inheritdoc />
  public Type TxtRecord => typeof (TxtRecord);

  /// <inheritdoc />
  public ZeroconfCapability Capabilities { get; private set; } = ZeroconfCapability.Browse;

  /// <summary>
  ///   Determines whether the systemd-resolved provider can be used on the current system.
  /// </summary>
  /// <returns>
  ///   <see langword="true" /> on Linux when the D-Bus system bus is reachable and the
  ///   <c>MulticastDNS</c> property is not <c>no</c>; otherwise, <see langword="false" />.
  /// </returns>
  public bool IsAvailable ()
  {
    if (!OperatingSystem.IsLinux ())
      return false;

    try
    {
      string mode = CheckMdnsModeAsync ().GetAwaiter ().GetResult ();

      return !string.Equals (a: mode, b: "no", comparisonType: StringComparison.OrdinalIgnoreCase);
    }
    catch { return false; }
  }

  /// <summary>
  ///   Initializes the systemd-resolved provider, establishing a D-Bus connection and detecting capabilities.
  /// </summary>
  public void Initialize ()
  {
    SystemdResolvedClient.InitializeAsync ().GetAwaiter ().GetResult ();

    try
    {
      string mode = ResolveManagerProxy.GetMulticastDnsModeAsync (SystemdResolvedClient.Connection)
                                       .GetAwaiter ()
                                       .GetResult ();

      this.Capabilities =
        string.Equals (a: mode, b: "yes", comparisonType: StringComparison.OrdinalIgnoreCase) && ProbePublishAuthorized ()
          ? ZeroconfCapability.Browse | ZeroconfCapability.Publish
          : ZeroconfCapability.Browse;
    }
    catch { this.Capabilities = ZeroconfCapability.Browse; }
  }

  /// <summary>
  ///   Probes whether the current process is authorized to publish services via systemd-resolved.
  ///   Sends a dummy <c>RegisterService</c> call; polkit authorization errors indicate no publish access.
  /// </summary>
  private static bool ProbePublishAuthorized ()
  {
    try
    {
      string path = ResolveManagerProxy
                   .RegisterServiceAsync (connection: SystemdResolvedClient.Connection,
                                          id: "__arkane-probe__",
                                          nameTemplate: "__probe__",
                                          type: "_probe._tcp",
                                          port: 0,
                                          priority: 0,
                                          weight: 0,
                                          txtItems: [])
                   .GetAwaiter ()
                   .GetResult ();

      // Probe succeeded — clean up and report publish is available.
      try
      {
        ResolveManagerProxy.UnregisterServiceAsync (connection: SystemdResolvedClient.Connection, objectPath: path)
                           .GetAwaiter ()
                           .GetResult ();
      }
      catch
      {
        /* best-effort cleanup */
      }

      return true;
    }
    catch (DBusErrorReplyException ex) when (IsAuthorizationError (ex)) { return false; }
    catch
    {
      // Any other error (e.g. invalid type format) means polkit allowed the call.
      return true;
    }
  }

  private static bool IsAuthorizationError (DBusErrorReplyException ex)
    => ex.ErrorName is
         "org.freedesktop.DBus.Error.InteractiveAuthorizationRequired" or
         "org.freedesktop.DBus.Error.AccessDenied" or
         "org.freedesktop.DBus.Error.AuthFailed" or
         "org.freedesktop.DBus.Error.NotSupported";

  private static async Task<string> CheckMdnsModeAsync ()
  {
    using var connection = new DBusConnection (DBusAddress.System!);
    await connection.ConnectAsync ();

    return await ResolveManagerProxy.GetMulticastDnsModeAsync (connection);
  }
}
