#region header

// Arkane.ZeroConf - RegisterService.cs

#endregion

#region using

using System;
using System.Collections.Generic;
using System.Linq;

using Tmds.DBus.Protocol;

#endregion

namespace ArkaneSystems.Arkane.Zeroconf.Providers.SystemdResolved;

public sealed class RegisterService : IRegisterService, IDisposable
{
  private string? _registeredObjectPath;
  private bool    _disposed;

  public string Name        { get; set; } = string.Empty;
  public string RegType     { get; set; } = string.Empty;
  public string ReplyDomain { get; set; } = string.Empty;

  public ITxtRecord? TxtRecord { get; set; }

  public short Port
  {
    get => (short)this.UPort;
    set => this.UPort = (ushort)value;
  }

  public ushort UPort { get; set; }

  public event RegisterServiceEventHandler? Response;

  public void Register ()
  {
    if (this._disposed)
      throw new ObjectDisposedException (nameof (RegisterService));

    string type = this.RegType.TrimEnd ('.');
    string id   = $"arkane-zeroconf-{Guid.NewGuid ():N}";

    IEnumerable<TxtRecordItem> txtItems = this.TxtRecord != null
      ? Enumerable.Range (0, this.TxtRecord.Count).Select (i => this.TxtRecord.GetItemAt (i))
      : Enumerable.Empty<TxtRecordItem> ();

    try
    {
      this._registeredObjectPath =
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
      throw new PlatformNotSupportedException (
        "mDNS service publishing is not authorized on this system. " +
        "Check ZeroconfSupport.CanPublish before calling RegisterService.Register().", ex);
    }

    this.Response?.Invoke (o: this,
                           args: new RegisterServiceEventArgs
                                 {
                                   Service      = this,
                                   IsRegistered = true,
                                   ServiceError = ServiceErrorCode.None,
                                 });
  }

  public void Dispose ()
  {
    if (this._disposed)
      return;

    this._disposed = true;

    if (this._registeredObjectPath != null)
    {
      try
      {
        SystemdResolvedClient.UnregisterDnsServiceAsync (this._registeredObjectPath)
                             .GetAwaiter ()
                             .GetResult ();
      }
      catch (Exception ex)
      {
        System.Diagnostics.Debug.WriteLine ($"Failed to unregister service: {ex.Message}");
      }

      this._registeredObjectPath = null;
    }
  }

  private static bool IsAuthorizationError (DBusErrorReplyException ex)
    => ex.ErrorName is
       "org.freedesktop.DBus.Error.InteractiveAuthorizationRequired" or
       "org.freedesktop.DBus.Error.AccessDenied" or
       "org.freedesktop.DBus.Error.AuthFailed" or
       "org.freedesktop.DBus.Error.NotSupported";
}
