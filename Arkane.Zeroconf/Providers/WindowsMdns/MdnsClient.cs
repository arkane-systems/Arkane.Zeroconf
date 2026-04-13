#region header

// Arkane.ZeroConf - MdnsClient.cs

#endregion

#region using

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;

#endregion

namespace ArkaneSystems.Arkane.Zeroconf.Providers.WindowsMdns;

internal static class MdnsClient
{
  #region Nested type: BrowseState

  private sealed class BrowseState
  {
    public object SyncRoot { get; } = new ();

    public List<DnsRecord> Records { get; } = new ();

    public ManualResetEventSlim WaitHandle { get; } = new (false);
  }

  #endregion

  #region Nested type: DnsRecord

  private sealed class DnsRecord
  {
    public string Name { get; init; } = string.Empty;

    public DnsRecordType Type { get; init; }

    public string PtrTarget { get; set; } = string.Empty;
  }

  #endregion

  #region Nested type: DnsRecordType

  private enum DnsRecordType : ushort
  {
    Ptr = 12,
  }

  #endregion

  #region Nested type: MdnsResolveResult

  internal sealed class MdnsResolveResult
  {
    public static readonly MdnsResolveResult Empty = new ()
                                                     {
                                                       HostName   = string.Empty,
                                                       Addresses  = Array.Empty<IPAddress> (),
                                                       TxtEntries = Array.Empty<TxtRecordItem> ()
                                                     };

    public string HostName { get; init; } = string.Empty;

    public ushort Port { get; init; }

    public IPAddress[] Addresses { get; init; } = Array.Empty<IPAddress> ();

    public TxtRecordItem[] TxtEntries { get; init; } = Array.Empty<TxtRecordItem> ();

    public bool IsResolved => this.Addresses.Length > 0;
  }

  #endregion

  #region Nested type: MdnsServiceInfo

  internal sealed class MdnsServiceInfo
  {
    public string Name { get; init; } = string.Empty;

    public string FullName { get; init; } = string.Empty;

    public string RegType { get; init; } = string.Empty;

    public string ReplyDomain { get; init; } = string.Empty;

    public uint InterfaceIndex { get; init; }
  }

  #endregion

  #region Nested type: ResolvedInstance

  private sealed class ResolvedInstance
  {
    public string HostName { get; init; } = string.Empty;

    public ushort Port { get; init; }

    public List<IPAddress> Addresses { get; init; } = new ();

    public List<TxtRecordItem> TxtItems { get; init; } = new ();
  }

  #endregion

  #region Nested type: ResolveState

  private sealed class ResolveState
  {
    public ManualResetEventSlim WaitHandle { get; } = new (false);

    public ResolvedInstance? Result { get; set; }
  }

  #endregion

  private static readonly TimeSpan DefaultBrowseTimeout  = TimeSpan.FromSeconds (4);
  private static readonly TimeSpan DefaultResolveTimeout = TimeSpan.FromSeconds (4);

  private static TimeSpan browseTimeout  = DefaultBrowseTimeout;
  private static TimeSpan resolveTimeout = DefaultResolveTimeout;
  private static readonly object timeoutLock = new ();

  /// <summary>
  /// Gets or sets the timeout for mDNS browse operations.
  /// Default is 4 seconds.
  /// </summary>
  /// <remarks>
  /// <para>
  /// This property is thread-safe and can be configured before using the Zeroconf provider
  /// to accommodate slow networks or high-latency environments.
  /// For example:
  /// <code>
  /// MdnsClient.BrowseTimeout = TimeSpan.FromSeconds(10);
  /// </code>
  /// </para>
  /// </remarks>
  public static TimeSpan BrowseTimeout
  {
    get
    {
      lock (timeoutLock)
        return browseTimeout;
    }
    set
    {
      lock (timeoutLock)
        browseTimeout = value;
    }
  }

  /// <summary>
  /// Gets or sets the timeout for mDNS resolve operations.
  /// Default is 4 seconds.
  /// </summary>
  /// <remarks>
  /// <para>
  /// This property is thread-safe and can be configured before using the Zeroconf provider
  /// to accommodate slow networks or high-latency environments.
  /// For example:
  /// <code>
  /// MdnsClient.ResolveTimeout = TimeSpan.FromSeconds(10);
  /// </code>
  /// </para>
  /// </remarks>
  public static TimeSpan ResolveTimeout
  {
    get
    {
      lock (timeoutLock)
        return resolveTimeout;
    }
    set
    {
      lock (timeoutLock)
        resolveTimeout = value;
    }
  }

