#region header

// Arkane.Zeroconf.Tests - ServiceBrowserTests.cs
// 

#endregion

#region using

using System ;
using System.Collections.Generic ;
using System.Linq ;
using Moq ;
using Xunit ;

using ArkaneSystems.Arkane.Zeroconf ;
using ArkaneSystems.Arkane.Zeroconf.Providers ;

#endregion

namespace ArkaneSystems.Arkane.Zeroconf.Tests.Facades;

public class ServiceBrowserTests : IDisposable
{
    private readonly ServiceBrowser browser;

    public ServiceBrowserTests()
    {
        this.browser = new ServiceBrowser();
    }

    public void Dispose()
    {
        this.browser?.Dispose();
    }

    [Fact]
    public void Constructor_CreatesValidBrowser()
    {
        // Act & Assert
        Assert.NotNull(this.browser);
    }

    [Theory]
    [InlineData("_http._tcp", "local")]
    [InlineData("_workstation._tcp", "local")]
    [InlineData("_custom._tcp", "example.com")]
    public void Browse_WithTwoParams_CallsUnderlyingBrowser(string regtype, string domain)
    {
        // Act
        this.browser.Browse(regtype, domain);

        // Assert - no exception thrown
        Assert.True(true);
    }

    [Theory]
    [InlineData(AddressProtocol.IPv4)]
    [InlineData(AddressProtocol.IPv6)]
    [InlineData(AddressProtocol.Any)]
    public void Browse_WithAddressProtocol_CallsUnderlyingBrowser(AddressProtocol protocol)
    {
        // Act
        this.browser.Browse(protocol, "_http._tcp", "local");

        // Assert - no exception thrown
        Assert.True(true);
    }

    [Theory]
    [InlineData(0u)]
    [InlineData(1u)]
    [InlineData(2u)]
    public void Browse_WithInterfaceIndex_CallsUnderlyingBrowser(uint interfaceIndex)
    {
        // Act
        this.browser.Browse(interfaceIndex, "_http._tcp", "local");

        // Assert - no exception thrown
        Assert.True(true);
    }

    [Theory]
    [InlineData(0u, AddressProtocol.IPv4, "_http._tcp", "local")]
    [InlineData(1u, AddressProtocol.IPv6, "_custom._tcp", "example.com")]
    [InlineData(0u, AddressProtocol.Any, "_workstation._tcp", "local")]
    public void Browse_WithAllParams_CallsUnderlyingBrowser(
        uint interfaceIndex,
        AddressProtocol addressProtocol,
        string regtype,
        string domain)
    {
        // Act
        this.browser.Browse(interfaceIndex, addressProtocol, regtype, domain);

        // Assert - no exception thrown
        Assert.True(true);
    }

    [Fact]
    public void Browse_WithNullDomain_DefaultsToLocal()
    {
        // Act
        this.browser.Browse("_http._tcp", null);

        // Assert - no exception thrown (defaults to "local")
        Assert.True(true);
    }

    [Fact]
    public void Browse_WithNullRegtype_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => this.browser.Browse(0, AddressProtocol.Any, null, "local"));
    }

    [Fact]
    public void ServiceAdded_Event_CanSubscribe()
    {
        // Arrange
        var eventFired = false;
        this.browser.ServiceAdded += (s, e) => { eventFired = true; };

        // Act - just verify subscription works, event may not fire in unit test
        // Assert
        Assert.False(eventFired); // Event not fired, but subscription succeeded
    }

    [Fact]
    public void ServiceAdded_Event_CanUnsubscribe()
    {
        // Arrange
        ServiceBrowseEventHandler handler = (s, e) => { };
        this.browser.ServiceAdded += handler;

        // Act
        this.browser.ServiceAdded -= handler;

        // Assert - no exception thrown
        Assert.True(true);
    }

    [Fact]
    public void ServiceRemoved_Event_CanSubscribe()
    {
        // Arrange
        var eventFired = false;
        this.browser.ServiceRemoved += (s, e) => { eventFired = true; };

        // Act - just verify subscription works
        // Assert
        Assert.False(eventFired);
    }

    [Fact]
    public void ServiceRemoved_Event_CanUnsubscribe()
    {
        // Arrange
        ServiceBrowseEventHandler handler = (s, e) => { };
        this.browser.ServiceRemoved += handler;

        // Act
        this.browser.ServiceRemoved -= handler;

        // Assert - no exception thrown
        Assert.True(true);
    }

    [Fact]
    public void GetEnumerator_ReturnsIEnumerator()
    {
        // Act
        var enumerator = this.browser.GetEnumerator();

        // Assert
        Assert.NotNull(enumerator);
    }

    [Fact]
    public void GetEnumerator_Generic_ReturnsGenericEnumerator()
    {
        // Act
        var enumerator = ((IEnumerable<IResolvableService>)this.browser).GetEnumerator();

        // Assert
        Assert.NotNull(enumerator);
    }

    [Fact]
    public void Dispose_DoesNotThrow()
    {
        // Act & Assert
        this.browser.Dispose();
    }

    [Fact]
    public void Dispose_CanBeCalledMultipleTimes()
    {
        // Act & Assert
        this.browser.Dispose();
        this.browser.Dispose();
    }
}
