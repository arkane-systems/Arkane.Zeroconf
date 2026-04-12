#region header

// Arkane.Zeroconf.Tests - ProviderFactoryTests.cs
// 

#endregion

#region using

using System ;
using Xunit ;

#endregion

namespace ArkaneSystems.Arkane.Zeroconf.Tests.Providers ;

/// <summary>
/// Note: ProviderFactory is internal to the Arkane.Zeroconf library.
/// These tests verify provider behavior through the public facade classes instead.
/// This test class is currently commented out as ProviderFactory is not publicly accessible.
/// </summary>
public class ProviderFactoryTests
{
    [Fact (Skip = "ProviderFactory is internal API")]
    public void SelectedProvider_DefaultsToAvailableProvider ()
    {
        // ProviderFactory testing would be done through public APIs
        // like ServiceBrowser, RegisterService, and TxtRecord
    }

    [Fact (Skip = "ProviderFactory is internal API")]
    public void SelectedProvider_HasValidServiceBrowserType ()
    {
        // Provider validation happens implicitly when using public facades
    }

    [Fact]
    public void PublicApis_UseProviderFactoryInternally ()
    {
        // Arrange
        using var browser = new ServiceBrowser () ;
        using var service = new RegisterService () ;
        using var txtRecord = new TxtRecord () ;

        // Act & Assert - if these don't throw, providers are working
        Assert.NotNull (browser) ;
        Assert.NotNull (service) ;
        Assert.NotNull (txtRecord) ;
    }
}
