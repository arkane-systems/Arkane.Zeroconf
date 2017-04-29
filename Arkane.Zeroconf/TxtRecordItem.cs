#region header

// Arkane.Zeroconf - TxtRecordItem.cs
// 

#endregion

#region using

using System.Text ;

#endregion

namespace ArkaneSystems.Arkane.Zeroconf
{
    public class TxtRecordItem
    {
        private static readonly Encoding encoding = new UTF8Encoding () ;

        public TxtRecordItem (string key, byte[] valueRaw)
        {
            this.Key = key ;
            this.ValueRaw = valueRaw ;
        }

        public TxtRecordItem (string key, string valueString)
        {
            this.Key = key ;
            this.ValueString = valueString ;
        }

        private string value_string ;

        public string Key { get ; }

        public byte[] ValueRaw { get ; set ; }

        public string ValueString
        {
            get
            {
                if (this.value_string != null)
                    return this.value_string ;

                this.value_string = TxtRecordItem.encoding.GetString (this.ValueRaw) ;
                return this.value_string ;
            }
            set
            {
                this.value_string = value ;
                this.ValueRaw = TxtRecordItem.encoding.GetBytes (value) ;
            }
        }

        public override string ToString () { return string.Format ("{0} = {1}", this.Key, this.ValueString) ; }
    }
}
