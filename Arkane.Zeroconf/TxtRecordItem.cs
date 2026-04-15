#region header

// Arkane.ZeroConf - TxtRecordItem.cs

#endregion

#region using

using System;
using System.Text;

using JetBrains.Annotations;

#endregion

namespace ArkaneSystems.Arkane.Zeroconf;

/// <summary>
///   Represents a single Zeroconf TXT record item.
/// </summary>
[PublicAPI]
public class TxtRecordItem
{
  private static readonly Encoding Encoding = new UTF8Encoding ();

  /// <summary>
  ///   Initializes a new instance of the <see cref="TxtRecordItem" /> class using a raw byte-array value.
  /// </summary>
  /// <param name="key">The TXT record key.</param>
  /// <param name="valueRaw">The raw TXT record value bytes.</param>
  public TxtRecordItem (string key, byte[] valueRaw)
  {
    ArgumentNullException.ThrowIfNull (key);
    ArgumentNullException.ThrowIfNull (valueRaw);

    this.Key      = key;
    this.ValueRaw = valueRaw;
  }

  /// <summary>
  ///   Initializes a new instance of the <see cref="TxtRecordItem" /> class using a string value.
  /// </summary>
  /// <param name="key">The TXT record key.</param>
  /// <param name="valueString">The TXT record string value.</param>
  public TxtRecordItem (string key, string valueString)
  {
    ArgumentNullException.ThrowIfNull (key);
    ArgumentNullException.ThrowIfNull (valueString);

    this.Key         = key;
    this.ValueString = valueString;
  }

  private string? valueString;

  /// <summary>
  ///   Gets the TXT record key.
  /// </summary>
  public string Key { get; }

  /// <summary>
  ///   Gets or sets the raw TXT record value bytes.
  /// </summary>
  public byte[] ValueRaw { get; set; } = [];

  /// <summary>
  ///   Gets or sets the TXT record value as a string.
  /// </summary>
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

  /// <inheritdoc />
  public override string ToString () => string.Format (format: "{0} = {1}", arg0: this.Key, arg1: this.ValueString);
}
