#region header

// Arkane.ZeroConf - NativeWindowsDns.cs

#endregion

#region using

using System;
using System.Runtime.InteropServices;

#endregion

namespace ArkaneSystems.Arkane.Zeroconf.Providers.WindowsMdns;

internal static class NativeWindowsDns
{
  private const string DnsApi = "dnsapi.dll";

  public const uint DnsQueryRequestVersion1 = 1;
  public const int DnsRequestPending = 9506;

  [DllImport (DnsApi, EntryPoint = "DnsRecordListFree")]
  public static extern void DnsRecordListFree (IntPtr pRecordList, DnsFreeType freeType);

  [DllImport (DnsApi, CharSet = CharSet.Unicode, EntryPoint = "DnsServiceBrowse")]
  public static extern int DnsServiceBrowse (ref DnsServiceBrowseRequest request, ref DnsServiceCancel cancel);

  [DllImport (DnsApi, EntryPoint = "DnsServiceBrowseCancel")]
  public static extern int DnsServiceBrowseCancel (ref DnsServiceCancel cancel);

  [DllImport (DnsApi, CharSet = CharSet.Unicode, EntryPoint = "DnsServiceResolve")]
  public static extern int DnsServiceResolve (ref DnsServiceResolveRequest request, ref DnsServiceCancel cancel);

  [DllImport (DnsApi, EntryPoint = "DnsServiceResolveCancel")]
  public static extern int DnsServiceResolveCancel (ref DnsServiceCancel cancel);

  [DllImport (DnsApi, EntryPoint = "DnsServiceFreeInstance")]
  public static extern void DnsServiceFreeInstance (IntPtr instance);

  [UnmanagedFunctionPointer (CallingConvention.Winapi)]
  public delegate void DnsServiceBrowseCallback (uint status, IntPtr queryContext, IntPtr dnsRecord);

  [UnmanagedFunctionPointer (CallingConvention.Winapi)]
  public delegate void DnsServiceResolveComplete (uint status, IntPtr queryContext, IntPtr instance);

  [StructLayout (LayoutKind.Sequential, CharSet = CharSet.Unicode)]
  public struct DnsServiceBrowseRequest
  {
    public uint Version;
    public uint InterfaceIndex;

    [MarshalAs (UnmanagedType.LPWStr)]
    public string QueryName;

    [MarshalAs (UnmanagedType.FunctionPtr)]
    public DnsServiceBrowseCallback BrowseCallback;

    public IntPtr QueryContext;
  }

  [StructLayout (LayoutKind.Sequential, CharSet = CharSet.Unicode)]
  public struct DnsServiceResolveRequest
  {
    public uint Version;
    public uint InterfaceIndex;

    [MarshalAs (UnmanagedType.LPWStr)]
    public string QueryName;

    [MarshalAs (UnmanagedType.FunctionPtr)]
    public DnsServiceResolveComplete ResolveCallback;

    public IntPtr QueryContext;
  }

  [StructLayout (LayoutKind.Sequential)]
  public struct DnsServiceCancel
  {
    public IntPtr Reserved;
  }

  [StructLayout (LayoutKind.Sequential)]
  public struct DnsServiceInstance
  {
    public IntPtr InstanceName;
    public IntPtr HostName;
    public IntPtr Ip4Address;
    public IntPtr Ip6Address;
    public ushort Port;
    public ushort Priority;
    public ushort Weight;
    public uint PropertyCount;
    public IntPtr Keys;
    public IntPtr Values;
    public uint InterfaceIndex;
  }

  public enum DnsFreeType
  {
    Flat = 0,
    RecordList = 1,
    ParsedMessageFields = 2,
  }

  [StructLayout (LayoutKind.Sequential)]
  public struct DnsRecordHeader
  {
    public IntPtr pNext;
    public IntPtr pName;
    public ushort wType;
    public ushort wDataLength;
    public uint Flags;
    public uint dwTtl;
    public uint dwReserved;
  }
}
