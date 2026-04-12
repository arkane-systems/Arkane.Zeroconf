#region header

// Arkane.ZeroConf - MdnsClient.cs
// 

#endregion

#region using

using System ;
using System.Buffers.Binary ;
using System.Collections.Generic ;
using System.Linq ;
using System.Net ;
using System.Net.Sockets ;
using System.Text ;
using System.Threading ;

#endregion

namespace ArkaneSystems.Arkane.Zeroconf.Providers.WindowsMdns ;

internal static class MdnsClient
{
    private const int DnsPort = 5353 ;

    private static readonly TimeSpan QueryWindow = TimeSpan.FromMilliseconds (1200) ;

    public static IReadOnlyList <MdnsServiceInfo> Browse (string regtype,
                                                           string domain,
                                                           AddressProtocol addressProtocol,
                                                           CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull (regtype) ;
        ArgumentNullException.ThrowIfNull (domain) ;

        var queryName = NormalizeName ($"{regtype}.{domain}") ;
        var records = Query (queryName, DnsRecordType.Ptr, addressProtocol, cancellationToken) ;

        var services = new List <MdnsServiceInfo> () ;

        foreach (var record in records.Where (record => record.Type == DnsRecordType.Ptr
                                                     && string.Equals (NormalizeName (record.Name), queryName, StringComparison.OrdinalIgnoreCase)
                                                     && !string.IsNullOrWhiteSpace (record.PtrTarget)))
        {
            var fullName = NormalizeName (record.PtrTarget) ;
            var serviceName = ParseServiceName (fullName, regtype) ;

            services.Add (new MdnsServiceInfo
                          {
                              Name = serviceName,
                              FullName = fullName,
                              RegType = regtype,
                              ReplyDomain = domain,
                              InterfaceIndex = 0,
                          }) ;
        }

        return services.GroupBy (service => service.FullName, StringComparer.OrdinalIgnoreCase)
                       .Select (group => group.First ())
                       .ToArray () ;
    }

    public static MdnsResolveResult Resolve (MdnsServiceInfo service,
                                             AddressProtocol addressProtocol,
                                             CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull (service) ;

        var serviceRecords = Query (service.FullName, DnsRecordType.Any, addressProtocol, cancellationToken) ;

        var srvRecord = serviceRecords.FirstOrDefault (record => record.Type == DnsRecordType.Srv) ;
        if (srvRecord == null)
            return MdnsResolveResult.Empty ;

        var txtEntries = serviceRecords.Where (record => record.Type == DnsRecordType.Txt)
                                       .SelectMany (record => record.TxtItems)
                                       .Where (item => item != null)
                                       .ToArray () ;

        var hostName = NormalizeName (srvRecord.TargetHost) ;
        var hostRecords = Query (hostName, DnsRecordType.Any, addressProtocol, cancellationToken) ;

        var addresses = hostRecords.Where (record => record.Type is DnsRecordType.A or DnsRecordType.Aaaa)
                                   .Select (record => record.Address)
                                   .Where (address => address != null)
                                   .Distinct ()
                                   .ToArray () ;

        if (addresses.Length == 0)
            return MdnsResolveResult.Empty ;

        return new MdnsResolveResult
               {
                   HostName = hostName,
                   Port = srvRecord.Port,
                   Addresses = addresses,
                   TxtEntries = txtEntries,
               } ;
    }

    private static IReadOnlyList <DnsRecord> Query (string name,
                                                     DnsRecordType type,
                                                     AddressProtocol addressProtocol,
                                                     CancellationToken cancellationToken)
    {
        var query = BuildQuery (name, type) ;
        var records = new List <DnsRecord> () ;

        if (addressProtocol is AddressProtocol.Any or AddressProtocol.IPv4)
            records.AddRange (QuerySocket (query, new IPEndPoint (IPAddress.Parse ("224.0.0.251"), DnsPort), AddressFamily.InterNetwork, cancellationToken)) ;

        if (addressProtocol is AddressProtocol.Any or AddressProtocol.IPv6)
        {
            if (Socket.OSSupportsIPv6)
                records.AddRange (QuerySocket (query, new IPEndPoint (IPAddress.Parse ("ff02::fb"), DnsPort), AddressFamily.InterNetworkV6, cancellationToken)) ;
        }

        return records ;
    }

