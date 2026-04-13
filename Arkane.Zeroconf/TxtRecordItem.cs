#region header

// Arkane.ZeroConf - TxtRecordItem.cs

#endregion

#region using

using System;
using System.Text;

#endregion

namespace ArkaneSystems.Arkane.Zeroconf;

public class TxtRecordItem
{
  private static readonly Encoding Encoding = new UTF8Encoding ();

  public TxtRecordItem (string key, byte[] valueRaw)
  {
    ArgumentNullException.ThrowIfNull (key);
    ArgumentNullException.ThrowIfNull (valueRaw);

    this.Key      = key;
    this.ValueRaw = valueRaw;
  }

  public TxtRecordItem (string key, string valueString)
  {
    ArgumentNullException.ThrowIfNull (key);
    ArgumentNullException.ThrowIfNull (valueString);

    this.Key         = key;
    this.ValueString = valueString;
  }

  private string? valueString;

  public string Key { get; }

  public byte[] ValueRaw { get; set; } = [];

  public string ValueString
  {
    get
    {
      if (this.valueString != null)
        return this.valueString;

      this.valueString = Encoding.GetString (this.ValueRaw);

      return this.valueString;
    }
    set
    {
      ArgumentNullException.ThrowIfNull (value);
      this.valueString = value;
      this.ValueRaw    = Encoding.GetBytes (value);
    }
  }

  public override string ToString () => string.Format (format: "{0} = {1}", arg0: this.Key, arg1: this.ValueString);
}
