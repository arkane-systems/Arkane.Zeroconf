#region header

// Arkane.ZeroConf - TxtRecord.cs
// 

#endregion

#region using

using System ;
using System.Collections ;
using System.Collections.Generic ;
using System.Linq ;

#endregion

namespace ArkaneSystems.Arkane.Zeroconf.Providers.WindowsMdns ;

public sealed class TxtRecord : ITxtRecord
{
    private readonly List <TxtRecordItem> items = new() ;

    public TxtRecordItem this [string key] => this.items.FirstOrDefault (item => string.Equals (item.Key, key, StringComparison.OrdinalIgnoreCase)) ;

    public int Count => this.items.Count ;

    public ITxtRecord BaseRecord => this ;

    public void Add (string key, string value)
    {
        ArgumentNullException.ThrowIfNull (key) ;
        ArgumentNullException.ThrowIfNull (value) ;

        this.items.Add (new TxtRecordItem (key, value)) ;
    }

    public void Add (string key, byte[] value)
    {
        ArgumentNullException.ThrowIfNull (key) ;
        ArgumentNullException.ThrowIfNull (value) ;

        this.items.Add (new TxtRecordItem (key, value)) ;
    }

    public void Add (TxtRecordItem item)
    {
        ArgumentNullException.ThrowIfNull (item) ;
        this.items.Add (item) ;
    }

    public void Remove (string key)
    {
        ArgumentNullException.ThrowIfNull (key) ;

        var item = this.items.FirstOrDefault (current => string.Equals (current.Key, key, StringComparison.OrdinalIgnoreCase)) ;
        if (item != null)
            this.items.Remove (item) ;
    }

    public TxtRecordItem GetItemAt (int index) => this.items[index] ;

    public IEnumerator GetEnumerator () => this.items.GetEnumerator () ;

    public void Dispose ()
    {
        this.items.Clear () ;
    }

    internal static TxtRecord FromEntries (IEnumerable <TxtRecordItem> entries)
    {
        var record = new TxtRecord () ;

        if (entries == null)
            return record ;

        foreach (var entry in entries)
            record.Add (entry) ;

        return record ;
    }
}
