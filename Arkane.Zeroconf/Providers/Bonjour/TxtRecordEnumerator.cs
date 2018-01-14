#region header

// Arkane.Zeroconf - TxtRecordEnumerator.cs
// 

#endregion

#region using

using System.Collections ;

#endregion

namespace ArkaneSystems.Arkane.Zeroconf.Providers.Bonjour
{
    internal class TxtRecordEnumerator : IEnumerator
    {
        public TxtRecordEnumerator (TxtRecord record) => this.record = record ;

        private TxtRecordItem currentItem ;
        private int index ;
        private readonly TxtRecord record ;

        public object Current => this.currentItem ;

        public void Reset ()
        {
            this.index = 0 ;
            this.currentItem = null ;
        }

        public bool MoveNext ()
        {
            if ((this.index < 0) || (this.index >= this.record.Count))
                return false ;

            this.currentItem = this.record.GetItemAt (this.index++) ;
            return this.currentItem != null ;
        }
    }
}
