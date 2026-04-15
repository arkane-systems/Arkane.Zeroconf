#region header

// Arkane.Zeroconf - ZeroconfProviderAttribute.cs

#endregion

#region using

using System;

using JetBrains.Annotations;

#endregion

namespace ArkaneSystems.Arkane.Zeroconf.Providers;

[MeansImplicitUse (ImplicitUseTargetFlags.WithMembers)]
[PublicAPI]
[AttributeUsage (AttributeTargets.Assembly, AllowMultiple = true)]
public class ZeroconfProviderAttribute (Type providerType, int priority = 0) : Attribute
{
  public Type ProviderType { get; } = providerType;

  public int Priority { get; } = priority;
}