    private static IReadOnlyList <DnsRecord> QuerySocket (byte[] query,
                                                           IPEndPoint endpoint,
                                                           AddressFamily addressFamily,
                                                           CancellationToken cancellationToken)
    {
        var records = new List <DnsRecord> () ;

        using var socket = new Socket (addressFamily, SocketType.Dgram, ProtocolType.Udp) ;
        socket.SetSocketOption (SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true) ;

        if (addressFamily == AddressFamily.InterNetwork)
            socket.Bind (new IPEndPoint (IPAddress.Any, 0)) ;
        else
            socket.Bind (new IPEndPoint (IPAddress.IPv6Any, 0)) ;

        socket.SendTo (query, endpoint) ;

        var deadline = DateTime.UtcNow + QueryWindow ;
        var buffer = new byte[4096] ;

        while (DateTime.UtcNow <= deadline)
        {
            cancellationToken.ThrowIfCancellationRequested () ;

            var timeout = (int) Math.Max (0, (deadline - DateTime.UtcNow).TotalMilliseconds) ;
            if (timeout == 0)
                break ;

            if (!socket.Poll (timeout * 1000, SelectMode.SelectRead))
                continue ;

            EndPoint remote = addressFamily == AddressFamily.InterNetwork
                                  ? new IPEndPoint (IPAddress.Any, 0)
                                  : new IPEndPoint (IPAddress.IPv6Any, 0) ;

            var received = socket.ReceiveFrom (buffer, ref remote) ;
            records.AddRange (ParseResponse (buffer.AsSpan (0, received))) ;
        }

        return records ;
    }

    private static byte[] BuildQuery (string name, DnsRecordType type)
    {
        var bytes = new List <byte> (512) ;

        var header = new byte[12] ;
        BinaryPrimitives.WriteUInt16BigEndian (header.AsSpan (4, 2), 1) ;
        bytes.AddRange (header) ;

        WriteName (bytes, name) ;

        bytes.AddRange (BitConverter.GetBytes (IPAddress.HostToNetworkOrder ((short) type))) ;
        bytes.AddRange (BitConverter.GetBytes (IPAddress.HostToNetworkOrder ((short) 1))) ;

        return bytes.ToArray () ;
    }

    private static IReadOnlyList <DnsRecord> ParseResponse (ReadOnlySpan <byte> data)
    {
        if (data.Length < 12)
            return Array.Empty <DnsRecord> () ;

        var offset = 0 ;
        offset += 4 ;

        var questionCount = ReadUInt16 (data, ref offset) ;
        var answerCount = ReadUInt16 (data, ref offset) ;
        var authorityCount = ReadUInt16 (data, ref offset) ;
        var additionalCount = ReadUInt16 (data, ref offset) ;

        for (var i = 0; i < questionCount; i++)
        {
            _ = ReadName (data, ref offset) ;
            offset += 4 ;
        }

        var records = new List <DnsRecord> () ;
        var totalRecords = answerCount + authorityCount + additionalCount ;

        for (var i = 0; i < totalRecords; i++)
        {
            var name = ReadName (data, ref offset) ;
            var type = (DnsRecordType) ReadUInt16 (data, ref offset) ;
            _ = ReadUInt16 (data, ref offset) ;
            _ = ReadUInt32 (data, ref offset) ;
            var dataLength = ReadUInt16 (data, ref offset) ;

            if (offset + dataLength > data.Length)
                break ;

            var recordDataOffset = offset ;
            var recordData = data.Slice (offset, dataLength) ;
            offset += dataLength ;

            var record = new DnsRecord { Name = name, Type = type } ;

            switch (type)
            {
                case DnsRecordType.Ptr:
                    var ptrOffset = recordDataOffset ;
                    record.PtrTarget = ReadName (data, ref ptrOffset) ;
                    break ;
                case DnsRecordType.Srv:
                    if (recordData.Length < 6)
                        break ;

                    var srvOffset = recordDataOffset ;
                    _ = ReadUInt16 (data, ref srvOffset) ;
                    _ = ReadUInt16 (data, ref srvOffset) ;
                    record.Port = ReadUInt16 (data, ref srvOffset) ;
                    record.TargetHost = ReadName (data, ref srvOffset) ;
                    break ;
                case DnsRecordType.Txt:
                    record.TxtItems = ParseTxt (recordData) ;
                    break ;
                case DnsRecordType.A:
                    if (recordData.Length == 4)
                        record.Address = new IPAddress (recordData.ToArray ()) ;
                    break ;
                case DnsRecordType.Aaaa:
                    if (recordData.Length == 16)
                        record.Address = new IPAddress (recordData.ToArray ()) ;
                    break ;
            }

            records.Add (record) ;
        }

        return records ;
    }