  private static readonly NativeWindowsDns.DnsServiceBrowseCallback  BrowseCallback  = OnBrowseResult;
  private static readonly NativeWindowsDns.DnsServiceResolveComplete ResolveCallback = OnResolveResult;

  public static IReadOnlyList<MdnsServiceInfo> Browse (string            regtype,
                                                       string            domain,
                                                       AddressProtocol   addressProtocol,
                                                       CancellationToken cancellationToken)
  {
    ArgumentNullException.ThrowIfNull (regtype);
    ArgumentNullException.ThrowIfNull (domain);

    cancellationToken.ThrowIfCancellationRequested ();
    _ = addressProtocol;

    string queryName = NormalizeName ($"{regtype}.{domain}").TrimEnd ('.');
    IReadOnlyList<DnsRecord> records = BrowseRecords (queryName: queryName,
                                                      interfaceIndex: 0,
                                                      cancellationToken: cancellationToken);

    var services = new List<MdnsServiceInfo> ();

    foreach (DnsRecord record in records.Where (record => (record.Type == DnsRecordType.Ptr) &&
                                                          string.Equals (a: NormalizeName (record.Name),
                                                                         b: NormalizeName (queryName),
                                                                         comparisonType: StringComparison.OrdinalIgnoreCase) &&
                                                          !string.IsNullOrWhiteSpace (record.PtrTarget)))
    {
      string fullName    = NormalizeName (record.PtrTarget);
      string serviceName = ParseServiceName (fullName: fullName, regtype: regtype);

      services.Add (new MdnsServiceInfo
                    {
                      Name           = serviceName,
                      FullName       = fullName,
                      RegType        = regtype,
                      ReplyDomain    = domain,
                      InterfaceIndex = 0,
                    });
    }

    return services.GroupBy (keySelector: service => service.FullName, comparer: StringComparer.OrdinalIgnoreCase)
                   .Select (group => group.First ())
                   .ToArray ();
  }

  public static MdnsResolveResult Resolve (MdnsServiceInfo   service,
                                           AddressProtocol   addressProtocol,
                                           CancellationToken cancellationToken)
  {
    ArgumentNullException.ThrowIfNull (service);

    cancellationToken.ThrowIfCancellationRequested ();

    ResolvedInstance? resolved = ResolveService (queryName: service.FullName.TrimEnd ('.'),
                                                 interfaceIndex: service.InterfaceIndex,
                                                 cancellationToken: cancellationToken);

    if (resolved == null)
      return MdnsResolveResult.Empty;

    IPAddress[] addresses = resolved.Addresses.Where (address => address != null)
                                    .Distinct ()
                                    .ToArray ();

    if (addresses.Length == 0)
      return MdnsResolveResult.Empty;

    return new MdnsResolveResult
           {
             HostName   = NormalizeName (resolved.HostName),
             Port       = resolved.Port,
             Addresses  = addresses,
             TxtEntries = resolved.TxtItems.ToArray (),
           };
  }

  private static IReadOnlyList<DnsRecord> BrowseRecords (string queryName, uint interfaceIndex, CancellationToken cancellationToken)
  {
    var      state  = new BrowseState ();
    GCHandle handle = GCHandle.Alloc (state);

    var cancel = new NativeWindowsDns.DnsServiceCancel ();

    try
    {
      var request = new NativeWindowsDns.DnsServiceBrowseRequest
                    {
                      Version        = NativeWindowsDns.DnsQueryRequestVersion1,
                      InterfaceIndex = interfaceIndex,
                      QueryName      = queryName,
                      BrowseCallback = BrowseCallback,
                      QueryContext   = GCHandle.ToIntPtr (handle),
                    };

      int status = NativeWindowsDns.DnsServiceBrowse (request: ref request, cancel: ref cancel);

      if ((status != 0) && (status != NativeWindowsDns.DnsRequestPending))
        return Array.Empty<DnsRecord> ();

      state.WaitHandle.Wait (timeout: BrowseTimeout, cancellationToken: cancellationToken);

      lock (state.SyncRoot)
        return state.Records.ToArray ();
    }
    finally
    {
      if (cancel.Reserved != IntPtr.Zero)
        _ = NativeWindowsDns.DnsServiceBrowseCancel (cancel: ref cancel);

      state.WaitHandle.Dispose ();

      if (handle.IsAllocated)
        handle.Free ();
    }
  }

