#region header

// Arkane.ZeroConf - ZeroconfProviderAttribute.cs

#endregion

#region using

using System;

#endregion

namespace ArkaneSystems.Arkane.Zeroconf.Providers;

[AttributeUsage (AttributeTargets.Assembly, AllowMultiple = true)]
public class ZeroconfProviderAttribute : Attribute
{
  public ZeroconfProviderAttribute (Type providerType, int priority = 0)
  {
    this.ProviderType = providerType;
    this.Priority     = priority;
  }

  public Type ProviderType { get; }

  public int Priority { get; }
}
