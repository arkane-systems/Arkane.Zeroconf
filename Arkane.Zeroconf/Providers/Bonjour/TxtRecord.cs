#region header

// Arkane.ZeroConf - TxtRecord.cs
// 

#endregion

#region using

using System ;
using System.Collections ;
using System.Runtime.InteropServices ;
using System.Text ;

#endregion

namespace ArkaneSystems.Arkane.Zeroconf.Providers.Bonjour ;

public class TxtRecord : ITxtRecord
{
    private static readonly Encoding Encoding = new ASCIIEncoding () ;

    public TxtRecord ()
    {
        this.handle = Marshal.AllocHGlobal (16) ;
        Native.TXTRecordCreate (this.handle, 0, IntPtr.Zero) ;
    }

    public TxtRecord (ushort length, IntPtr buffer)
    {
        this.length = length ;
        this.buffer = buffer ;
    }

    private readonly IntPtr buffer ;
    private readonly ushort length ;
    private          IntPtr handle = IntPtr.Zero ;

    public TxtRecordItem this [string key]
    {
        get
        {
            foreach (TxtRecordItem item in this)
            {
                if (item.Key == key)
                    return item ;
            }

            return null ;
        }
    }

    public IntPtr RawBytes => this.handle == IntPtr.Zero ? this.buffer : Native.TXTRecordGetBytesPtr (this.handle) ;

    public ushort RawLength => this.handle == IntPtr.Zero ? this.length : Native.TXTRecordGetLength (this.handle) ;

    public int Count => Native.TXTRecordGetCount (this.RawLength, this.RawBytes) ;

    public ITxtRecord BaseRecord => this ;

    public void Dispose ()
    {
        if (this.handle != IntPtr.Zero)
        {
            Native.TXTRecordDeallocate (this.handle) ;
            this.handle = IntPtr.Zero ;
        }
    }

    public void Add (string key, string value) { this.Add (new TxtRecordItem (key, value)) ; }

    public void Add (string key, byte[] value) { this.Add (new TxtRecordItem (key, value)) ; }

    public void Add (TxtRecordItem item)
    {
        if (this.handle == IntPtr.Zero)
            throw new InvalidOperationException ("This TXT Record is read only") ;

        var key = item.Key ;
        if (key[key.Length - 1] != '\0')
            key += "\0" ;

        var error = Native.TXTRecordSetValue (this.handle,
                                              TxtRecord.Encoding.GetBytes (key + "\0"),
                                              (sbyte) item.ValueRaw.Length,
                                              item.ValueRaw) ;

        if (error != ServiceError.NoError)
            throw new ServiceErrorException (error) ;
    }

    public void Remove (string key)
    {
        if (this.handle == IntPtr.Zero)
            throw new InvalidOperationException ("This TXT Record is read only") ;

        var error = Native.TXTRecordRemoveValue (this.handle, TxtRecord.Encoding.GetBytes (key)) ;

        if (error != ServiceError.NoError)
            throw new ServiceErrorException (error) ;
    }

    public TxtRecordItem GetItemAt (int index)
    {
        var key = new byte[32] ;

        if ((index < 0) || (index >= this.Count))
            throw new IndexOutOfRangeException () ;

        var error = Native.TXTRecordGetItemAtIndex (this.RawLength,
                                                    this.RawBytes,
                                                    (ushort) index,
                                                    (ushort) key.Length,
                                                    key,
                                                    out var valueLength,
                                                    out var valueRaw) ;

        if (error != ServiceError.NoError)
            throw new ServiceErrorException (error) ;

        var buffer = new byte[valueLength] ;
        for (var i = 0; i < valueLength; i++)
            buffer[i] = Marshal.ReadByte (valueRaw, i) ;

        var pos = 0 ;
        for (; (pos < key.Length) && (key[pos] != 0); pos++)
        { }

        return new TxtRecordItem (TxtRecord.Encoding.GetString (key, 0, pos), buffer) ;
    }

    public IEnumerator GetEnumerator () => new TxtRecordEnumerator (this) ;

    public override string ToString ()
    {
        var ret   = string.Empty ;
        var i     = 0 ;
        var count = this.Count ;

        foreach (TxtRecordItem item in this)
        {
            ret += "\"" + item + "\"" ;
            if (i < count - 1)
                ret += ", " ;

            i++ ;
        }

        return ret ;
    }
}
