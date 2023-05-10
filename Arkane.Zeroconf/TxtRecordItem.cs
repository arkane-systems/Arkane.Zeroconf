#region header

// Arkane.ZeroConf - TxtRecordItem.cs
// 

#endregion

#region using

using System.Text ;

#endregion

namespace ArkaneSystems.Arkane.Zeroconf ;

public class TxtRecordItem
{
    private static readonly Encoding Encoding = new UTF8Encoding () ;

    public TxtRecordItem (string key, byte[] valueRaw)
    {
        this.Key      = key ;
        this.ValueRaw = valueRaw ;
    }

    public TxtRecordItem (string key, string valueString)
    {
        this.Key         = key ;
        this.ValueString = valueString ;
    }

    private string valueString ;

    public string Key { get ; }

    public byte[] ValueRaw { get ; set ; }

    public string ValueString
    {
        get
        {
            if (this.valueString != null)
                return this.valueString ;

            this.valueString = TxtRecordItem.Encoding.GetString (this.ValueRaw) ;
            return this.valueString ;
        }
        set
        {
            this.valueString = value ;
            this.ValueRaw    = TxtRecordItem.Encoding.GetBytes (value) ;
        }
    }

    public override string ToString () => string.Format ("{0} = {1}", this.Key, this.ValueString) ;
}
