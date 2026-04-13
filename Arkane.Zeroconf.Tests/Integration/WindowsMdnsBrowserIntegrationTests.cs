#region header

// Arkane.Zeroconf.Tests - WindowsMdnsBrowserIntegrationTests.cs

#endregion

#region using

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Xunit;

#endregion

namespace ArkaneSystems.Arkane.Zeroconf.Tests.Integration;

public class WindowsMdnsBrowserIntegrationTests
{
  private static readonly string[] DefaultServiceTypes =
  [
    "_http._tcp", "_ipp._tcp", "_printer._tcp", "_ssh._tcp", "_smb._tcp", "_workstation._tcp",
  ];

  [Fact]
  public async Task Browse_CommonServiceTypes_FindsAtLeastOneService_OnWindows11Network ()
  {
    if (!OperatingSystem.IsWindowsVersionAtLeast (major: 10, minor: 0, build: 22000))
      return;

    string[] serviceTypes = WindowsMdnsBrowserIntegrationTests.GetServiceTypesFromConfiguration ();

    TimeSpan discoveryTimeout = TimeSpan.FromSeconds (8);

    Dictionary<string, Task<int>> discoveryTasks = serviceTypes.ToDictionary (keySelector: currentServiceType => currentServiceType,
                                                                              elementSelector: currentServiceType
                                                                                                 => Task
                                                                                                  .Run (()
                                                                                                          => WindowsMdnsBrowserIntegrationTests
                                                                                                           .DiscoverServiceCount (serviceType
                                                                                                                                  : currentServiceType,
                                                                                                                                  timeout
                                                                                                                                  : discoveryTimeout)));

    Task allDiscoveryTasks = Task.WhenAll (discoveryTasks.Values);
    await Task.WhenAny (task1: allDiscoveryTasks,
                        task2: Task.Delay (delay: TimeSpan.FromSeconds (14),
                                           cancellationToken: TestContext.Current.CancellationToken));

    string[] discoveredTypes = discoveryTasks.Where (pair => pair.Value.IsCompletedSuccessfully && (pair.Value.Result > 0))
                                             .Select (pair => pair.Key)
                                             .ToArray ();

    Assert.True (condition: discoveredTypes.Length > 0,
                 userMessage:
                 $"Expected at least one mDNS service for any of [{string.Join (separator: ", ", value: serviceTypes)}]. " +
                 "Configure ARKANE_ZEROCONF_TEST_SERVICE_TYPES with a comma/semicolon-separated list to match your network.");
  }

  private static string[] GetServiceTypesFromConfiguration ()
  {
    string? configuredTypes = Environment.GetEnvironmentVariable ("ARKANE_ZEROCONF_TEST_SERVICE_TYPES");

    if (string.IsNullOrWhiteSpace (configuredTypes))
      return WindowsMdnsBrowserIntegrationTests.DefaultServiceTypes;

    string[] serviceTypes = configuredTypes.Split (separator: [',', ';'],
                                                   options: StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    return serviceTypes.Length == 0 ? WindowsMdnsBrowserIntegrationTests.DefaultServiceTypes : serviceTypes;
  }

  private static int DiscoverServiceCount (string serviceType, TimeSpan timeout)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace (serviceType);

    using var browser         = new ServiceBrowser ();
    using var discoveredEvent = new ManualResetEventSlim (false);

    var discoveredCount = 0;

    browser.ServiceAdded += (o, args) =>
                            {
                              Interlocked.Increment (ref discoveredCount);
                              discoveredEvent.Set ();
                            };

    browser.Browse (interfaceIndex: 0,
                    addressProtocol: AddressProtocol.Any,
                    regtype: serviceType,
                    domain: "local");

    discoveredEvent.Wait (timeout);

    return discoveredCount;
  }
}
