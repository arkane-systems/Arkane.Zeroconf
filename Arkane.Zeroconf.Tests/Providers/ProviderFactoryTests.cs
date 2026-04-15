#region header

// Arkane.Zeroconf.Tests - ProviderFactoryTests.cs

#endregion

#region using

using System;

using ArkaneSystems.Arkane.Zeroconf.Providers;
using ArkaneSystems.Arkane.Zeroconf.Tests.Helpers;

using Xunit;

#endregion

namespace ArkaneSystems.Arkane.Zeroconf.Tests.Providers;

[Collection ("ProviderFactory selection")]
public class ProviderFactoryTests
{
  #region Nested type: InvalidProvider

  private sealed class InvalidProvider : IZeroconfProvider
  {
    public Type ServiceBrowser => typeof (object);

    public Type RegisterService => typeof (object);

    public Type TxtRecord => typeof (object);

    public ZeroconfCapability Capabilities => ZeroconfCapability.Browse | ZeroconfCapability.Publish;

    public bool IsAvailable () => true;

    public void Initialize () { }
  }

  #endregion

  [Fact]
  public void ServiceBrowser_Constructor_Failure_Message_IsSanitized ()
  {
    // Arrange
    ProviderFactory.SelectedProvider = new InvalidProvider ();

    try
    {
      // Act
      var exception = Assert.Throws<ZeroconfException> (() => _ = new ServiceBrowser ());

      // Assert
      Assert.DoesNotContain (expectedSubstring: "System.Object",
                             actualString: exception.Message,
                             comparisonType: StringComparison.Ordinal);
      Assert.Contains (expectedSubstring: "compatible Zeroconf provider",
                       actualString: exception.Message,
                       comparisonType: StringComparison.OrdinalIgnoreCase);
    }
    finally { ProviderFactory.ResetToDefaultProvider (); }
  }

  [Fact]
  public void RegisterService_Constructor_Failure_Message_IsSanitized ()
  {
    // Arrange
    ProviderFactory.SelectedProvider = new InvalidProvider ();

    try
    {
      // Act
      var exception = Assert.Throws<ZeroconfException> (() => _ = new RegisterService ());

      // Assert
      Assert.DoesNotContain (expectedSubstring: "System.Object",
                             actualString: exception.Message,
                             comparisonType: StringComparison.Ordinal);
      Assert.Contains (expectedSubstring: "compatible Zeroconf provider",
                       actualString: exception.Message,
                       comparisonType: StringComparison.OrdinalIgnoreCase);
    }
    finally { ProviderFactory.ResetToDefaultProvider (); }
  }

  [Fact]
  public void TxtRecord_Constructor_Failure_Message_IsSanitized ()
  {
    // Arrange
    ProviderFactory.SelectedProvider = new InvalidProvider ();

    try
    {
      // Act
      var exception = Assert.Throws<ZeroconfException> (() => _ = new TxtRecord ());

      // Assert
      Assert.DoesNotContain (expectedSubstring: "System.Object",
                             actualString: exception.Message,
                             comparisonType: StringComparison.Ordinal);
      Assert.Contains (expectedSubstring: "compatible Zeroconf provider",
                       actualString: exception.Message,
                       comparisonType: StringComparison.OrdinalIgnoreCase);
    }
    finally { ProviderFactory.ResetToDefaultProvider (); }
  }

  [Fact]
  public void SelectedProvider_DefaultsToAvailableProvider ()
  {
    ZeroconfTestHelper.SkipIfNoProvider ();

    IZeroconfProvider provider = ProviderFactory.SelectedProvider;

    Assert.NotNull (provider);
    Assert.True (condition: provider.IsAvailable ());
  }

  [Fact]
  public void SelectedProvider_HasValidServiceBrowserType ()
  {
    ZeroconfTestHelper.SkipIfNoProvider ();

    IZeroconfProvider provider = ProviderFactory.SelectedProvider;

    Assert.NotNull (provider.ServiceBrowser);
    Assert.True (condition: typeof (IServiceBrowser).IsAssignableFrom (provider.ServiceBrowser));
  }

  [Fact]
  public void PublicApis_UseProviderFactoryInternally ()
  {
    ZeroconfTestHelper.SkipIfNoProvider ();

    // Arrange
    using var browser   = new ServiceBrowser ();
    using var service   = new RegisterService ();
    using var txtRecord = new TxtRecord ();

    // Act & Assert - if these don't throw, providers are working
    Assert.NotNull (browser);
    Assert.NotNull (service);
    Assert.NotNull (txtRecord);
  }
}
