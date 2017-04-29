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
        public TxtRecordEnumerator (TxtRecord record) { this.record = record ; }

        private TxtRecordItem current_item ;
        private int index ;
        private readonly TxtRecord record ;

        public object Current { get { return this.current_item ; } }

        public void Reset ()
        {
            this.index = 0 ;
            this.current_item = null ;
        }

        public bool MoveNext ()
        {
            if ((this.index < 0) || (this.index >= this.record.Count))
                return false ;

            this.current_item = this.record.GetItemAt (this.index++) ;
            return this.current_item != null ;
        }
    }
}
