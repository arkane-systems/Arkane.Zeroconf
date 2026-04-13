#region header

// Arkane.ZeroConf - ProviderFactory.cs

#endregion

#region using

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

#endregion

namespace ArkaneSystems.Arkane.Zeroconf.Providers;

internal static class ProviderFactory
{
  private static IZeroconfProvider[]? providers;
  private static IZeroconfProvider?   selectedProvider;

  private static IZeroconfProvider DefaultProvider
  {
    get
    {
      providers ??= GetProviders ();

      return providers[0];
    }
  }

  public static IZeroconfProvider SelectedProvider { get => selectedProvider ?? DefaultProvider; set => selectedProvider = value; }

  private static IZeroconfProvider[] GetProviders ()
  {
    if (providers != null)
      return providers;

    var providersList = new List<IZeroconfProvider> ();

    var asm = Assembly.GetExecutingAssembly ();

    (IZeroconfProvider? Provider, int Priority)[] candidates = asm.GetCustomAttributes (false)
                                                                  .OfType<ZeroconfProviderAttribute> ()
                                                                  .OrderByDescending (attr => attr.Priority)
                                                                  .Select (attr => (Provider: Activator
                                                                                                 .CreateInstance (attr.ProviderType)
                                                                                                as IZeroconfProvider,
                                                                                    attr.Priority))
                                                                  .ToArray ();

    foreach ((IZeroconfProvider? Provider, int Priority) candidate in candidates)
    {
      if (candidate.Provider == null)
        continue;

      if (!candidate.Provider.IsAvailable ())
        continue;

      candidate.Provider.Initialize ();
      providersList.Add (candidate.Provider);
    }

    if (providersList.Count == 0)
      throw new
        InvalidOperationException ("No Zeroconf providers could be found or initialized. Necessary daemon may not be running.");

    providers = providersList.ToArray ();

    return providers;
  }
}
