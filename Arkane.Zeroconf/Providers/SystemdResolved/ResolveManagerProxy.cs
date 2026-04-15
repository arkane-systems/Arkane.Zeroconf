#region header

// Arkane.ZeroConf - ResolveManagerProxy.cs

#endregion

#region using

using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Tmds.DBus.Protocol;

#endregion

namespace ArkaneSystems.Arkane.Zeroconf.Providers.SystemdResolved;

internal static class ResolveManagerProxy
{
  private const string DbusService   = "org.freedesktop.resolve1";
  private const string DbusPath      = "/org/freedesktop/resolve1";
  private const string DbusInterface = "org.freedesktop.resolve1.Manager";

  // AF_INET and AF_INET6 values on Linux
  private const int AfInet  = 2;
  private const int AfInet6 = 10;

  #region Nested type: ServiceResolveResult

  internal record ServiceResolveResult (
    string     HostName,
    ushort     Port,
    IPAddress[] Addresses,
    TxtRecordItem[] TxtItems,
    string     CanonicalName,
    string     CanonicalType,
    string     CanonicalDomain);

  #endregion

  internal static async Task<string> GetMulticastDnsModeAsync (DBusConnection    connection,
                                                               CancellationToken ct = default)
  {
    MessageWriter writer = connection.GetMessageWriter ();

    writer.WriteMethodCallHeader (destination: DbusService,
                                  path: DbusPath,
                                  @interface: "org.freedesktop.DBus.Properties",
                                  member: "Get",
                                  signature: "ss",
                                  flags: MessageFlags.None);

    writer.WriteString (DbusInterface);
    writer.WriteString ("MulticastDNS");

    MessageBuffer buffer = writer.CreateMessage ();

    return await connection.CallMethodAsync (buffer,
                                             static (Message message, object? _) =>
                                             {
                                               Reader reader = message.GetBodyReader ();
                                               VariantValue variant = reader.ReadVariantValue ();

                                               return variant.GetString ();
                                             },
                                             null)
                           .WaitAsync (ct);
  }

  internal static async Task<IReadOnlyList<byte[]>> ResolveRecordAsync (DBusConnection    connection,
                                                                        int               ifindex,
                                                                        string            name,
                                                                        ushort            rrClass,
                                                                        ushort            rrType,
                                                                        ulong             flags,
                                                                        CancellationToken ct = default)
  {
    MessageWriter writer = connection.GetMessageWriter ();

    writer.WriteMethodCallHeader (destination: DbusService,
                                  path: DbusPath,
                                  @interface: DbusInterface,
                                  member: "ResolveRecord",
                                  signature: "isqqt",
                                  flags: MessageFlags.None);

    writer.WriteInt32 (ifindex);
    writer.WriteString (name);
    writer.WriteUInt16 (rrClass);
    writer.WriteUInt16 (rrType);
    writer.WriteUInt64 (flags);

    MessageBuffer buffer = writer.CreateMessage ();

    return await connection.CallMethodAsync (buffer,
                                             static (Message message, object? _) =>
                                             {
                                               Reader reader = message.GetBodyReader ();

                                               var rawRecords = new List<byte[]> ();

                                               ArrayEnd rrArrayEnd = reader.ReadArrayStart (DBusType.Struct);

                                               while (reader.HasNext (rrArrayEnd))
                                               {
                                                 reader.AlignStruct ();

                                                 // Skip ifindex (i), class (q), type (q) — we only need the raw bytes
                                                 _ = reader.ReadInt32 ();
                                                 _ = reader.ReadUInt16 ();
                                                 _ = reader.ReadUInt16 ();

                                                 byte[] rawRr = reader.ReadArrayOfByte ();
                                                 rawRecords.Add (rawRr);
                                               }

                                               // flags (t) — not used
                                               _ = reader.ReadUInt64 ();

                                               return (IReadOnlyList<byte[]>)rawRecords;
                                             },
                                             null)
                           .WaitAsync (ct);
  }

