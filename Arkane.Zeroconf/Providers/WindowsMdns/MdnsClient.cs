#region header

// Arkane.ZeroConf - MdnsClient.cs

#endregion

#region using

using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

#endregion

namespace ArkaneSystems.Arkane.Zeroconf.Providers.WindowsMdns;

internal static class MdnsClient
{
  #region Nested type: DnsRecord

  private sealed class DnsRecord
  {
    public string Name { get; init; }

    public DnsRecordType Type { get; init; }

    public string PtrTarget { get; set; }

    public string TargetHost { get; set; }

    public ushort Port { get; set; }

    public IPAddress Address { get; set; }

    public IEnumerable<TxtRecordItem> TxtItems { get; set; } = Array.Empty<TxtRecordItem> ();
  }

  #endregion

  #region Nested type: DnsRecordType

  private enum DnsRecordType : ushort
  {
    A    = 1,
    Ptr  = 12,
    Txt  = 16,
    Aaaa = 28,
    Srv  = 33,
    Any  = 255,
  }

  #endregion

  #region Nested type: MdnsResolveResult

  internal sealed class MdnsResolveResult
  {
    public static readonly MdnsResolveResult Empty = new ()
                                                     {
                                                       Addresses  = Array.Empty<IPAddress> (),
                                                       TxtEntries = Array.Empty<TxtRecordItem> ()
                                                     };

    public string HostName { get; init; }

    public ushort Port { get; init; }

    public IPAddress[] Addresses { get; init; }

    public TxtRecordItem[] TxtEntries { get; init; }

    public bool IsResolved => this.Addresses?.Length > 0;
  }

  #endregion

  #region Nested type: MdnsServiceInfo

  internal sealed class MdnsServiceInfo
  {
    public string Name { get; init; }

    public string FullName { get; init; }

    public string RegType { get; init; }

    public string ReplyDomain { get; init; }

    public uint InterfaceIndex { get; init; }
  }

  #endregion

  private const int DnsPort = 5353;

  private static readonly TimeSpan QueryWindow = TimeSpan.FromMilliseconds (1200);

