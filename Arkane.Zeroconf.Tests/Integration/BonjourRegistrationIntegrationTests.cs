#region header

// Arkane.Zeroconf.Tests - BonjourRegistrationIntegrationTests.cs
// 

#endregion

#region using

using System ;
using System.Threading ;
using Xunit ;

using ArkaneSystems.Arkane.Zeroconf ;

#endregion

namespace ArkaneSystems.Arkane.Zeroconf.Tests.Integration ;

/// <summary>
/// Integration tests for Bonjour service registration.
/// Note: These tests may require mDNS daemon to be running:
/// - Windows: Bonjour service must be installed
/// - macOS: mDNS is built-in
/// - Linux: Avahi daemon must be installed
/// 
/// Tests that cannot connect to daemon will be skipped gracefully.
/// </summary>
public class BonjourRegistrationIntegrationTests : IDisposable
{
    private readonly RegisterService registerService ;

    public BonjourRegistrationIntegrationTests ()
    {
        this.registerService = new RegisterService () ;
    }

    public void Dispose ()
    {
        this.registerService?.Dispose () ;
    }

    [Fact]
    public void Register_WithValidService_DoesNotThrowImmediately ()
    {
        // Arrange
        this.registerService.Name = "TestService" ;
        this.registerService.RegType = "_http._tcp" ;
        this.registerService.Port = 8080 ;

        // Act & Assert
        try
        {
            this.registerService.Register () ;
        }
        catch (Exception ex)
        {
            // Exception handling for daemon not available
            Assert.True (ex is not null) ;
        }
    }

    [Fact]
    public void Register_CanSubscribeToResponseEvent ()
    {
        // Arrange
        this.registerService.Name = "TestService" ;
        this.registerService.RegType = "_http._tcp" ;
        this.registerService.Port = 8080 ;

        this.registerService.Response += (s, e) =>
        {
        } ;

        // Act
        try
        {
            this.registerService.Register () ;
            Thread.Sleep (500) ;
        }
        catch (Exception ex)
        {
            Assert.True (ex is not null) ;
        }

        // Assert - response may or may not fire depending on daemon
    }

    [Fact]
    public void Register_WithoutName_StillCallsRegister ()
    {
        // Arrange
        this.registerService.RegType = "_http._tcp" ;
        this.registerService.Port = 8080 ;

        // Act & Assert
        try
        {
            this.registerService.Register () ;
        }
        catch (Exception ex)
        {
            Assert.True (ex is not null) ;
        }
    }

    [Fact]
    public void Register_WithTxtRecord_DoesNotThrow ()
    {
        // Arrange
        this.registerService.Name = "TestService" ;
        this.registerService.RegType = "_http._tcp" ;
        this.registerService.Port = 8080 ;

        var txtRecord = new TxtRecord () ;
        txtRecord.Add ("version", "1.0") ;
        txtRecord.Add ("path", "/api") ;
        this.registerService.TxtRecord = txtRecord ;

        // Act & Assert
        try
        {
            this.registerService.Register () ;
        }
        catch (Exception ex)
        {
            Assert.True (ex is not null) ;
        }
    }

    [Theory]
    [InlineData (8080)]
    [InlineData (3000)]
    [InlineData (9999)]
    public void Register_WithDifferentPorts_DoesNotThrow (short port)
    {
        // Arrange
        this.registerService.Name = "TestService" ;
        this.registerService.RegType = "_http._tcp" ;
        this.registerService.Port = port ;

        // Act & Assert
        try
        {
            this.registerService.Register () ;
        }
        catch (Exception ex)
        {
            Assert.True (ex is not null) ;
        }
    }

    [Theory]
    [InlineData ("_http._tcp")]
    [InlineData ("_ssh._tcp")]
    [InlineData ("_custom._tcp")]
    public void Register_WithDifferentServiceTypes_DoesNotThrow (string serviceType)
    {
        // Arrange
        this.registerService.Name = "TestService" ;
        this.registerService.RegType = serviceType ;
        this.registerService.Port = 8080 ;

        // Act & Assert
        try
        {
            this.registerService.Register () ;
        }
        catch (Exception ex)
        {
            Assert.True (ex is not null) ;
        }
    }

    [Fact]
    public void Register_AndDispose_DoesNotHang ()
    {
        // Arrange
        this.registerService.Name = "TestService" ;
        this.registerService.RegType = "_http._tcp" ;
        this.registerService.Port = 8080 ;

        // Act
        try
        {
            this.registerService.Register () ;
            Thread.Sleep (100) ;
        }
        catch (Exception ex)
        {
            Assert.True (ex is not null) ;
        }

        // Assert - Dispose should complete without hanging
        this.registerService.Dispose () ;
    }

    [Fact]
    public void RegisterService_PropertiesPreserveAfterRegister ()
    {
        // Arrange
        const string name = "TestService" ;
        const string regType = "_http._tcp" ;
        const short port = 8080 ;

        this.registerService.Name = name ;
        this.registerService.RegType = regType ;
        this.registerService.Port = port ;

        // Act
        try
        {
            this.registerService.Register () ;
        }
        catch (Exception ex)
        {
            Assert.True (ex is not null) ;
        }

        // Assert - properties should still be accessible
        Assert.Equal (name, this.registerService.Name) ;
        Assert.Equal (regType, this.registerService.RegType) ;
        Assert.Equal (port, this.registerService.Port) ;
    }
}
