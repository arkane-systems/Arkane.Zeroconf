#region header

// Arkane.ZeroConf - SystemdResolvedClient.cs

#endregion

#region using

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Tmds.DBus.Protocol;

#endregion

namespace ArkaneSystems.Arkane.Zeroconf.Providers.SystemdResolved;

internal static class SystemdResolvedClient
{
  private static DBusConnection? _connection;

  private static readonly object   _browseTimeoutLock  = new ();
  private static          TimeSpan _browseTimeout       = TimeSpan.FromSeconds (4);
  private static readonly object   _resolveTimeoutLock = new ();
  private static          TimeSpan _resolveTimeout      = TimeSpan.FromSeconds (4);

  /// <summary>
  /// Gets or sets the timeout for mDNS browse operations via systemd-resolved.
  /// Default is 4 seconds.
  /// </summary>
  public static TimeSpan BrowseTimeout
  {
    get
    {
      lock (_browseTimeoutLock)
        return _browseTimeout;
    }
    set
    {
      lock (_browseTimeoutLock)
        _browseTimeout = value;
    }
  }

  /// <summary>
  /// Gets or sets the timeout for mDNS resolve operations via systemd-resolved.
  /// Default is 4 seconds.
  /// </summary>
  public static TimeSpan ResolveTimeout
  {
    get
    {
      lock (_resolveTimeoutLock)
        return _resolveTimeout;
    }
    set
    {
      lock (_resolveTimeoutLock)
        _resolveTimeout = value;
    }
  }

  internal static DBusConnection Connection
    => _connection ?? throw new InvalidOperationException ("SystemdResolved client not initialized.");

  internal static async Task InitializeAsync ()
  {
    _connection = new DBusConnection (DBusAddress.System!);
    await _connection.ConnectAsync ();
  }

  internal static void Dispose ()
  {
    _connection?.Dispose ();
    _connection = null;
  }

  #region Nested type: ServiceInstance

  internal sealed class ServiceInstance
  {
    public ServiceInstance (string name, string fullName, string regType, string domain, uint interfaceIndex)
    {
      this.Name           = name;
      this.FullName       = fullName;
      this.RegType        = regType;
      this.Domain         = domain;
      this.InterfaceIndex = interfaceIndex;
    }

    public string Name           { get; }
    public string FullName       { get; }
    public string RegType        { get; }
    public string Domain         { get; }
    public uint   InterfaceIndex { get; }
  }

  #endregion

  /// <summary>
  ///   Browses for PTR records for the given service type and domain, returning discovered service instances.
  /// </summary>
  internal static async Task<IReadOnlyList<ServiceInstance>> BrowseInstancesAsync (
    string            regtype,
    string            domain,
    CancellationToken cancellationToken = default)
  {
    string queryName = $"{regtype}.{domain}";

    const ushort classIn  = 1;
    const ushort typePTR  = 12;
    const ulong  dnsFlags = 0;

    using var cts = CancellationTokenSource.CreateLinkedTokenSource (cancellationToken);
    cts.CancelAfter (BrowseTimeout);

    IReadOnlyList<byte[]> rawRecords =
      await ResolveManagerProxy.ResolveRecordAsync (connection: Connection,
                                                    ifindex: 0,
                                                    name: queryName,
                                                    rrClass: classIn,
                                                    rrType: typePTR,
                                                    flags: dnsFlags,
                                                    ct: cts.Token);

    var     results = new List<ServiceInstance> ();
    string  suffix  = $".{regtype}.{domain}";

    foreach (byte[] rawRr in rawRecords)
    {
      string? fqdn = ResolveManagerProxy.ExtractPtrTarget (rawRr);

      if (string.IsNullOrEmpty (fqdn))
        continue;

      string instanceName;

      if (fqdn.EndsWith (value: suffix, comparisonType: StringComparison.OrdinalIgnoreCase))
        instanceName = fqdn[..^suffix.Length];
      else
        instanceName = fqdn.Split ('.')[0];

      if (string.IsNullOrEmpty (instanceName))
        continue;

      results.Add (new ServiceInstance (name: instanceName,
                                        fullName: fqdn,
                                        regType: regtype,
                                        domain: domain,
                                        interfaceIndex: 0));
    }

    return results;
  }

  /// <summary>
  ///   Resolves a service instance by name, type, and domain.
  /// </summary>
  internal static async Task<ResolveManagerProxy.ServiceResolveResult?> ResolveInstanceAsync (
    string            name,
    string            regtype,
    string            domain,
    int               family             = 0,
    CancellationToken cancellationToken  = default)
  {
    using var cts = CancellationTokenSource.CreateLinkedTokenSource (cancellationToken);
    cts.CancelAfter (ResolveTimeout);

    return await ResolveManagerProxy.ResolveServiceAsync (connection: Connection,
                                                          ifindex: 0,
                                                          name: name,
                                                          type: regtype,
                                                          domain: domain,
                                                          family: family,
                                                          flags: 0,
                                                          ct: cts.Token);
  }

  /// <summary>
  ///   Registers a DNS service via systemd-resolved and returns the D-Bus object path handle.
  /// </summary>
  internal static async Task<string> RegisterDnsServiceAsync (
    string                    id,
    string                    nameTemplate,
    string                    type,
    ushort                    port,
    IEnumerable<TxtRecordItem> txtItems,
    CancellationToken         cancellationToken = default)
  {
    return await ResolveManagerProxy.RegisterServiceAsync (connection: Connection,
                                                           id: id,
                                                           nameTemplate: nameTemplate,
                                                           type: type,
                                                           port: port,
                                                           priority: 0,
                                                           weight: 0,
                                                           txtItems: txtItems,
                                                           ct: cancellationToken);
  }

  /// <summary>
  ///   Unregisters a previously registered DNS service.
  /// </summary>
  internal static async Task UnregisterDnsServiceAsync (
    string            objectPath,
    CancellationToken cancellationToken = default)
  {
    await ResolveManagerProxy.UnregisterServiceAsync (connection: Connection,
                                                      objectPath: objectPath,
                                                      ct: cancellationToken);
  }
}
