#region header

// Arkane.ZeroConf - TxtRecord.cs

#endregion

#region using

using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Text;

#endregion

namespace ArkaneSystems.Arkane.Zeroconf.Providers.Bonjour;

public class TxtRecord : ITxtRecord
{
  private static readonly Encoding Encoding = new ASCIIEncoding ();

  public TxtRecord ()
  {
    this.handle = Marshal.AllocHGlobal (16);
    Native.TXTRecordCreate (txtRecord: this.handle, bufferLen: 0, buffer: IntPtr.Zero);
  }

  public TxtRecord (ushort length, IntPtr buffer)
  {
    this.length = length;
    this.buffer = buffer;
  }

  private readonly IntPtr buffer;
  private readonly ushort length;
  private          IntPtr handle = IntPtr.Zero;

  public TxtRecordItem? this [string key]
  {
    get
    {
      foreach (TxtRecordItem item in this)
      {
        if (item.Key == key)
          return item;
      }

      return null;
    }
  }

  public IntPtr RawBytes => this.handle == IntPtr.Zero ? this.buffer : Native.TXTRecordGetBytesPtr (this.handle);

  public ushort RawLength => this.handle == IntPtr.Zero ? this.length : Native.TXTRecordGetLength (this.handle);

  public int Count => Native.TXTRecordGetCount (txtLen: this.RawLength, txtRecord: this.RawBytes);

  public ITxtRecord BaseRecord => this;

  public void Dispose ()
  {
    if (this.handle != IntPtr.Zero)
    {
      Native.TXTRecordDeallocate (this.handle);
      this.handle = IntPtr.Zero;
    }
  }

  public void Add (string key, string value) { this.Add (new TxtRecordItem (key: key, valueString: value)); }

  public void Add (string key, byte[] value) { this.Add (new TxtRecordItem (key: key, valueRaw: value)); }

  public void Add (TxtRecordItem item)
  {
    if (this.handle == IntPtr.Zero)
      throw new InvalidOperationException ("This TXT Record is read only");

    string key = item.Key;
    if (key[key.Length - 1] != '\0')
      key += "\0";

    ServiceError error = Native.TXTRecordSetValue (txtRecord: this.handle,
                                                   key: Encoding.GetBytes (key + "\0"),
                                                   valueSize: (sbyte)item.ValueRaw.Length,
                                                   value: item.ValueRaw);

    if (error != ServiceError.NoError)
      throw new ServiceErrorException (error);
  }

  public void Remove (string key)
  {
    if (this.handle == IntPtr.Zero)
      throw new InvalidOperationException ("This TXT Record is read only");

    ServiceError error = Native.TXTRecordRemoveValue (txtRecord: this.handle, key: Encoding.GetBytes (key));

    if ((error != ServiceError.NoError) && (error != ServiceError.NoSuchKey))
      throw new ServiceErrorException (error);
  }

  public TxtRecordItem GetItemAt (int index)
  {
    var key = new byte[32];

    if ((index < 0) || (index >= this.Count))
      throw new IndexOutOfRangeException ();

    ServiceError error = Native.TXTRecordGetItemAtIndex (txtLen: this.RawLength,
                                                         txtRecord: this.RawBytes,
                                                         index: (ushort)index,
                                                         keyBufLen: (ushort)key.Length,
                                                         key: key,
                                                         valueLen: out byte valueLength,
                                                         value: out IntPtr valueRaw);

    if (error != ServiceError.NoError)
      throw new ServiceErrorException (error);

    var buffer = new byte[valueLength];
    for (var i = 0; i < valueLength; i++)
      buffer[i] = Marshal.ReadByte (ptr: valueRaw, ofs: i);

    var pos = 0;

    for (; (pos < key.Length) && (key[pos] != 0); pos++) { }

    return new TxtRecordItem (key: Encoding.GetString (bytes: key, index: 0, count: pos), valueRaw: buffer);
  }

  public IEnumerator GetEnumerator () => new TxtRecordEnumerator (this);

  public override string ToString ()
  {
    var ret   = string.Empty;
    var i     = 0;
    int count = this.Count;

    foreach (TxtRecordItem item in this)
    {
      ret += "\"" + item + "\"";
      if (i < count - 1)
        ret += ", ";

      i++;
    }

    return ret;
  }
}
