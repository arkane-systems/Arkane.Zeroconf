#region header

// Arkane.Zeroconf - RegisterService.cs

#endregion

#region using

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using Tmds.DBus.Protocol;

#endregion

namespace ArkaneSystems.Arkane.Zeroconf.Providers.SystemdResolved;

/// <summary>
///   Publishes a service via <c>org.freedesktop.resolve1.Manager.RegisterService</c> on the system D-Bus.
/// </summary>
/// <remarks>
///   <para>
///     Publishing requires polkit authorization.  On systems where the calling process does not hold the
///     necessary privilege, <see cref="Register" /> throws <see cref="PlatformNotSupportedException" />.
///     Check <see cref="ZeroconfSupport.CanPublish" /> before calling <see cref="Register" />.
///   </para>
///   <para>
///     To obtain publish capability on a system where polkit blocks unprivileged access, run with
///     <c>sudo</c> or configure a polkit rule for <c>org.freedesktop.resolve1</c>.
///   </para>
///   <para>
///     Dispose this instance to unregister the service.  Disposal is idempotent.
///   </para>
/// </remarks>
public sealed class RegisterService : IRegisterService, IDisposable
{
  private bool    disposed;
  private string? registeredObjectPath;

  public string Name { get; set; } = string.Empty;

  public string RegType { get; set; } = string.Empty;

  public string ReplyDomain { get; set; } = string.Empty;

  public ITxtRecord? TxtRecord { get; set; }

  public short Port { get => (short)this.UPort; set => this.UPort = (ushort)value; }

  public ushort UPort { get; set; }

  public event RegisterServiceEventHandler? Response;

  /// <summary>
  ///   Registers this service with systemd-resolved via D-Bus.
  /// </summary>
  /// <exception cref="PlatformNotSupportedException">
  ///   The calling process is not authorized to register services (polkit denied).
  ///   Check <see cref="ZeroconfSupport.CanPublish" /> before calling this method.
  /// </exception>
  /// <exception cref="ObjectDisposedException">This instance has already been disposed.</exception>
  public void Register ()
  {
    ObjectDisposedException.ThrowIf (condition: this.disposed, instance: this);

    string type = this.RegType.TrimEnd ('.');
    var    id   = $"arkane-zeroconf-{Guid.NewGuid ():N}";

    IEnumerable<TxtRecordItem> txtItems = this.TxtRecord != null
                                            ? Enumerable.Range (start: 0, count: this.TxtRecord.Count)
                                                        .Select (i => this.TxtRecord.GetItemAt (i))
                                            : [];

    try
    {
      this.registeredObjectPath =
        SystemdResolvedClient.RegisterDnsServiceAsync (id: id,
                                                       nameTemplate: this.Name,
                                                       type: type,
                                                       port: this.UPort,
                                                       txtItems: txtItems)
                             .GetAwaiter ()
                             .GetResult ();
    }
    catch (DBusErrorReplyException ex) when (IsAuthorizationError (ex))
    {
      throw new PlatformNotSupportedException (message: "mDNS service publishing is not authorized on this system. " +
                                                        "Check ZeroconfSupport.CanPublish before calling RegisterService.Register().",
                                               inner: ex);
    }

    this.Response?.Invoke (o: this,
                           args: new RegisterServiceEventArgs
                                 {
                                   Service = this, IsRegistered = true, ServiceError = ServiceErrorCode.None,
                                 });
  }

  public void Dispose ()
  {
    if (this.disposed)
      return;

    this.disposed = true;

    if (this.registeredObjectPath != null)
    {
      try
      {
        SystemdResolvedClient.UnregisterDnsServiceAsync (this.registeredObjectPath)
                             .GetAwaiter ()
                             .GetResult ();
      }
      catch (Exception ex) { Debug.WriteLine ($"Failed to unregister service: {ex.Message}"); }

      this.registeredObjectPath = null;
    }
  }

  private static bool IsAuthorizationError (DBusErrorReplyException ex)
    => ex.ErrorName is
         "org.freedesktop.DBus.Error.InteractiveAuthorizationRequired" or
         "org.freedesktop.DBus.Error.AccessDenied" or
         "org.freedesktop.DBus.Error.AuthFailed" or
         "org.freedesktop.DBus.Error.NotSupported";
}
