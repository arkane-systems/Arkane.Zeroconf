#region header

// Arkane.Zeroconf - ZeroconfProviderAttribute.cs

#endregion

#region using

using System;

#endregion

namespace ArkaneSystems.Arkane.Zeroconf.Providers;

[AttributeUsage (AttributeTargets.Assembly, AllowMultiple = true)]
public class ZeroconfProviderAttribute (Type providerType, int priority = 0) : Attribute
{
  public Type ProviderType { get; } = providerType;

  public int Priority { get; } = priority;
}