  private static ResolvedInstance? ResolveService (string queryName, uint interfaceIndex, CancellationToken cancellationToken)
  {
    var      state  = new ResolveState ();
    GCHandle handle = GCHandle.Alloc (state);

    var cancel = new NativeWindowsDns.DnsServiceCancel ();

    try
    {
      var request = new NativeWindowsDns.DnsServiceResolveRequest
                    {
                      Version         = NativeWindowsDns.DnsQueryRequestVersion1,
                      InterfaceIndex  = interfaceIndex,
                      QueryName       = queryName,
                      ResolveCallback = ResolveCallback,
                      QueryContext    = GCHandle.ToIntPtr (handle),
                    };

      int status = NativeWindowsDns.DnsServiceResolve (request: ref request, cancel: ref cancel);

      if ((status != 0) && (status != NativeWindowsDns.DnsRequestPending))
        return null;

      state.WaitHandle.Wait (timeout: ResolveTimeout, cancellationToken: cancellationToken);

      return state.Result;
    }
    finally
    {
      if (cancel.Reserved != IntPtr.Zero)
        _ = NativeWindowsDns.DnsServiceResolveCancel (cancel: ref cancel);

      state.WaitHandle.Dispose ();

      if (handle.IsAllocated)
        handle.Free ();
    }
  }

  private static void OnBrowseResult (uint status, IntPtr queryContext, IntPtr dnsRecord)
  {
    try
    {
      if (!TryGetHandleTarget<BrowseState> (queryContext, out BrowseState? state))
        return;

      if ((status != 0) || (dnsRecord == IntPtr.Zero))
        return;

      IReadOnlyList<DnsRecord> records = ProjectBrowseRecords (dnsRecord);

      lock (state.SyncRoot)
        state.Records.AddRange (records);

      if (records.Count > 0)
        state.WaitHandle.Set ();
    }
    catch (Exception ex)
    {
      System.Diagnostics.Debug.WriteLine ($"Error in OnBrowseResult callback: {ex.Message}");
    }
    finally
    {
      if (dnsRecord != IntPtr.Zero)
        NativeWindowsDns.DnsRecordListFree (pRecordList: dnsRecord, freeType: NativeWindowsDns.DnsFreeType.RecordList);
    }
  }

  private static void OnResolveResult (uint status, IntPtr queryContext, IntPtr instance)
  {
    try
    {
      if (!TryGetHandleTarget<ResolveState> (queryContext, out ResolveState? state))
        return;

      if ((status != 0) || (instance == IntPtr.Zero))
        return;

      state.Result = ProjectResolvedInstance (instance);
      state.WaitHandle.Set ();
    }
    catch (Exception ex)
    {
      System.Diagnostics.Debug.WriteLine ($"Error in OnResolveResult callback: {ex.Message}");
    }
    finally
    {
      if (instance != IntPtr.Zero)
        NativeWindowsDns.DnsServiceFreeInstance (instance);
    }
  }

  private static bool TryGetHandleTarget<T> (IntPtr queryContext, out T? target) where T : class
  {
    target = null;

    try
    {
      GCHandle handle = GCHandle.FromIntPtr (queryContext);

      if (!handle.IsAllocated)
        return false;

      if (handle.Target is T typedTarget)
      {
        target = typedTarget;
        return true;
      }
    }
    catch (Exception ex)
    {
      System.Diagnostics.Debug.WriteLine ($"Error retrieving GCHandle target: {ex.Message}");
    }

    return false;
  }

  private static IReadOnlyList<DnsRecord> ProjectBrowseRecords (IntPtr recordList)
  {
    var records = new List<DnsRecord> ();

    IntPtr current = recordList;
    var    guard   = 0;

    while ((current != IntPtr.Zero) && (guard++ < 4096))
    {
      var header = Marshal.PtrToStructure<NativeWindowsDns.DnsRecordHeader> (current);

      if ((DnsRecordType)header.wType == DnsRecordType.Ptr)
      {
        IntPtr dataPtr = IntPtr.Add (pointer: current, offset: Marshal.SizeOf<NativeWindowsDns.DnsRecordHeader> ());

        try
        {
          string name   = header.pName != IntPtr.Zero ? Marshal.PtrToStringUni (header.pName) ?? string.Empty : string.Empty;
          string target = Marshal.ReadIntPtr (dataPtr) is { } targetPtr && targetPtr != IntPtr.Zero
            ? Marshal.PtrToStringUni (targetPtr) ?? string.Empty
            : string.Empty;

          records.Add (new DnsRecord
                       {
                         Name      = name,
                         Type      = DnsRecordType.Ptr,
                         PtrTarget = target,
                       });
        }
        catch (Exception ex)
        {
          System.Diagnostics.Debug.WriteLine ($"Failed to parse browse record: {ex.Message}");
        }
      }

      current = header.pNext;
    }

    return records;
  }

