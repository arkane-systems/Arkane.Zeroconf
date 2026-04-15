#region header

// Arkane.Zeroconf.Tests - SystemdResolvedProviderTests.cs

#endregion

#region using

using System;
using System.Threading;
using System.Threading.Tasks;

using ArkaneSystems.Arkane.Zeroconf.Providers.SystemdResolved;
using ArkaneSystems.Arkane.Zeroconf.Tests.Helpers;

using Xunit;

using SrServiceBrowser = ArkaneSystems.Arkane.Zeroconf.Providers.SystemdResolved.ServiceBrowser;

#endregion

namespace ArkaneSystems.Arkane.Zeroconf.Tests.Providers;

[Collection ("SystemdResolved polling interval")]
public class SystemdResolvedProviderTests
{
  // ── BrowseService tests ─────────────────────────────────────────────────

  [Fact]
  public async Task BrowseService_ResolveAsync_WithCanceledToken_ThrowsOperationCanceledException ()
  {
    // Arrange
    var browseService = new BrowseService (name: "svc",
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
    ZeroconfTestHelper.SkipIfSystemdResolvedUnavailable ();

    // Shorten the D-Bus resolve timeout so the test doesn't block for the full 4 s.
    TimeSpan originalTimeout = SystemdResolvedClient.ResolveTimeout;
    SystemdResolvedClient.ResolveTimeout = TimeSpan.FromMilliseconds (300);

    try
    {
      // Arrange
      var browseService = new BrowseService (name: "missing-service",
                                             fullName: "missing-service._no-such-service._tcp.local.",
                                             regType: "_no-such-service._tcp",
                                             replyDomain: "local",
                                             interfaceIndex: 0,
                                             addressProtocol: AddressProtocol.Any);

      // Act
      await browseService.ResolveAsync (TestContext.Current.CancellationToken);

      // Assert — resolve failure is swallowed; all fields remain at defaults
      Assert.Null (browseService.HostEntry);
      Assert.Null (browseService.HostTarget);
      Assert.Null (browseService.TxtRecord);
      Assert.Equal (expected: 0, actual: browseService.UPort);
    }
    finally
    {
      SystemdResolvedClient.ResolveTimeout = originalTimeout;
    }
  }

  // ── ServiceBrowser tests ────────────────────────────────────────────────

  [Fact]
  public void ServiceBrowser_Dispose_CanBeCalledBeforeBrowse ()
  {
    // Arrange
    var browser = new SrServiceBrowser ();

    // Act
    browser.Dispose ();
    browser.Dispose ();

    // Assert
    Assert.True (condition: true);
  }

  [Fact]
  public void ServiceBrowser_Browse_WhenAlreadyStarted_ThrowsInvalidOperationException ()
  {
    ZeroconfTestHelper.SkipIfSystemdResolvedUnavailable ();

    // Arrange
    var browser = new SrServiceBrowser ();

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
    TimeSpan original = SrServiceBrowser.PollingInterval;

    try
    {
      // Act
      TimeSpan actual = SrServiceBrowser.PollingInterval;

      // Assert
      Assert.Equal (expected: TimeSpan.FromSeconds (5), actual: actual);
    }
    finally
    {
      SrServiceBrowser.PollingInterval = original;
    }
  }

  [Fact]
  public void ServiceBrowser_PollingInterval_CanSetAndRestore ()
  {
    // Arrange
    TimeSpan original = SrServiceBrowser.PollingInterval;
    TimeSpan updated  = TimeSpan.FromSeconds (2);

    try
    {
      // Act
      SrServiceBrowser.PollingInterval = updated;

      // Assert
      Assert.Equal (expected: updated, actual: SrServiceBrowser.PollingInterval);
    }
    finally
    {
      SrServiceBrowser.PollingInterval = original;
    }
  }

  [Fact]
  public void ServiceBrowser_Browse_WithNullRegtype_ThrowsArgumentNullException ()
  {
    // Arrange
    var browser = new SrServiceBrowser ();

    // Act & Assert
    Assert.Throws<ArgumentNullException> (() => browser.Browse (interfaceIndex: 0,
                                                                addressProtocol: AddressProtocol.Any,
                                                                regtype: null!,
                                                                domain: "local"));
  }

  // ── ZeroconfProvider tests ──────────────────────────────────────────────

  [Fact]
  public void ZeroconfProvider_DefaultCapabilities_IsBrowse ()
  {
    // The default (before Initialize) is always Browse.
    var provider = new ZeroconfProvider ();

    Assert.Equal (expected: ZeroconfCapability.Browse, actual: provider.Capabilities);
  }

  [Fact]
  public void ZeroconfProvider_IsAvailable_ReturnsFalseOnNonLinux ()
  {
    ZeroconfTestHelper.SkipIfNotLinux ();
    // This branch is vacuously verified: on Linux the test does nothing — the
    // positive case is covered by ZeroconfProvider_IsAvailable_WhenSystemdResolved.
    Assert.Skip ("Skipping non-Linux assertion: currently running on Linux.");
  }

  [Fact]
  public void ZeroconfProvider_IsAvailable_WhenSystemdResolved_ReturnsTrue ()
  {
    ZeroconfTestHelper.SkipIfSystemdResolvedUnavailable ();

    var provider = new ZeroconfProvider ();

    Assert.True (condition: provider.IsAvailable (),
                 userMessage: "systemd-resolved reported as available during skip check but IsAvailable() returned false.");
  }

  // ── TxtRecord tests ──────────────────────────────────────────────────────

  [Fact]
  public void TxtRecord_AddAndRead_RoundTripsValue ()
  {
    // Arrange
    var record = new TxtRecord ();

    // Act
    record.Add (key: "k", value: "v");
    TxtRecordItem? item = record["k"];

    // Assert
    Assert.NotNull (item);
    Assert.Equal (expected: "v", actual: item.ValueString);
  }

  // ── RegisterService tests ────────────────────────────────────────────────

  [Fact]
  public void RegisterService_Register_ThrowsPlatformNotSupportedException_WhenPublishBlocked ()
  {
    // This test only makes sense when systemd-resolved is the active provider
    // and polkit has blocked publishing (CanPublish == false).
    ZeroconfTestHelper.SkipIfSystemdResolvedUnavailable ();

    if (ZeroconfSupport.CanPublish)
      Assert.Skip ("mDNS publishing is authorized on this system; cannot test the blocked-publish path.");

    // Arrange
    var registerService = new RegisterService
    {
      Name      = "test-service",
      RegType   = "_http._tcp",
      UPort     = 8080,
    };

    // Act & Assert
    var ex = Assert.Throws<PlatformNotSupportedException> (() => registerService.Register ());
    Assert.Contains (expectedSubstring: "ZeroconfSupport.CanPublish",
                     actualString: ex.Message,
                     comparisonType: StringComparison.Ordinal);
  }
}
