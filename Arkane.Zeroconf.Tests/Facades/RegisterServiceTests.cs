#region header

// Arkane.Zeroconf.Tests - RegisterServiceTests.cs
// 

#endregion

#region using

using System ;
using Xunit ;

using ArkaneSystems.Arkane.Zeroconf ;

#endregion

namespace ArkaneSystems.Arkane.Zeroconf.Tests.Facades ;

public class RegisterServiceTests : IDisposable
{
    private readonly RegisterService registerService ;

    public RegisterServiceTests ()
    {
        this.registerService = new RegisterService () ;
    }

    public void Dispose ()
    {
        this.registerService?.Dispose () ;
    }

    [Fact]
    public void Constructor_CreatesValidRegisterService ()
    {
        // Act & Assert
        Assert.NotNull (this.registerService) ;
    }

    [Fact]
    public void Name_Property_CanSetAndGet ()
    {
        // Arrange
        const string serviceName = "My Test Service" ;

        // Act
        this.registerService.Name = serviceName ;
        var result = this.registerService.Name ;

        // Assert
        Assert.Equal (serviceName, result) ;
    }

    [Fact]
    public void RegType_Property_CanSetAndGet ()
    {
        // Arrange
        const string regType = "_http._tcp" ;

        // Act
        this.registerService.RegType = regType ;
        var result = this.registerService.RegType ;

        // Assert
        Assert.Equal (regType, result) ;
    }

    [Fact]
    public void ReplyDomain_Property_CanSetAndGet ()
    {
        // Arrange
        const string domain = "local" ;

        // Act
        this.registerService.ReplyDomain = domain ;
        var result = this.registerService.ReplyDomain ;

        // Assert
        Assert.Equal (domain, result) ;
    }

    [Fact]
    public void Port_Property_CanSetAndGet ()
    {
        // Arrange
        const short port = 8080 ;

        // Act
        this.registerService.Port = port ;
        var result = this.registerService.Port ;

        // Assert
        Assert.Equal (port, result) ;
    }

    [Fact]
    public void UPort_Property_CanSetAndGet ()
    {
        // Arrange
        const ushort port = 9000 ;

        // Act
        this.registerService.UPort = port ;
        var result = this.registerService.UPort ;

        // Assert
        Assert.Equal (port, result) ;
    }

    [Fact]
    public void TxtRecord_Property_CanSetAndGet ()
    {
        // Arrange
        var txtRecord = new TxtRecord () ;

        // Act
        this.registerService.TxtRecord = txtRecord ;
        var result = this.registerService.TxtRecord ;

        // Assert
        Assert.NotNull (result) ;
        Assert.Same (txtRecord, result) ;
    }

    [Fact]
    public void Register_DoesNotThrow ()
    {
        // Arrange
        this.registerService.Name = "Test Service" ;
        this.registerService.RegType = "_http._tcp" ;
        this.registerService.Port = 8080 ;

        // Act & Assert
        this.registerService.Register () ;
    }

    [Fact]
    public void Register_RespectsCapabilitySupport ()
    {
        // Arrange
        this.registerService.Name = "Test Service" ;
        this.registerService.RegType = "_http._tcp" ;
        this.registerService.Port = 8080 ;

        // Act & Assert
        if (ZeroconfSupport.CanPublish)
        {
            this.registerService.Register () ;
            return ;
        }

        Assert.Throws <PlatformNotSupportedException> (() => this.registerService.Register ()) ;
    }

    [Fact]
    public void Response_Event_CanSubscribe ()
    {
        // Arrange
        var eventFired = false ;
        this.registerService.Response += (s, e) => { eventFired = true ; } ;

        // Act - just verify subscription works
        // Assert
        Assert.False (eventFired) ;
    }

    [Fact]
    public void Response_Event_CanUnsubscribe ()
    {
        // Arrange
        RegisterServiceEventHandler handler = (s, e) => { } ;
        this.registerService.Response += handler ;

        // Act
        this.registerService.Response -= handler ;

        // Assert - no exception thrown
        Assert.True (true) ;
    }

    [Fact]
    public void Dispose_DoesNotThrow ()
    {
        // Act & Assert
        this.registerService.Dispose () ;
    }

    [Fact]
    public void Dispose_CanBeCalledMultipleTimes ()
    {
        // Act & Assert
        this.registerService.Dispose () ;
        this.registerService.Dispose () ;
    }

    [Fact]
    public void Properties_CanBeSetMultipleTimes ()
    {
        // Act
        this.registerService.Name = "Service 1" ;
        this.registerService.Name = "Service 2" ;
        this.registerService.Port = 8080 ;
        this.registerService.Port = 9090 ;

        // Assert
        Assert.Equal ("Service 2", this.registerService.Name) ;
        Assert.Equal (9090, this.registerService.Port) ;
    }
}
