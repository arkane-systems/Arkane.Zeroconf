#region header

// azclient - ZeroconfClient.cs
// 

#endregion

#region using

using System ;
using System.Collections ;
using System.Linq ;
using System.Text.RegularExpressions ;
using System.Threading ;

using ArkaneSystems.Arkane.Zeroconf ;

#endregion

public class MZClient
{
    private static bool resolve_shares ;
    private static uint @interface ;
    private static AddressProtocol address_protocol = AddressProtocol.Any ;
    private static string domain = "local" ;
    private static readonly string app_name = "azclient" ;
    private static bool verbose ;

    public static int Main (string[] args)
    {
        var type = "_workstation._tcp" ;
        var show_help = false ;
        var services = new ArrayList () ;

        for (var i = 0; i < args.Length; i++)
        {
            if (args[i][0] != '-')
                continue ;

            switch (args[i])
            {
                case "-t":
                case "--type":
                    type = args[++i] ;
                    break ;
                case "-r":
                case "--resolve":
                    MZClient.resolve_shares = true ;
                    break ;
                case "-p":
                case "--publish":
                    services.Add (args[++i]) ;
                    break ;
                case "-i":
                case "--interface":
                    if (!uint.TryParse (args[++i], out MZClient.@interface))
                    {
                        Console.Error.WriteLine ("Invalid interface index, '{0}'", args[i]) ;
                        show_help = true ;
                    }
                    break ;
                case "-a":
                case "--aprotocol":
                    string proto = args[++i].ToLower ().Trim () ;
                    switch (proto)
                    {
                        case "ipv4":
                        case "4":
                            MZClient.address_protocol = AddressProtocol.IPv4 ;
                            break ;
                        case "ipv6":
                        case "6":
                            MZClient.address_protocol = AddressProtocol.IPv6 ;
                            break ;
                        case "any":
                        case "all":
                            MZClient.address_protocol = AddressProtocol.Any ;
                            break ;
                        default:
                            Console.Error.WriteLine ("Invalid IP Address Protocol, '{0}'", args[i]) ;
                            show_help = true ;
                            break ;
                    }

                    break ;
                case "-d":
                case "--domain":
                    MZClient.domain = args[++i] ;
                    break ;
                case "-h":
                case "--help":
                    show_help = true ;
                    break ;
                case "-v":
                case "--verbose":
                    MZClient.verbose = true ;
                    break ;
            }
        }

        if (show_help)
        {
            Console.WriteLine ("Usage: {0} [-t type] [--resolve] [--publish \"description\"]", MZClient.app_name) ;
            Console.WriteLine () ;
            Console.WriteLine ("    -h|--help       shows this help") ;
            Console.WriteLine ("    -v|--verbose    print verbose details of what's happening") ;
            Console.WriteLine ("    -t|--type       uses 'type' as the service type") ;
            Console.WriteLine ("                    (default is '_workstation._tcp')") ;
            Console.WriteLine ("    -r|--resolve    resolve found services to hosts") ;
            Console.WriteLine ("    -d|--domain     which domain to broadcast/listen on") ;
            Console.WriteLine ("    -i|--interface  which network interface index to listen") ;
            Console.WriteLine ("                    on (default is '0', meaning 'all')") ;
            Console.WriteLine ("    -a|--aprotocol  which address protocol to use (Any, IPv4, IPv6)") ;
            Console.WriteLine ("    -p|--publish    publish a service of 'description'") ;
            Console.WriteLine () ;
            Console.WriteLine ("The -d, -i and -a options are optional. By default {0} will listen", MZClient.app_name) ;
            Console.WriteLine ("on all network interfaces ('0') on the 'local' domain, and will resolve ") ;
            Console.WriteLine ("all address types, IPv4 and IPv6, as available.") ;
            Console.WriteLine () ;
            Console.WriteLine ("The service description for publishing has the following syntax.") ;
            Console.WriteLine ("The TXT record is optional.\n") ;
            Console.WriteLine ("    <type> <port> <name> TXT [ <key>='<value>', ... ]\n") ;
            Console.WriteLine ("For example:\n") ;
            Console.WriteLine ("    -p \"_http._tcp 80 Simple Web Server\"") ;
            Console.WriteLine ("    -p \"_daap._tcp 3689 Aaron's Music TXT [ Password='false', \\") ;
            Console.WriteLine ("        Machine Name='Aaron\\'s Box', txtvers='1' ]\"") ;
            Console.WriteLine () ;
            return 1 ;
        }

        if (services.Count > 0)
        {
            foreach (string service_description in services)
                MZClient.RegisterService (service_description) ;
        }
        else
        {
            if (MZClient.verbose)
            {
                Console.WriteLine ("Creating a ServiceBrowser with the following settings:") ;
                Console.WriteLine ("  Interface         = {0}",
                                   MZClient.@interface == 0 ? "0 (All)" : MZClient.@interface.ToString ()) ;
                Console.WriteLine ("  Address Protocol  = {0}", MZClient.address_protocol) ;
                Console.WriteLine ("  Domain            = {0}", MZClient.domain) ;
                Console.WriteLine ("  Registration Type = {0}", type) ;
                Console.WriteLine ("  Resolve Shares    = {0}", MZClient.resolve_shares) ;
                Console.WriteLine () ;
            }

            Console.WriteLine ("Hit ^C when you're bored waiting for responses.") ;
            Console.WriteLine () ;

            // Listen for events of some service type
            var browser = new ServiceBrowser () ;
            browser.ServiceAdded += MZClient.OnServiceAdded ;
            browser.ServiceRemoved += MZClient.OnServiceRemoved ;
            browser.Browse (MZClient.@interface, MZClient.address_protocol, type, MZClient.domain) ;
        }

        while (true)
            Thread.Sleep (1000) ;
    }

