#region header

// Arkane.Zeroconf - TxtRecord.cs
// 

#endregion

#region using

using System ;
using System.Collections ;
using System.Runtime.InteropServices ;
using System.Text ;

#endregion

namespace ArkaneSystems.Arkane.Zeroconf.Providers.Bonjour
{
    public class TxtRecord : ITxtRecord
    {
        private static readonly Encoding encoding = new ASCIIEncoding () ;

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
        private IntPtr handle = IntPtr.Zero ;
        private readonly ushort length ;

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

        public IntPtr RawBytes
        {
            get { return this.handle == IntPtr.Zero ? this.buffer : Native.TXTRecordGetBytesPtr (this.handle) ; }
        }

        public ushort RawLength
        {
            get { return this.handle == IntPtr.Zero ? this.length : Native.TXTRecordGetLength (this.handle) ; }
        }

        public int Count { get { return Native.TXTRecordGetCount (this.RawLength, this.RawBytes) ; } }

        public ITxtRecord BaseRecord { get { return this ; } }

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

            string key = item.Key ;
            if (key[key.Length - 1] != '\0')
                key += "\0" ;

            ServiceError error = Native.TXTRecordSetValue (this.handle,
                                                           TxtRecord.encoding.GetBytes (key + "\0"),
                                                           (sbyte) item.ValueRaw.Length,
                                                           item.ValueRaw) ;

            if (error != ServiceError.NoError)
                throw new ServiceErrorException (error) ;
        }

        public void Remove (string key)
        {
            if (this.handle == IntPtr.Zero)
                throw new InvalidOperationException ("This TXT Record is read only") ;

            ServiceError error = Native.TXTRecordRemoveValue (this.handle, TxtRecord.encoding.GetBytes (key)) ;

            if (error != ServiceError.NoError)
                throw new ServiceErrorException (error) ;
        }

        public TxtRecordItem GetItemAt (int index)
        {
            var key = new byte[32] ;
            byte value_length = 0 ;
            IntPtr value_raw = IntPtr.Zero ;

            if ((index < 0) || (index >= this.Count))
                throw new IndexOutOfRangeException () ;

            ServiceError error = Native.TXTRecordGetItemAtIndex (this.RawLength,
                                                                 this.RawBytes,
                                                                 (ushort) index,
                                                                 (ushort) key.Length,
                                                                 key,
                                                                 out value_length,
                                                                 out value_raw) ;

            if (error != ServiceError.NoError)
                throw new ServiceErrorException (error) ;

            var buffer = new byte[value_length] ;
            for (var i = 0; i < value_length; i++)
                buffer[i] = Marshal.ReadByte (value_raw, i) ;

            var pos = 0 ;
            for (; (pos < key.Length) && (key[pos] != 0); pos++)
                ;

            return new TxtRecordItem (TxtRecord.encoding.GetString (key, 0, pos), buffer) ;
        }

        public IEnumerator GetEnumerator () { return new TxtRecordEnumerator (this) ; }

        public override string ToString ()
        {
            string ret = string.Empty ;
            var i = 0 ;
            int count = this.Count ;

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
}
