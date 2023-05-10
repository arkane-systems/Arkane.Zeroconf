#region header

// Arkane.ZeroConf - TxtRecord.cs
// 

#endregion

#region using

using System ;
using System.Collections ;

using ArkaneSystems.Arkane.Zeroconf.Providers ;

#endregion

namespace ArkaneSystems.Arkane.Zeroconf ;

public class TxtRecord : ITxtRecord
{
    public TxtRecord () => this.BaseRecord = (ITxtRecord) Activator.CreateInstance (ProviderFactory.SelectedProvider.TxtRecord) ;

    public TxtRecordItem this [string index] => this.BaseRecord[index] ;

    public int Count => this.BaseRecord.Count ;

    public ITxtRecord BaseRecord { get ; }

    public void Add (string key, string value) { this.BaseRecord.Add (key, value) ; }

    public void Add (string key, byte[] value) { this.BaseRecord.Add (key, value) ; }

    public void Add (TxtRecordItem item) { this.BaseRecord.Add (item) ; }

    public void Remove (string key) { this.BaseRecord.Remove (key) ; }

    public TxtRecordItem GetItemAt (int index) => this.BaseRecord.GetItemAt (index) ;

    public IEnumerator GetEnumerator () => this.BaseRecord.GetEnumerator () ;

    public void Dispose () { this.BaseRecord.Dispose () ; }
}
