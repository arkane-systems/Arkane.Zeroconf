#region header

// Arkane.Zeroconf.Tests - WindowsMdnsProviderTests.cs

#endregion

#region using

using System;
using System.Threading;
using System.Threading.Tasks;

using ArkaneSystems.Arkane.Zeroconf.Providers.WindowsMdns;

using Xunit;

#endregion

namespace ArkaneSystems.Arkane.Zeroconf.Tests.Providers;

[Collection ("WindowsMdns polling interval")]
public class WindowsMdnsProviderTests
{
  [Fact]
  public async Task BrowseService_ResolveAsync_WithCanceledToken_ThrowsOperationCanceledException ()
  {
    // Arrange
    var browseService = new ArkaneSystems.Arkane.Zeroconf.Providers.WindowsMdns.BrowseService (name: "svc",
                                                                                                 fullName: "svc._http._tcp.local.",
                                                                                                 regType: "_http._tcp",
                                                                                                 replyDomain: "local",
                                                                                                 interfaceIndex: 0,
                                                                                                 addressProtocol: AddressProtocol.Any);
    using var cts = new CancellationTokenSource ();
    cts.Cancel ();

    // Act & Assert
    await Assert.ThrowsAsync<OperationCanceledException> (() => browseService.ResolveAsync (cts.Token));
  }

  [Fact]
  public async Task BrowseService_ResolveAsync_WithUnknownService_DoesNotPopulateResolutionData ()
  {
    // Arrange
    var browseService = new ArkaneSystems.Arkane.Zeroconf.Providers.WindowsMdns.BrowseService (name: "missing-service",
                                                                                                 fullName: "missing-service._no-such-service._tcp.local.",
                                                                                                 regType: "_no-such-service._tcp",
                                                                                                 replyDomain: "local",
                                                                                                 interfaceIndex: 0,
                                                                                                 addressProtocol: AddressProtocol.Any);

    // Act
    await browseService.ResolveAsync (TestContext.Current.CancellationToken);

    // Assert
    Assert.Null (browseService.HostEntry);
    Assert.Null (browseService.HostTarget);
    Assert.Null (browseService.TxtRecord);
    Assert.Equal (expected: 0, actual: browseService.UPort);
  }

  [Fact]
  public void ServiceBrowser_Dispose_CanBeCalledBeforeBrowse ()
  {
    // Arrange
    var browser = new Zeroconf.Providers.WindowsMdns.ServiceBrowser ();

    // Act
    browser.Dispose ();
    browser.Dispose ();

    // Assert
    Assert.True (condition: true);
  }

  [Fact]
  public void ServiceBrowser_Browse_WhenAlreadyStarted_ThrowsInvalidOperationException ()
  {
    // Arrange
    var browser = new Zeroconf.Providers.WindowsMdns.ServiceBrowser ();

    try
    {
      browser.Browse (interfaceIndex: 0,
                      addressProtocol: AddressProtocol.Any,
                      regtype: "_workstation._tcp",
                      domain: "local");

      // Act & Assert
      Assert.Throws<InvalidOperationException> (() => browser.Browse (interfaceIndex: 0,
                                                                      addressProtocol: AddressProtocol.Any,
                                                                      regtype: "_workstation._tcp",
                                                                      domain: "local"));
    }
    finally
    {
      browser.Dispose ();
    }
  }

  [Fact]
  public void ServiceBrowser_PollingInterval_DefaultsToFiveSeconds ()
  {
    // Arrange
    TimeSpan original = ArkaneSystems.Arkane.Zeroconf.Providers.WindowsMdns.ServiceBrowser.PollingInterval;

    try
    {
      // Act
      TimeSpan actual = ArkaneSystems.Arkane.Zeroconf.Providers.WindowsMdns.ServiceBrowser.PollingInterval;

      // Assert
      Assert.Equal (expected: TimeSpan.FromSeconds (5), actual: actual);
    }
    finally
    {
      ArkaneSystems.Arkane.Zeroconf.Providers.WindowsMdns.ServiceBrowser.PollingInterval = original;
    }
  }

  [Fact]
  public void ServiceBrowser_PollingInterval_CanSetAndRestore ()
  {
    // Arrange
    TimeSpan original = ArkaneSystems.Arkane.Zeroconf.Providers.WindowsMdns.ServiceBrowser.PollingInterval;
    TimeSpan updated  = TimeSpan.FromSeconds (2);

    try
    {
      // Act
      ArkaneSystems.Arkane.Zeroconf.Providers.WindowsMdns.ServiceBrowser.PollingInterval = updated;

      // Assert
      Assert.Equal (expected: updated,
                    actual: ArkaneSystems.Arkane.Zeroconf.Providers.WindowsMdns.ServiceBrowser.PollingInterval);
    }
    finally
    {
      ArkaneSystems.Arkane.Zeroconf.Providers.WindowsMdns.ServiceBrowser.PollingInterval = original;
    }
  }

  [Fact]
  public void ZeroconfProvider_ExposesBrowseOnlyCapabilities ()
  {
    // Arrange
    var provider = new ZeroconfProvider ();

    // Act
    ZeroconfCapability capabilities = provider.Capabilities;

    // Assert
    Assert.Equal (expected: ZeroconfCapability.Browse, actual: capabilities);
  }

  [Fact]
  public void ZeroconfProvider_IsAvailable_MatchesWindows11Check ()
  {
    // Arrange
    var provider = new ZeroconfProvider ();

    // Act
    bool available = provider.IsAvailable ();

    // Assert
    Assert.Equal (expected: OperatingSystem.IsWindowsVersionAtLeast (major: 10, minor: 0, build: 22000), actual: available);
  }

  [Fact]
  public void RegisterService_Register_ThrowsPlatformNotSupportedException ()
  {
    // Arrange
    var registerService = new Zeroconf.Providers.WindowsMdns.RegisterService ();

    // Act
    var exception = Assert.Throws<PlatformNotSupportedException> (() => registerService.Register ());

    // Assert
    Assert.Contains (expectedSubstring: "ZeroconfSupport.CanPublish",
                     actualString: exception.Message,
                     comparisonType: StringComparison.Ordinal);
  }

  [Fact]
  public void TxtRecord_AddAndRead_RoundTripsValue ()
  {
    // Arrange
    var record = new Zeroconf.Providers.WindowsMdns.TxtRecord ();

    // Act
    record.Add (key: "k", value: "v");
    TxtRecordItem? item = record["k"];

    // Assert
    Assert.NotNull (item);
    Assert.Equal (expected: "v", actual: item.ValueString);
  }

  [Fact]
  public void ServiceBrowser_Browse_WithNullRegtype_ThrowsArgumentNullException ()
  {
    // Arrange
    var browser = new Zeroconf.Providers.WindowsMdns.ServiceBrowser ();

    // Act & Assert
    Assert.Throws<ArgumentNullException> (() => browser.Browse (interfaceIndex: 0,
                                                                addressProtocol: AddressProtocol.Any,
                                                                regtype: null!,
                                                                domain: "local"));
  }
}