  internal static async Task<ServiceResolveResult?> ResolveServiceAsync (DBusConnection    connection,
                                                                         int               ifindex,
                                                                         string            name,
                                                                         string            type,
                                                                         string            domain,
                                                                         int               family,
                                                                         ulong             flags,
                                                                         CancellationToken ct = default)
  {
    MessageWriter writer = connection.GetMessageWriter ();

    writer.WriteMethodCallHeader (destination: DbusService,
                                  path: DbusPath,
                                  @interface: DbusInterface,
                                  member: "ResolveService",
                                  signature: "isssit",
                                  flags: MessageFlags.None);

    writer.WriteInt32 (ifindex);
    writer.WriteString (name);
    writer.WriteString (type);
    writer.WriteString (domain);
    writer.WriteInt32 (family);
    writer.WriteUInt64 (flags);

    MessageBuffer buffer = writer.CreateMessage ();

    return await connection.CallMethodAsync (buffer,
                                             static (Message message, object? _) =>
                                             {
                                               Reader reader = message.GetBodyReader ();

                                               string     hostname  = string.Empty;
                                               ushort     port      = 0;
                                               var        addresses = new List<IPAddress> ();
                                               var        txtItems  = new List<TxtRecordItem> ();

                                               // srv_records: a(qqqsa(iiay)s)
                                               ArrayEnd srvArrayEnd = reader.ReadArrayStart (DBusType.Struct);

                                               while (reader.HasNext (srvArrayEnd))
                                               {
                                                 reader.AlignStruct ();

                                                 _ = reader.ReadUInt16 (); // priority
                                                 _ = reader.ReadUInt16 (); // weight
                                                 ushort srvPort = reader.ReadUInt16 ();
                                                 string srvHost = reader.ReadString ();

                                                 // addresses: a(iiay)
                                                 ArrayEnd addrArrayEnd = reader.ReadArrayStart (DBusType.Struct);

                                                 while (reader.HasNext (addrArrayEnd))
                                                 {
                                                   reader.AlignStruct ();

                                                   _ = reader.ReadInt32 (); // ifindex
                                                   int    addrFamily = reader.ReadInt32 ();
                                                   byte[] addrBytes  = reader.ReadArrayOfByte ();

                                                   try
                                                   {
                                                     if ((addrFamily == AfInet && addrBytes.Length == 4) ||
                                                         (addrFamily == AfInet6 && addrBytes.Length == 16))
                                                       addresses.Add (new IPAddress (addrBytes));
                                                   }
                                                   catch (Exception ex)
                                                   {
                                                     System.Diagnostics.Debug.WriteLine ($"Failed to parse address: {ex.Message}");
                                                   }
                                                 }

                                                 _ = reader.ReadString (); // canon_hostname

                                                 // Use first SRV record's host and port
                                                 if (port == 0)
                                                 {
                                                   port     = srvPort;
                                                   hostname = srvHost;
                                                 }
                                               }

                                               // txt_records: aay
                                               ArrayEnd txtArrayEnd = reader.ReadArrayStart (DBusType.Array);

                                               while (reader.HasNext (txtArrayEnd))
                                               {
                                                 byte[] txtBytes = reader.ReadArrayOfByte ();
                                                 TxtRecordItem? item = ParseTxtEntry (txtBytes);

                                                 if (item != null)
                                                   txtItems.Add (item);
                                               }

                                               string canonicalName   = reader.ReadString ();
                                               string canonicalType   = reader.ReadString ();
                                               string canonicalDomain = reader.ReadString ();
                                               _ = reader.ReadUInt64 (); // flags

                                               return new ServiceResolveResult (HostName: hostname,
                                                                                Port: port,
                                                                                Addresses: addresses.ToArray (),
                                                                                TxtItems: txtItems.ToArray (),
                                                                                CanonicalName: canonicalName,
                                                                                CanonicalType: canonicalType,
                                                                                CanonicalDomain: canonicalDomain);
                                             },
                                             null)
                           .WaitAsync (ct);
  }

