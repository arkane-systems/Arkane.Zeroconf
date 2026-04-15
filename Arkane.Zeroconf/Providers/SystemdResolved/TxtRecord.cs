#region header

// Arkane.Zeroconf - TxtRecord.cs

#endregion

#region using

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

#endregion

namespace ArkaneSystems.Arkane.Zeroconf.Providers.SystemdResolved;

/// <summary>
///   An <see cref="ITxtRecord" /> implementation backed by a simple list, used by the
///   systemd-resolved provider.
/// </summary>
/// <remarks>
///   TXT record items are stored as <see cref="TxtRecordItem" /> instances supporting both
///   string and raw-byte values.  Key comparison is case-insensitive (ordinal).
/// </remarks>
public sealed class TxtRecord : ITxtRecord
{
  private readonly List<TxtRecordItem> items = [];

  public TxtRecordItem? this [string key]
    => this.items.FirstOrDefault (item => string.Equals (a: item.Key, b: key, comparisonType: StringComparison.OrdinalIgnoreCase));

  public int Count => this.items.Count;

  public ITxtRecord BaseRecord => this;

  public void Add (string key, string value)
  {
    ArgumentNullException.ThrowIfNull (key);
    ArgumentNullException.ThrowIfNull (value);

    this.items.Add (new TxtRecordItem (key: key, valueString: value));
  }

  public void Add (string key, byte[] value)
  {
    ArgumentNullException.ThrowIfNull (key);
    ArgumentNullException.ThrowIfNull (value);

    this.items.Add (new TxtRecordItem (key: key, valueRaw: value));
  }

  public void Add (TxtRecordItem item)
  {
    ArgumentNullException.ThrowIfNull (item);
    this.items.Add (item);
  }

  public void Remove (string key)
  {
    ArgumentNullException.ThrowIfNull (key);

    TxtRecordItem? item =
      this.items.FirstOrDefault (current => string.Equals (a: current.Key,
                                                           b: key,
                                                           comparisonType: StringComparison.OrdinalIgnoreCase));

    if (item != null)
      _ = this.items.Remove (item);
  }

  public TxtRecordItem GetItemAt (int index)
  {
    try { return this.items[index]; }
    catch (ArgumentOutOfRangeException ex) { throw new IndexOutOfRangeException (message: ex.Message, innerException: ex); }
  }

  public IEnumerator GetEnumerator () => this.items.GetEnumerator ();

  public void Dispose () => this.items.Clear ();

  internal static TxtRecord FromEntries (IEnumerable<TxtRecordItem> entries)
  {
    var record = new TxtRecord ();

    if (entries == null)
      return record;

    foreach (TxtRecordItem entry in entries)
      record.Add (entry);

    return record;
  }
}
