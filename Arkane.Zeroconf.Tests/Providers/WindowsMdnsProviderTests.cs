#region header

// Arkane.Zeroconf.Tests - WindowsMdnsProviderTests.cs

#endregion

#region using

using System;

using ArkaneSystems.Arkane.Zeroconf.Providers.WindowsMdns;

using Xunit;

#endregion

namespace ArkaneSystems.Arkane.Zeroconf.Tests.Providers;

public class WindowsMdnsProviderTests
{
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
