#region header

// Arkane.ZeroConf - ITxtRecord.cs
// 

#endregion

#region using

using System ;
using System.Collections ;

#endregion

namespace ArkaneSystems.Arkane.Zeroconf ;

public interface ITxtRecord : IEnumerable, IDisposable
{
    TxtRecordItem this [string key] { get ; }

    int Count { get ; }

    ITxtRecord BaseRecord { get ; }

    void Add (string key, string value) ;

    void Add (string key, byte[] value) ;

    void Add (TxtRecordItem item) ;

    void Remove (string key) ;

    TxtRecordItem GetItemAt (int index) ;
}
