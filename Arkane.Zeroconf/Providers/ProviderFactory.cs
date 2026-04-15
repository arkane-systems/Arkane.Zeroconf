#region header

// Arkane.Zeroconf - ProviderFactory.cs

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
  private static readonly Lazy<IZeroconfProvider[]> providers = new (GetProviders);
  private static          IZeroconfProvider?        selectedProvider;

  private static IZeroconfProvider DefaultProvider
  {
    get
    {
      IZeroconfProvider[] available = providers.Value;

      return available.Length == 0
               ? throw new
                   InvalidOperationException ("No Zeroconf providers could be found or initialized. Necessary daemon may not be running.")
               : available[0];
    }
  }

  public static IZeroconfProvider SelectedProvider { get => selectedProvider ?? DefaultProvider; set => selectedProvider = value; }

  /// <summary>
  ///   Gets whether at least one Zeroconf provider was discovered and initialized successfully.
  /// </summary>
  internal static bool HasAnyProvider => (selectedProvider != null) || (providers.Value.Length > 0);

  /// <summary>
  ///   Resets the selected provider back to the automatically discovered default.
  /// </summary>
  internal static void ResetToDefaultProvider () => selectedProvider = null;

  private static IZeroconfProvider[] GetProviders ()
  {
    var providersList = new List<IZeroconfProvider> ();

    var asm = Assembly.GetExecutingAssembly ();

    (IZeroconfProvider? Provider, int Priority)[] candidates =
    [
      .. asm.GetCustomAttributes (false)
            .OfType<ZeroconfProviderAttribute> ()
            .OrderByDescending (attr => attr.Priority)
            .Select (attr => (Provider: Activator
                                           .CreateInstance (attr.ProviderType)
                                          as IZeroconfProvider,
                              attr.Priority)),
    ];

    foreach ((IZeroconfProvider? Provider, int Priority) candidate in candidates)
    {
      if (candidate.Provider == null)
        continue;

      if (!candidate.Provider.IsAvailable ())
        continue;

      candidate.Provider.Initialize ();
      providersList.Add (candidate.Provider);
    }

    return [.. providersList];
  }
}
