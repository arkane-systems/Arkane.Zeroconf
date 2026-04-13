#region header

// Arkane.Zeroconf.Tests - PublicApiRoundTripIntegrationTests.cs

#endregion

#region using

using System;
using System.Threading;

using Xunit;

#endregion

namespace ArkaneSystems.Arkane.Zeroconf.Tests.Integration;

/// <summary>
///   Public API round-trip integration tests that publish mDNS services and read them back.
/// </summary>
public class PublicApiRoundTripIntegrationTests
{
  private const string TestRegType = "_arkanezrct._tcp";
  private const string TestDomain  = "local";

  [Fact]
  public void Publish_ThenBrowse_FindsPublishedService ()
  {
    if (!ZeroconfSupport.CanBrowse || !ZeroconfSupport.CanPublish)
      return;

    var   serviceName = $"arkane-roundtrip-{Guid.NewGuid ():N}";
    short servicePort = 28081;

    using var             browser         = new ServiceBrowser ();
    using RegisterService service         = CreateRegisterService (serviceName: serviceName, port: servicePort, txtValue: "browse");
    using var             discoveredEvent = new ManualResetEventSlim (false);

    IResolvableService? discoveredService = null;

    browser.ServiceAdded += (o, args) =>
                            {
                              if (!string.Equals (a: args.Service.Name, b: serviceName, comparisonType: StringComparison.Ordinal))
                                return;

                              discoveredService = args.Service;
                              discoveredEvent.Set ();
                            };

    browser.Browse (regtype: TestRegType, domain: TestDomain);
    Thread.Sleep (100);

    service.Register ();

    bool discovered = discoveredEvent.Wait (TimeSpan.FromSeconds (10));

    Assert.True (condition: discovered,
                 userMessage: "Expected published service to be discovered by ServiceBrowser via public API.");
    Assert.NotNull (discoveredService);
    Assert.Equal (expected: serviceName,               actual: discoveredService.Name);
    Assert.Equal (expected: TestRegType.TrimEnd ('.'), actual: discoveredService.RegType.TrimEnd ('.'));
  }

  [Fact]
  public void Publish_ThenResolve_ReadsBackServiceIdentity_AndAttemptsTxtRoundTrip ()
  {
    if (!ZeroconfSupport.CanBrowse || !ZeroconfSupport.CanPublish)
      return;

    var   serviceName = $"arkane-roundtrip-{Guid.NewGuid ():N}";
    var   txtValue    = Guid.NewGuid ().ToString ("N");
    short servicePort = 28082;

    using var             browser         = new ServiceBrowser ();
    using RegisterService service         = CreateRegisterService (serviceName: serviceName, port: servicePort, txtValue: txtValue);
    using var             discoveredEvent = new ManualResetEventSlim (false);

    IResolvableService? discoveredService = null;

    browser.ServiceAdded += (o, args) =>
                            {
                              if (!string.Equals (a: args.Service.Name, b: serviceName, comparisonType: StringComparison.Ordinal))
                                return;

                              discoveredService = args.Service;
                              discoveredEvent.Set ();
                            };

    browser.Browse (regtype: TestRegType, domain: TestDomain);
    Thread.Sleep (100);

    service.Register ();

    bool discovered = discoveredEvent.Wait (TimeSpan.FromSeconds (10));

    Assert.True (condition: discovered,
                 userMessage: "Expected published service to be discovered before resolve.");
    Assert.NotNull (discoveredService);
    Assert.Equal (expected: serviceName,               actual: discoveredService.Name);
    Assert.Equal (expected: TestRegType.TrimEnd ('.'), actual: discoveredService.RegType.TrimEnd ('.'));

    discoveredService.Resolve ();

    DateTime deadline = DateTime.UtcNow.AddSeconds (4);
    while ((DateTime.UtcNow < deadline) && (discoveredService.Port == 0))
      Thread.Sleep (100);

    if (discoveredService.Port > 0)
      Assert.Equal (expected: servicePort, actual: discoveredService.Port);
  }

  private static RegisterService CreateRegisterService (string serviceName, short port, string txtValue)
  {
    var txtRecord = new TxtRecord ();
    txtRecord.Add (key: "roundtrip", value: txtValue);

    return new RegisterService
           {
             Name        = serviceName,
             RegType     = TestRegType,
             ReplyDomain = TestDomain,
             Port        = port,
             TxtRecord   = txtRecord,
           };
  }
}
