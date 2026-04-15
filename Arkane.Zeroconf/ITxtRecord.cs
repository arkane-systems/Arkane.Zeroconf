#region header

// Arkane.ZeroConf - ITxtRecord.cs

#endregion

#region using

using System;
using System.Collections;

using JetBrains.Annotations;

#endregion

namespace ArkaneSystems.Arkane.Zeroconf;

/// <summary>
///   Represents a Zeroconf TXT record collection.
/// </summary>
/// <remarks>
///   TXT records store service metadata as key/value entries. Providers may expose provider-specific
///   backing implementations through <see cref="BaseRecord" />, while the public contract remains provider-agnostic.
/// </remarks>
[PublicAPI]
public interface ITxtRecord : IEnumerable, IDisposable
{
  /// <summary>
  ///   Gets the first TXT record item matching the specified key, or <see langword="null" /> when no match exists.
  /// </summary>
  /// <param name="key">The TXT record key.</param>
  TxtRecordItem? this [string key] { get; }

  /// <summary>
  ///   Gets the number of TXT record items in the collection.
  /// </summary>
  int Count { get; }

  /// <summary>
  ///   Gets the provider-specific backing TXT record implementation.
  /// </summary>
  ITxtRecord BaseRecord { get; }

  /// <summary>
  ///   Adds a TXT record item using a string value.
  /// </summary>
  /// <param name="key">The TXT record key.</param>
  /// <param name="value">The TXT record string value.</param>
  void Add (string key, string value);

  /// <summary>
  ///   Adds a TXT record item using a raw byte array value.
  /// </summary>
  /// <param name="key">The TXT record key.</param>
  /// <param name="value">The TXT record raw value bytes.</param>
  void Add (string key, byte[] value);

  /// <summary>
  ///   Adds a TXT record item.
  /// </summary>
  /// <param name="item">The item to add.</param>
  void Add (TxtRecordItem item);

  /// <summary>
  ///   Removes TXT record items matching the specified key.
  /// </summary>
  /// <param name="key">The TXT record key to remove.</param>
  void Remove (string key);

  /// <summary>
  ///   Gets the TXT record item at the specified index.
  /// </summary>
  /// <param name="index">The zero-based index.</param>
  /// <returns>The TXT record item at the specified index.</returns>
  TxtRecordItem GetItemAt (int index);
}
