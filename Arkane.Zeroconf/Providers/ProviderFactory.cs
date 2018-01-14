#region header

// Arkane.Zeroconf - ProviderFactory.cs
// 

#endregion

#region using

using System ;
using System.Collections.Generic ;
using System.IO ;
using System.Linq ;
using System.Reflection ;

#endregion

namespace ArkaneSystems.Arkane.Zeroconf.Providers
{
    internal static class ProviderFactory
    {
        private static IZeroconfProvider[] providers ;
        private static IZeroconfProvider selectedProvider ;

        private static IZeroconfProvider DefaultProvider
        {
            get
            {
                if (ProviderFactory.providers == null)
                    ProviderFactory.GetProviders () ;

                return ProviderFactory.providers[0] ;
            }
        }

        public static IZeroconfProvider SelectedProvider
        {
            get => ProviderFactory.selectedProvider == null
                       ? ProviderFactory.DefaultProvider
                       : ProviderFactory.selectedProvider ;
            set => ProviderFactory.selectedProvider = value ;
        }

        private static IZeroconfProvider[] GetProviders ()
        {
            if (ProviderFactory.providers != null)
                return ProviderFactory.providers ;

            var providersList = new List <IZeroconfProvider> () ;

            Assembly asm = Assembly.GetExecutingAssembly () ;

            foreach (var provider in asm.GetCustomAttributes (false).OfType <ZeroconfProviderAttribute> ().Select (attr => attr.ProviderType).Select (type => (IZeroconfProvider) Activator.CreateInstance (type)))
            {
                provider.Initialize ();
                providersList.Add (provider);
            }

            if (providersList.Count == 0)
                throw new Exception ("No Zeroconf providers could be found or initialized. Necessary daemon may not be running.") ;

            ProviderFactory.providers = providersList.ToArray () ;

            return ProviderFactory.providers ;
        }
    }
}
