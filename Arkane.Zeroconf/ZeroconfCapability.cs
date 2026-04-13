#region header

// Arkane.ZeroConf - ZeroconfCapability.cs

#endregion

#region using

using System;

#endregion

namespace ArkaneSystems.Arkane.Zeroconf;

/// <summary>
///   Describes the operations supported by a Zeroconf provider.
/// </summary>
[Flags]
public enum ZeroconfCapability
{
  /// <summary>
  ///   No capabilities are supported.
  /// </summary>
  None = 0,

  /// <summary>
  ///   Browsing and lookup are supported.
  /// </summary>
  Browse = 1,

  /// <summary>
  ///   Service publishing is supported.
  /// </summary>
  Publish = 2,
}