    private static void RegisterService (string serviceDescription)
    {
        Match match = Regex.Match (serviceDescription, @"(_[a-z]+._tcp|udp)\s*(\d+)\s*(.*)") ;
        if (match.Groups.Count < 4)
            throw new ApplicationException ("Invalid service description syntax") ;

        string type = match.Groups[1].Value.Trim () ;
        short port = Convert.ToInt16 (match.Groups[2].Value) ;
        string name = match.Groups[3].Value.Trim () ;

        int txt_pos = name.IndexOf ("TXT") ;
        string txt_data = null ;

        if (txt_pos > 0)
        {
            txt_data = name.Substring (txt_pos).Trim () ;
            name = name.Substring (0, txt_pos).Trim () ;

            if (txt_data == string.Empty)
                txt_data = null ;
        }

        var service = new RegisterService () ;
        service.Name = name ;
        service.RegType = type ;
        service.ReplyDomain = "local." ;
        service.Port = port ;

        TxtRecord record = null ;

        if (txt_data != null)
        {
            Match tmatch = Regex.Match (txt_data, @"TXT\s*\[(.*)\]") ;

            if (tmatch.Groups.Count != 2)
                throw new ApplicationException ("Invalid TXT record definition syntax") ;

            txt_data = tmatch.Groups[1].Value ;

            foreach (string part in Regex.Split (txt_data, @"'\s*,"))
            {
                string expr = part.Trim () ;
                if (!expr.EndsWith ("'"))
                    expr += "'" ;

                Match pmatch = Regex.Match (expr, @"(\w+\s*\w*)\s*=\s*['](.*)[']\s*") ;
                string key = pmatch.Groups[1].Value.Trim () ;
                string val = pmatch.Groups[2].Value.Trim () ;

                if ((key == null) || (key == string.Empty) || (val == null) || (val == string.Empty))
                    throw new ApplicationException ("Invalid key = 'value' syntax for TXT record item") ;

                if (record == null)
                    record = new TxtRecord () ;

                record.Add (key, val) ;
            }
        }

        if (record != null)
            service.TxtRecord = record ;

        Console.WriteLine ("*** Registering name = '{0}', type = '{1}', domain = '{2}'",
                           service.Name,
                           service.RegType,
                           service.ReplyDomain) ;

        service.Response += MZClient.OnRegisterServiceResponse ;
        service.Register () ;
    }

    private static void OnServiceAdded (object o, ServiceBrowseEventArgs args)
    {
        Console.WriteLine ("*** Found name = '{0}', type = '{1}', domain = '{2}'",
                           args.Service.Name,
                           args.Service.RegType,
                           args.Service.ReplyDomain) ;

        if (MZClient.resolve_shares)
        {
            args.Service.Resolved += MZClient.OnServiceResolved ;
            args.Service.Resolve () ;
        }
    }

    private static void OnServiceRemoved (object o, ServiceBrowseEventArgs args)
    {
        Console.WriteLine ("*** Lost  name = '{0}', type = '{1}', domain = '{2}'",
                           args.Service.Name,
                           args.Service.RegType,
                           args.Service.ReplyDomain) ;
    }

    private static void OnServiceResolved (object o, ServiceResolvedEventArgs args)
    {
        var service = o as IResolvableService ;
        Console.Write ("*** Resolved name = '{0}', host ip = '{1}', hostname = {2}, port = '{3}', " +
                       "interface = '{4}', address type = '{5}'",
                       service.FullName,
                       // service.HostEntry.AddressList[0],
                       string.Join (", ", service.HostEntry.AddressList.Select (s => s.ToString()).ToArray ()),
                       service.HostEntry.HostName,
                       service.Port,
                       service.NetworkInterface,
                       service.AddressProtocol) ;

        ITxtRecord record = service.TxtRecord ;
        int record_count = record != null ? record.Count : 0 ;
        if (record_count > 0)
        {
            Console.Write (", TXT Record = [") ;
            for (int i = 0, n = record.Count; i < n; i++)
            {
                TxtRecordItem item = record.GetItemAt (i) ;
                Console.Write ("{0} = '{1}'", item.Key, item.ValueString) ;
                if (i < n - 1)
                    Console.Write (", ") ;
            }

            Console.WriteLine ("]") ;
        }
        else
        {
            Console.WriteLine ("") ;
        }
    }

    private static void OnRegisterServiceResponse (object o, RegisterServiceEventArgs args)
    {
        switch (args.ServiceError)
        {
            case ServiceErrorCode.NameConflict:
                Console.WriteLine ("*** Name Collision! '{0}' is already registered",
                                   args.Service.Name) ;
                break ;
            case ServiceErrorCode.None:
                Console.WriteLine ("*** Registered name = '{0}'", args.Service.Name) ;
                break ;
            case ServiceErrorCode.Unknown:
                Console.WriteLine ("*** Error registering name = '{0}'", args.Service.Name) ;
                break ;
        }
    }
}
