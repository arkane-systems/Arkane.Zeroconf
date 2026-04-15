#region header

// Arkane.Zeroconf - TxtRecordEnumerator.cs

#endregion

#region using

using System;
using System.Collections;

#endregion

namespace ArkaneSystems.Arkane.Zeroconf.Providers.Bonjour;

internal class TxtRecordEnumerator (TxtRecord record) : IEnumerator
{
  private TxtRecordItem? currentItem;
  private int            index;

  public object Current => this.currentItem ?? throw new InvalidOperationException ("Enumeration has not started.");

  public void Reset ()
  {
    this.index       = 0;
    this.currentItem = null;
  }

  public bool MoveNext ()
  {
    if ((this.index < 0) || (this.index >= record.Count))
      return false;

    this.currentItem = record.GetItemAt (this.index++);

    return this.currentItem != null;
  }
}
