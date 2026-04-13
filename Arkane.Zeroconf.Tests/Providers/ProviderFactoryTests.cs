#region header

// Arkane.Zeroconf.Tests - ProviderFactoryTests.cs

#endregion

#region using

using System;

using ArkaneSystems.Arkane.Zeroconf.Providers;

using Xunit;

#endregion

namespace ArkaneSystems.Arkane.Zeroconf.Tests.Providers;

public class ProviderFactoryTests
{
  [Fact]
  public void SelectedProvider_DefaultsToAvailableProvider ()
  {
    IZeroconfProvider provider = ProviderFactory.SelectedProvider;

    Assert.NotNull (provider);
    Assert.True (condition: provider.IsAvailable ());
  }

  [Fact]
  public void SelectedProvider_HasValidServiceBrowserType ()
  {
    IZeroconfProvider provider = ProviderFactory.SelectedProvider;

    Assert.NotNull (provider.ServiceBrowser);
    Assert.True (condition: typeof (IServiceBrowser).IsAssignableFrom (provider.ServiceBrowser));
  }

  [Fact]
  public void PublicApis_UseProviderFactoryInternally ()
  {
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