    private static IEnumerable <TxtRecordItem> ParseTxt (ReadOnlySpan <byte> data)
    {
        var results = new List <TxtRecordItem> () ;
        var index = 0 ;

        while (index < data.Length)
        {
            var itemLength = data[index++] ;

            if (itemLength == 0 || index + itemLength > data.Length)
                break ;

            var item = Encoding.UTF8.GetString (data.Slice (index, itemLength)) ;
            index += itemLength ;

            var separator = item.IndexOf ('=') ;
            if (separator <= 0)
                continue ;

            var key = item[..separator] ;
            var value = item[(separator + 1)..] ;
            results.Add (new TxtRecordItem (key, value)) ;
        }

        return results ;
    }

    private static string ReadName (ReadOnlySpan <byte> data, ref int offset)
    {
        var labels = new List <string> () ;
        var jumped = false ;
        var currentOffset = offset ;
        var guard = 0 ;

        while (guard++ < data.Length)
        {
            if (currentOffset >= data.Length)
                break ;

            var length = data[currentOffset++] ;

            if (length == 0)
                break ;

            if ((length & 0xC0) == 0xC0)
            {
                if (currentOffset >= data.Length)
                    break ;

                var pointer = ((length & 0x3F) << 8) | data[currentOffset++] ;

                if (!jumped)
                {
                    offset = currentOffset ;
                    jumped = true ;
                }

                currentOffset = pointer ;
                continue ;
            }

            if (currentOffset + length > data.Length)
                break ;

            labels.Add (Encoding.UTF8.GetString (data.Slice (currentOffset, length))) ;
            currentOffset += length ;
        }

        if (!jumped)
            offset = currentOffset ;

        return NormalizeName (string.Join (".", labels)) ;
    }

    private static void WriteName (ICollection <byte> bytes, string name)
    {
        foreach (var label in NormalizeName (name).TrimEnd ('.').Split ('.', StringSplitOptions.RemoveEmptyEntries))
        {
            var labelBytes = Encoding.UTF8.GetBytes (label) ;
            bytes.Add ((byte) labelBytes.Length) ;

            foreach (var labelByte in labelBytes)
                bytes.Add (labelByte) ;
        }

        bytes.Add (0) ;
    }

    private static string NormalizeName (string value)
    {
        if (string.IsNullOrWhiteSpace (value))
            return string.Empty ;

        return value.Trim ().TrimEnd ('.') + "." ;
    }

    private static string ParseServiceName (string fullName, string regtype)
    {
        if (string.IsNullOrWhiteSpace (fullName))
            return string.Empty ;

        var normalizedRegType = NormalizeName (regtype).TrimEnd ('.') ;
        var marker = $".{normalizedRegType}." ;
        var markerIndex = fullName.IndexOf (marker, StringComparison.OrdinalIgnoreCase) ;

        if (markerIndex > 0)
            return fullName[..markerIndex] ;

        var labels = fullName.Split ('.', StringSplitOptions.RemoveEmptyEntries) ;
        return labels.Length > 0 ? labels[0] : fullName ;
    }

    private static ushort ReadUInt16 (ReadOnlySpan <byte> data, ref int offset)
    {
        var value = BinaryPrimitives.ReadUInt16BigEndian (data.Slice (offset, 2)) ;
        offset += 2 ;
        return value ;
    }

    private static uint ReadUInt32 (ReadOnlySpan <byte> data, ref int offset)
    {
        var value = BinaryPrimitives.ReadUInt32BigEndian (data.Slice (offset, 4)) ;
        offset += 4 ;
        return value ;
    }

    internal sealed class MdnsServiceInfo
    {
        public string Name { get ; init ; }

        public string FullName { get ; init ; }

        public string RegType { get ; init ; }

        public string ReplyDomain { get ; init ; }

        public uint InterfaceIndex { get ; init ; }
    }

    internal sealed class MdnsResolveResult
    {
        public static readonly MdnsResolveResult Empty = new() { Addresses = Array.Empty <IPAddress> (), TxtEntries = Array.Empty <TxtRecordItem> () } ;

        public string HostName { get ; init ; }

        public ushort Port { get ; init ; }

        public IPAddress[] Addresses { get ; init ; }

        public TxtRecordItem[] TxtEntries { get ; init ; }

        public bool IsResolved => this.Addresses?.Length > 0 ;
    }

    private enum DnsRecordType : ushort
    {
        A = 1,
        Ptr = 12,
        Txt = 16,
        Aaaa = 28,
        Srv = 33,
        Any = 255,
    }

    private sealed class DnsRecord
    {
        public string Name { get ; init ; }

        public DnsRecordType Type { get ; init ; }

        public string PtrTarget { get ; set ; }

        public string TargetHost { get ; set ; }

        public ushort Port { get ; set ; }

        public IPAddress Address { get ; set ; }

        public IEnumerable <TxtRecordItem> TxtItems { get ; set ; } = Array.Empty <TxtRecordItem> () ;
    }
}
