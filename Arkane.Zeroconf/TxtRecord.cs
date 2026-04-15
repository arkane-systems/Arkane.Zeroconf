#region header

// Arkane.Zeroconf - TxtRecord.cs

#endregion

#region using

using System;
using System.Collections;

using ArkaneSystems.Arkane.Zeroconf.Providers;

using JetBrains.Annotations;

#endregion

namespace ArkaneSystems.Arkane.Zeroconf;

[PublicAPI]
public class TxtRecord : ITxtRecord
{
  /// <summary>
  ///   Initializes a new instance of the <see cref="TxtRecord" /> class.
  /// </summary>
  /// <remarks>
  ///   <para>
  ///     This constructor delegates to the available Zeroconf provider (e.g., Bonjour or Windows mDNS).
  ///     The underlying provider instance is created via reflection using <see cref="Activator.CreateInstance" />.
  ///   </para>
  /// </remarks>
  /// <exception cref="ZeroconfException">
  ///   Thrown when the Zeroconf provider's <see cref="ITxtRecord" /> type cannot be instantiated.
  ///   This typically indicates that no suitable Zeroconf provider (Bonjour, Avahi, or Windows mDNS) is available on the
  ///   current system.
  ///   Ensure that at least one supported mDNS/Bonjour implementation is installed and properly configured.
  /// </exception>
  public TxtRecord ()
    => this.BaseRecord = TxtRecord.CreateRequiredInstance<ITxtRecord> (ProviderFactory.SelectedProvider.TxtRecord);

  public TxtRecordItem? this [string index] => this.BaseRecord[index];

  public int Count => this.BaseRecord.Count;

  public ITxtRecord BaseRecord { get; }

  public void Add (string key, string value)
  {
    ArgumentNullException.ThrowIfNull (key);
    ArgumentNullException.ThrowIfNull (value);

    this.BaseRecord.Add (key: key, value: value);
  }

  public void Add (string key, byte[] value)
  {
    ArgumentNullException.ThrowIfNull (key);
    ArgumentNullException.ThrowIfNull (value);

    this.BaseRecord.Add (key: key, value: value);
  }

  public void Add (TxtRecordItem item)
  {
    ArgumentNullException.ThrowIfNull (item);

    this.BaseRecord.Add (item);
  }

  public void Remove (string key)
  {
    ArgumentNullException.ThrowIfNull (key);

    this.BaseRecord.Remove (key);
  }

  public TxtRecordItem GetItemAt (int index) => this.BaseRecord.GetItemAt (index);

  public IEnumerator GetEnumerator () => this.BaseRecord.GetEnumerator ();

  public void Dispose ()
  {
    GC.SuppressFinalize (this);
    this.BaseRecord.Dispose ();
  }

  private static T CreateRequiredInstance<T> (Type type) where T : class
    => Activator.CreateInstance (type) as T ??
       throw new
         ZeroconfException ("Unable to create the requested Zeroconf service. Ensure a compatible Zeroconf provider is installed and properly configured.");
}