  internal static async Task<string> RegisterServiceAsync (DBusConnection            connection,
                                                           string                    id,
                                                           string                    nameTemplate,
                                                           string                    type,
                                                           ushort                    port,
                                                           ushort                    priority,
                                                           ushort                    weight,
                                                           IEnumerable<TxtRecordItem> txtItems,
                                                           CancellationToken         ct = default)
  {
    MessageWriter writer = connection.GetMessageWriter ();

    writer.WriteMethodCallHeader (destination: DbusService,
                                  path: DbusPath,
                                  @interface: DbusInterface,
                                  member: "RegisterService",
                                  signature: "sssqqqaa{say}",
                                  flags: MessageFlags.None);

    writer.WriteString (id);
    writer.WriteString (nameTemplate);
    writer.WriteString (type);
    writer.WriteUInt16 (port);
    writer.WriteUInt16 (priority);
    writer.WriteUInt16 (weight);

    // aa{say}: outer array of dicts, one dict per TXT record item
    ArrayStart outerArr = writer.WriteArrayStart (DBusType.Array);

    foreach (TxtRecordItem item in txtItems)
    {
      ArrayStart innerDict = writer.WriteDictionaryStart ();
      writer.WriteDictionaryEntryStart ();
      writer.WriteString (item.Key);
      writer.WriteArray (item.ValueRaw ?? Array.Empty<byte> ());
      writer.WriteDictionaryEnd (innerDict);
    }

    writer.WriteArrayEnd (outerArr);

    MessageBuffer buffer = writer.CreateMessage ();

    return await connection.CallMethodAsync (buffer,
                                             static (Message message, object? _) =>
                                             {
                                               Reader reader = message.GetBodyReader ();

                                               return reader.ReadObjectPathAsString ();
                                             },
                                             null)
                           .WaitAsync (ct);
  }

  internal static async Task UnregisterServiceAsync (DBusConnection    connection,
                                                     string            objectPath,
                                                     CancellationToken ct = default)
  {
    MessageWriter writer = connection.GetMessageWriter ();

    writer.WriteMethodCallHeader (destination: DbusService,
                                  path: DbusPath,
                                  @interface: DbusInterface,
                                  member: "UnregisterService",
                                  signature: "o",
                                  flags: MessageFlags.None);

    writer.WriteObjectPath (objectPath);

    MessageBuffer buffer = writer.CreateMessage ();

    await connection.CallMethodAsync (buffer).WaitAsync (ct);
  }

  // Parses PTR RDATA from a raw DNS wire-format RR byte array.
  // Wire format: [owner name in label format][type 2][class 2][ttl 4][rdlength 2][RDATA]
  // For PTR records, RDATA is another domain name in label format.
  internal static string? ExtractPtrTarget (byte[] rawRr)
  {
    // Skip owner name: consume label-encoded name
    int offset = 0;

    while (offset < rawRr.Length)
    {
      byte len = rawRr[offset];

      if (len == 0)
      {
        offset++;
        break;
      }

      if ((len & 0xC0) == 0xC0)
      {
        offset += 2;
        break;
      }

      offset += 1 + len;
    }

    // Skip type(2) + class(2) + TTL(4) + RDLENGTH(2) = 10 bytes
    offset += 10;

    if (offset >= rawRr.Length)
      return null;

    // Read RDATA: domain name in label format
    return ReadDnsName (data: rawRr, offset: offset);
  }

  private static string ReadDnsName (byte[] data, int offset)
  {
    var labels = new List<string> ();

    while (offset < data.Length)
    {
      byte len = data[offset];

      if (len == 0)
        break;

      if ((len & 0xC0) == 0xC0)
        break; // compression (shouldn't occur per spec)

      offset++;
      labels.Add (Encoding.UTF8.GetString (bytes: data, index: offset, count: len));
      offset += len;
    }

    return string.Join (".", labels);
  }

  private static TxtRecordItem? ParseTxtEntry (byte[] data)
  {
    if (data == null || data.Length == 0)
      return null;

    string text   = Encoding.UTF8.GetString (data);
    int    eqIdx  = text.IndexOf ('=');

    if (eqIdx < 0)
      return new TxtRecordItem (key: text, valueRaw: Array.Empty<byte> ());

    return new TxtRecordItem (key: text[..eqIdx],
                              valueRaw: Encoding.UTF8.GetBytes (text[(eqIdx + 1)..]));
  }
}