  public static IReadOnlyList<MdnsServiceInfo> Browse (string            regtype,
                                                       string            domain,
                                                       AddressProtocol   addressProtocol,
                                                       CancellationToken cancellationToken)
  {
    ArgumentNullException.ThrowIfNull (regtype);
    ArgumentNullException.ThrowIfNull (domain);

    string queryName = MdnsClient.NormalizeName ($"{regtype}.{domain}");
    IReadOnlyList<DnsRecord> records = MdnsClient.Query (name: queryName,
                                                         type: DnsRecordType.Ptr,
                                                         addressProtocol: addressProtocol,
                                                         cancellationToken: cancellationToken);

    var services = new List<MdnsServiceInfo> ();

    foreach (DnsRecord record in records.Where (record => (record.Type == DnsRecordType.Ptr) &&
                                                          string.Equals (a: MdnsClient.NormalizeName (record.Name),
                                                                         b: queryName,
                                                                         comparisonType: StringComparison.OrdinalIgnoreCase) &&
                                                          !string.IsNullOrWhiteSpace (record.PtrTarget)))
    {
      string fullName    = MdnsClient.NormalizeName (record.PtrTarget);
      string serviceName = MdnsClient.ParseServiceName (fullName: fullName, regtype: regtype);

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

    IReadOnlyList<DnsRecord> serviceRecords = MdnsClient.Query (name: service.FullName,
                                                                type: DnsRecordType.Any,
                                                                addressProtocol: addressProtocol,
                                                                cancellationToken: cancellationToken);

    DnsRecord srvRecord = serviceRecords.FirstOrDefault (record => record.Type == DnsRecordType.Srv);

    if (srvRecord == null)
      return MdnsResolveResult.Empty;

    TxtRecordItem[] txtEntries = serviceRecords.Where (record => record.Type == DnsRecordType.Txt)
                                               .SelectMany (record => record.TxtItems)
                                               .Where (item => item != null)
                                               .ToArray ();

    string hostName = MdnsClient.NormalizeName (srvRecord.TargetHost);
    IReadOnlyList<DnsRecord> hostRecords = MdnsClient.Query (name: hostName,
                                                             type: DnsRecordType.Any,
                                                             addressProtocol: addressProtocol,
                                                             cancellationToken: cancellationToken);

    IPAddress[] addresses = hostRecords.Where (record => record.Type is DnsRecordType.A or DnsRecordType.Aaaa)
                                       .Select (record => record.Address)
                                       .Where (address => address != null)
                                       .Distinct ()
                                       .ToArray ();

    if (addresses.Length == 0)
      return MdnsResolveResult.Empty;

    return new MdnsResolveResult
           {
             HostName = hostName, Port = srvRecord.Port, Addresses = addresses, TxtEntries = txtEntries,
           };
  }

  private static IReadOnlyList<DnsRecord> Query (string            name,
                                                 DnsRecordType     type,
                                                 AddressProtocol   addressProtocol,
                                                 CancellationToken cancellationToken)
  {
    byte[] query   = MdnsClient.BuildQuery (name: name, type: type);
    var    records = new List<DnsRecord> ();

    if (addressProtocol is AddressProtocol.Any or AddressProtocol.IPv4)
      records.AddRange (MdnsClient.QuerySocket (query: query,
                                                endpoint: new IPEndPoint (address: IPAddress.Parse ("224.0.0.251"),
                                                                          port: MdnsClient.DnsPort),
                                                addressFamily: AddressFamily.InterNetwork,
                                                cancellationToken: cancellationToken));

    if (addressProtocol is AddressProtocol.Any or AddressProtocol.IPv6)
      if (Socket.OSSupportsIPv6)
        records.AddRange (MdnsClient.QuerySocket (query: query,
                                                  endpoint: new IPEndPoint (address: IPAddress.Parse ("ff02::fb"),
                                                                            port: MdnsClient.DnsPort),
                                                  addressFamily: AddressFamily.InterNetworkV6,
                                                  cancellationToken: cancellationToken));

    return records;
  }

  private static IReadOnlyList<DnsRecord> QuerySocket (byte[]            query,
                                                       IPEndPoint        endpoint,
                                                       AddressFamily     addressFamily,
                                                       CancellationToken cancellationToken)
  {
    var records = new List<DnsRecord> ();

    using var socket = new Socket (addressFamily: addressFamily, socketType: SocketType.Dgram, protocolType: ProtocolType.Udp);
    socket.SetSocketOption (optionLevel: SocketOptionLevel.Socket, optionName: SocketOptionName.ReuseAddress, optionValue: true);

    if (addressFamily == AddressFamily.InterNetwork)
      socket.Bind (new IPEndPoint (address: IPAddress.Any, port: 0));
    else
      socket.Bind (new IPEndPoint (address: IPAddress.IPv6Any, port: 0));

    socket.SendTo (buffer: query, remoteEP: endpoint);

    DateTime deadline = DateTime.UtcNow + MdnsClient.QueryWindow;
    var      buffer   = new byte[4096];

    while (DateTime.UtcNow <= deadline)
    {
      cancellationToken.ThrowIfCancellationRequested ();

      var timeout = (int)Math.Max (val1: 0, val2: (deadline - DateTime.UtcNow).TotalMilliseconds);

      if (timeout == 0)
        break;

      if (!socket.Poll (microSeconds: timeout * 1000, mode: SelectMode.SelectRead))
        continue;

      EndPoint remote = addressFamily == AddressFamily.InterNetwork
                          ? new IPEndPoint (address: IPAddress.Any,     port: 0)
                          : new IPEndPoint (address: IPAddress.IPv6Any, port: 0);

      int received = socket.ReceiveFrom (buffer: buffer, remoteEP: ref remote);
      records.AddRange (MdnsClient.ParseResponse (buffer.AsSpan (start: 0, length: received)));
    }

    return records;
  }

  private static byte[] BuildQuery (string name, DnsRecordType type)
  {
    var bytes = new List<byte> (512);

    var header = new byte[12];
    BinaryPrimitives.WriteUInt16BigEndian (destination: header.AsSpan (start: 4, length: 2), value: 1);
    bytes.AddRange (header);

    MdnsClient.WriteName (bytes: bytes, name: name);

    bytes.AddRange (BitConverter.GetBytes (IPAddress.HostToNetworkOrder ((short)type)));
    bytes.AddRange (BitConverter.GetBytes (IPAddress.HostToNetworkOrder ((short)1)));

    return bytes.ToArray ();
  }

  private static IReadOnlyList<DnsRecord> ParseResponse (ReadOnlySpan<byte> data)
  {
    if (data.Length < 12)
      return Array.Empty<DnsRecord> ();

    var offset = 0;
    offset += 4;

    ushort questionCount   = MdnsClient.ReadUInt16 (data: data, offset: ref offset);
    ushort answerCount     = MdnsClient.ReadUInt16 (data: data, offset: ref offset);
    ushort authorityCount  = MdnsClient.ReadUInt16 (data: data, offset: ref offset);
    ushort additionalCount = MdnsClient.ReadUInt16 (data: data, offset: ref offset);

    for (var i = 0; i < questionCount; i++)
    {
      _      =  MdnsClient.ReadName (data: data, offset: ref offset);
      offset += 4;
    }

    var records      = new List<DnsRecord> ();
    int totalRecords = answerCount + authorityCount + additionalCount;

    for (var i = 0; i < totalRecords; i++)
    {
      string name = MdnsClient.ReadName (data: data, offset: ref offset);
      var    type = (DnsRecordType)MdnsClient.ReadUInt16 (data: data, offset: ref offset);
      _ = MdnsClient.ReadUInt16 (data: data, offset: ref offset);
      _ = MdnsClient.ReadUInt32 (data: data, offset: ref offset);
      ushort dataLength = MdnsClient.ReadUInt16 (data: data, offset: ref offset);

      if (offset + dataLength > data.Length)
        break;

      int                recordDataOffset = offset;
      ReadOnlySpan<byte> recordData       = data.Slice (start: offset, length: dataLength);
      offset += dataLength;

      var record = new DnsRecord { Name = name, Type = type };

      switch (type)
      {
        case DnsRecordType.Ptr:
          int ptrOffset = recordDataOffset;
          record.PtrTarget = MdnsClient.ReadName (data: data, offset: ref ptrOffset);

          break;

        case DnsRecordType.Srv:
          if (recordData.Length < 6)
            break;

          int srvOffset = recordDataOffset;
          _                 = MdnsClient.ReadUInt16 (data: data, offset: ref srvOffset);
          _                 = MdnsClient.ReadUInt16 (data: data, offset: ref srvOffset);
          record.Port       = MdnsClient.ReadUInt16 (data: data, offset: ref srvOffset);
          record.TargetHost = MdnsClient.ReadName (data: data, offset: ref srvOffset);

          break;

        case DnsRecordType.Txt:
          record.TxtItems = MdnsClient.ParseTxt (recordData);

          break;

        case DnsRecordType.A:
          if (recordData.Length == 4)
            record.Address = new IPAddress (recordData.ToArray ());

          break;

        case DnsRecordType.Aaaa:
          if (recordData.Length == 16)
            record.Address = new IPAddress (recordData.ToArray ());

          break;
      }

      records.Add (record);
    }

    return records;
  }

  private static IEnumerable<TxtRecordItem> ParseTxt (ReadOnlySpan<byte> data)
  {
    var results = new List<TxtRecordItem> ();
    var index   = 0;

    while (index < data.Length)
    {
      byte itemLength = data[index++];

      if ((itemLength == 0) || (index + itemLength > data.Length))
        break;

      string item = Encoding.UTF8.GetString (data.Slice (start: index, length: itemLength));
      index += itemLength;

      int separator = item.IndexOf ('=');

      if (separator <= 0)
        continue;

      string key   = item[..separator];
      string value = item[(separator + 1)..];
      results.Add (new TxtRecordItem (key: key, valueString: value));
    }

    return results;
  }

  private static string ReadName (ReadOnlySpan<byte> data, ref int offset)
  {
    var labels        = new List<string> ();
    var jumped        = false;
    int currentOffset = offset;
    var guard         = 0;

    while (guard++ < data.Length)
    {
      if (currentOffset >= data.Length)
        break;

      byte length = data[currentOffset++];

      if (length == 0)
        break;

      if ((length & 0xC0) == 0xC0)
      {
        if (currentOffset >= data.Length)
          break;

        int pointer = ((length & 0x3F) << 8) | data[currentOffset++];

        if (!jumped)
        {
          offset = currentOffset;
          jumped = true;
        }

        currentOffset = pointer;

        continue;
      }

      if (currentOffset + length > data.Length)
        break;

      labels.Add (Encoding.UTF8.GetString (data.Slice (start: currentOffset, length: length)));
      currentOffset += length;
    }

    if (!jumped)
      offset = currentOffset;

    return MdnsClient.NormalizeName (string.Join (separator: ".", values: labels));
  }

  private static void WriteName (ICollection<byte> bytes, string name)
  {
    foreach (string label in MdnsClient.NormalizeName (name)
                                       .TrimEnd ('.')
                                       .Split (separator: '.', options: StringSplitOptions.RemoveEmptyEntries))
    {
      byte[] labelBytes = Encoding.UTF8.GetBytes (label);
      bytes.Add ((byte)labelBytes.Length);

      foreach (byte labelByte in labelBytes)
        bytes.Add (labelByte);
    }

    bytes.Add (0);
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

    string normalizedRegType = MdnsClient.NormalizeName (regtype).TrimEnd ('.');
    var    marker            = $".{normalizedRegType}.";
    int    markerIndex       = fullName.IndexOf (value: marker, comparisonType: StringComparison.OrdinalIgnoreCase);

    if (markerIndex > 0)
      return fullName[..markerIndex];

    string[] labels = fullName.Split (separator: '.', options: StringSplitOptions.RemoveEmptyEntries);

    return labels.Length > 0 ? labels[0] : fullName;
  }

  private static ushort ReadUInt16 (ReadOnlySpan<byte> data, ref int offset)
  {
    ushort value = BinaryPrimitives.ReadUInt16BigEndian (data.Slice (start: offset, length: 2));
    offset += 2;

    return value;
  }

  private static uint ReadUInt32 (ReadOnlySpan<byte> data, ref int offset)
  {
    uint value = BinaryPrimitives.ReadUInt32BigEndian (data.Slice (start: offset, length: 4));
    offset += 4;

    return value;
  }
}
