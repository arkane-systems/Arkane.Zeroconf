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
            var directories = new List <string> () ;

            Assembly asm = Assembly.GetExecutingAssembly () ;

            string envPath = Environment.GetEnvironmentVariable ("MONO_ZEROCONF_PROVIDERS") ;
            if (!string.IsNullOrEmpty (envPath))
                foreach (string path in envPath.Split (':'))
                {
                    if (Directory.Exists (path))
                        directories.Add (path) ;
                }

            string thisAsmPath = asm.Location ;
            directories.Add (Path.GetDirectoryName (thisAsmPath)) ;

            //! We aren't a signed assembly. Ain't no GAC here.

            //if (Assembly.GetExecutingAssembly ().GlobalAssemblyCache)
            //{
            //    string[] path_parts = directories[0].Split (Path.DirectorySeparatorChar) ;
            //    string new_path = Path.DirectorySeparatorChar.ToString () ;
            //    string root = Path.GetPathRoot (this_asm_path) ;
            //    if (root.StartsWith (path_parts[0]))
            //        path_parts[0] = root ;

            //    for (var i = 0; i < path_parts.Length - 4; i++)
            //        new_path = Path.Combine (new_path, path_parts[i]) ;

            //    directories.Add (Path.Combine (new_path, "mono-zeroconf")) ;
            //}

            //! Addition since we built in the Bonjour provider.

            foreach (var provider in asm.GetCustomAttributes (false).OfType <ZeroconfProviderAttribute> ().Select (attr => attr.ProviderType).Select (type => (IZeroconfProvider) Activator.CreateInstance (type)))
            {
                provider.Initialize ();
                providersList.Add (provider);
            }

            //! -- AJRY 2017/04/30

            foreach (string directory in directories)
            {
                foreach (string file in Directory.GetFiles (directory, "Arkane.Zeroconf.Providers.*.dll"))
                {
                    if (Path.GetFileName (file) != Path.GetFileName (thisAsmPath))
                    {
                        Assembly providerAsm = Assembly.LoadFile (file) ;
                        foreach (Attribute attr in providerAsm.GetCustomAttributes (false))
                        {
                            if (attr is ZeroconfProviderAttribute)
                            {
                                Type type = (attr as ZeroconfProviderAttribute).ProviderType ;
                                var provider = (IZeroconfProvider) Activator.CreateInstance (type) ;
                                try
                                {
                                    provider.Initialize () ;
                                    providersList.Add (provider) ;
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine (e) ;
                                }
                            }
                        }
                    }
                }
            }

            if (providersList.Count == 0)
                throw new Exception ("No Zeroconf providers could be found or initialized. Necessary daemon may not be running.") ;

            ProviderFactory.providers = providersList.ToArray () ;

            return ProviderFactory.providers ;
        }
    }
}
