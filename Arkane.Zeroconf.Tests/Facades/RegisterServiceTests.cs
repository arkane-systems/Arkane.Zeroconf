#region header

// Arkane.Zeroconf.Tests - RegisterServiceTests.cs

#endregion

#region using

using System;

using ArkaneSystems.Arkane.Zeroconf.Tests.Helpers;

using Xunit;

#endregion

namespace ArkaneSystems.Arkane.Zeroconf.Tests.Facades;

public class RegisterServiceTests : IDisposable
{
  public RegisterServiceTests ()
  {
    ZeroconfTestHelper.SkipIfNoProvider ();
    this.registerService = new RegisterService ();
  }

  private readonly RegisterService registerService;

  public void Dispose () { this.registerService?.Dispose (); }

  [Fact]
  public void Constructor_CreatesValidRegisterService ()
  {
    // Act & Assert
    Assert.NotNull (this.registerService);
  }

  [Fact]
  public void Name_Property_CanSetAndGet ()
  {
    // Arrange
    const string serviceName = "My Test Service";

    // Act
    this.registerService.Name = serviceName;
    string? result = this.registerService.Name;

    // Assert
    Assert.Equal (expected: serviceName, actual: result);
  }

  [Fact]
  public void RegType_Property_CanSetAndGet ()
  {
    // Arrange
    const string regType = "_http._tcp";

    // Act
    this.registerService.RegType = regType;
    string? result = this.registerService.RegType;

    // Assert
    Assert.Equal (expected: regType, actual: result);
  }

  [Fact]
  public void ReplyDomain_Property_CanSetAndGet ()
  {
    // Arrange
    const string domain = "local";

    // Act
    this.registerService.ReplyDomain = domain;
    string? result = this.registerService.ReplyDomain;

    // Assert
    Assert.Equal (expected: domain, actual: result);
  }

  [Fact]
  public void Port_Property_CanSetAndGet ()
  {
    // Arrange
    const short port = 8080;

    // Act
    this.registerService.Port = port;
    short result = this.registerService.Port;

    // Assert
    Assert.Equal (expected: port, actual: result);
  }

  [Fact]
  public void UPort_Property_CanSetAndGet ()
  {
    // Arrange
    const ushort port = 9000;

    // Act
    this.registerService.UPort = port;
    ushort result = this.registerService.UPort;

    // Assert
    Assert.Equal (expected: port, actual: result);
  }

  [Fact]
  public void TxtRecord_Property_CanSetAndGet ()
  {
    // Arrange
    var txtRecord = new TxtRecord ();

    // Act
    this.registerService.TxtRecord = txtRecord;
    ITxtRecord? result = this.registerService.TxtRecord;

    // Assert
    Assert.NotNull (result);
    Assert.Same (expected: txtRecord, actual: result);
  }

  [Fact]
  public void Register_DoesNotThrow ()
  {
    ZeroconfTestHelper.SkipIfCannotPublish ();

    // Arrange
    this.registerService.Name    = "Test Service";
    this.registerService.RegType = "_http._tcp";
    this.registerService.Port    = 8080;

    // Act & Assert
    this.registerService.Register ();
  }

  [Fact]
  public void Register_RespectsCapabilitySupport ()
  {
    // Arrange
    this.registerService.Name    = "Test Service";
    this.registerService.RegType = "_http._tcp";
    this.registerService.Port    = 8080;

    // Act & Assert
    if (ZeroconfSupport.CanPublish)
    {
      this.registerService.Register ();

      return;
    }

    Assert.Throws<PlatformNotSupportedException> (() => this.registerService.Register ());
  }

  [Fact]
  public void Response_Event_CanSubscribe ()
  {
    // Arrange
    var eventFired = false;
    this.registerService.Response += (s, e) => { eventFired = true; };

    // Act - just verify subscription works
    // Assert
    Assert.False (eventFired);
  }

  [Fact]
  public void Response_Event_CanUnsubscribe ()
  {
    // Arrange
    RegisterServiceEventHandler handler = (s, e) => { };
    this.registerService.Response += handler;

    // Act
    this.registerService.Response -= handler;

    // Assert - no exception thrown
    Assert.True (true);
  }

  [Fact]
  public void Dispose_DoesNotThrow ()
  {
    // Act & Assert
    this.registerService.Dispose ();
  }

  [Fact]
  public void Dispose_CanBeCalledMultipleTimes ()
  {
    // Act & Assert
    this.registerService.Dispose ();
    this.registerService.Dispose ();
  }

  [Fact]
  public void Properties_CanBeSetMultipleTimes ()
  {
    // Act
    this.registerService.Name = "Service 1";
    this.registerService.Name = "Service 2";
    this.registerService.Port = 8080;
    this.registerService.Port = 9090;

    // Assert
    Assert.Equal (expected: "Service 2", actual: this.registerService.Name);
    Assert.Equal (expected: 9090,        actual: this.registerService.Port);
  }
}