  private static ResolvedInstance ProjectResolvedInstance (IntPtr instancePtr)
  {
    var instance = Marshal.PtrToStructure<NativeWindowsDns.DnsServiceInstance> (instancePtr);

    var addresses = new List<IPAddress> ();

    if (instance.Ip4Address != IntPtr.Zero)
    {
      try
      {
        var    ipv4Raw = (uint)Marshal.ReadInt32 (instance.Ip4Address);
        byte[] bytes   = BitConverter.GetBytes (ipv4Raw);

        if (BitConverter.IsLittleEndian)
          Array.Reverse (bytes);

        addresses.Add (new IPAddress (bytes));
      }
      catch (Exception ex)
      {
        // Log warning but continue - invalid IPv4 record, skip it
        System.Diagnostics.Debug.WriteLine ($"Failed to parse IPv4 address: {ex.Message}");
      }
    }

    if (instance.Ip6Address != IntPtr.Zero)
    {
      try
      {
        var bytes = new byte[16];
        Marshal.Copy (source: instance.Ip6Address, destination: bytes, startIndex: 0, length: bytes.Length);
        addresses.Add (new IPAddress (bytes));
      }
      catch (Exception ex)
      {
        // Log warning but continue - invalid IPv6 record, skip it
        System.Diagnostics.Debug.WriteLine ($"Failed to parse IPv6 address: {ex.Message}");
      }
    }

    var txtItems = new List<TxtRecordItem> ();

    if ((instance.PropertyCount > 0) && (instance.Keys != IntPtr.Zero) && (instance.Values != IntPtr.Zero))
      for (var i = 0; i < instance.PropertyCount; i++)
      {
        try
        {
          IntPtr keyPtr   = Marshal.ReadIntPtr (ptr: instance.Keys,   ofs: i * IntPtr.Size);
          IntPtr valuePtr = Marshal.ReadIntPtr (ptr: instance.Values, ofs: i * IntPtr.Size);

          if (keyPtr == IntPtr.Zero)
            continue;

          string? key   = Marshal.PtrToStringUni (keyPtr);
          string? value = valuePtr != IntPtr.Zero ? Marshal.PtrToStringUni (valuePtr) : null;

          if (string.IsNullOrWhiteSpace (key))
            continue;

          txtItems.Add (new TxtRecordItem (key: key, valueString: value ?? string.Empty));
        }
        catch (Exception ex)
        {
          // Log warning but continue - invalid TXT record, skip it
          System.Diagnostics.Debug.WriteLine ($"Failed to parse TXT record at index {i}: {ex.Message}");
        }
      }

    return new ResolvedInstance
           {
             HostName  = Marshal.PtrToStringUni (instance.HostName) ?? string.Empty,
             Port      = instance.Port,
             Addresses = addresses,
             TxtItems  = txtItems,
           };
  }

  private static string NormalizeName (string value)
  {
    if (string.IsNullOrWhiteSpace (value))
      return string.Empty;

    return value.Trim ().TrimEnd ('.') + ".";
  }

  private static string ParseServiceName (string fullName, string regtype)
  {
    if (string.IsNullOrWhiteSpace (fullName))
      return string.Empty;

    string normalizedRegType = NormalizeName (regtype).TrimEnd ('.');
    var    marker            = $".{normalizedRegType}.";
    int    markerIndex       = fullName.IndexOf (value: marker, comparisonType: StringComparison.OrdinalIgnoreCase);

    if (markerIndex > 0)
      return fullName[..markerIndex];

    string[] labels = fullName.Split (separator: '.', options: StringSplitOptions.RemoveEmptyEntries);

    return labels.Length > 0 ? labels[0] : fullName;
  }
}
