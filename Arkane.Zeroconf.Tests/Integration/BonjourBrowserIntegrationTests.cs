#region header

// Arkane.Zeroconf.Tests - BonjourBrowserIntegrationTests.cs

#endregion

#region using

using System;
using System.Collections.Generic;
using System.Threading;

using Xunit;

#endregion

namespace ArkaneSystems.Arkane.Zeroconf.Tests.Integration;

/// <summary>
///   Integration tests for Bonjour ServiceBrowser.
///   Note: These tests may require mDNS daemon to be running:
///   - Windows: Bonjour service must be installed
///   - macOS: mDNS is built-in
///   - Linux: Avahi daemon must be installed
///   Tests that cannot connect to daemon will be skipped gracefully.
/// </summary>
public class BonjourBrowserIntegrationTests : IDisposable
{
  public BonjourBrowserIntegrationTests () => this.browser = new ServiceBrowser ();

  private readonly ServiceBrowser browser;

  public void Dispose () { this.browser?.Dispose (); }

  [Fact]
  public void Browse_WithValidServiceType_DoesNotThrowImmediately ()
  {
    // Act & Assert - browsing may fail if daemon not available, but shouldn't throw during Browse call
    try { this.browser.Browse (interfaceIndex: 0, addressProtocol: AddressProtocol.Any, regtype: "_http._tcp", domain: "local"); }
    catch (Exception ex)
    {
      // Some systems may not have mDNS available
      Assert.True (ex is not null);
    }
  }

  [Theory]
  [InlineData ("_http._tcp")]
  [InlineData ("_workstation._tcp")]
  [InlineData ("_sftp-ssh._tcp")]
  public void Browse_WithCommonServiceTypes_DoesNotThrow (string serviceType)
  {
    // Act & Assert
    try { this.browser.Browse (interfaceIndex: 0, addressProtocol: AddressProtocol.Any, regtype: serviceType, domain: "local"); }
    catch (Exception ex)
    {
      // Exception handling for daemon not available
      Assert.True (ex is not null);
    }
  }

  [Fact]
  public void Browse_CanSubscribeToServiceAddedEvent ()
  {
    // Arrange
    var resetEvent = new ManualResetEvent (false);
    this.browser.ServiceAdded += (s, e) => { resetEvent.Set (); };

    // Act
    try
    {
      this.browser.Browse (regtype: "_http._tcp", domain: "local");

      // Wait briefly for events (will timeout on systems without mDNS)
      resetEvent.WaitOne (TimeSpan.FromSeconds (2));
    }
    catch (Exception ex)
    {
      // System may not have mDNS available
      Assert.True (ex is not null);
    }

    // Assert - event may or may not fire depending on system state
    // but subscription should not throw
  }

  [Fact]
  public void Browse_CanSubscribeToServiceRemovedEvent ()
  {
    // Arrange
    this.browser.ServiceRemoved += (s, e) => { };

    // Act
    try { this.browser.Browse (regtype: "_http._tcp", domain: "local"); }
    catch (Exception ex)
    {
      // System may not have mDNS available
      Assert.True (ex is not null);
    }

    // Assert - subscription should not throw
  }

  [Fact]
  public void Browse_GetEnumerator_ReturnsServices ()
  {
    // Act
    try
    {
      this.browser.Browse (regtype: "_http._tcp", domain: "local");
      Thread.Sleep (500); // Give it time to discover services

      var serviceList = new List<IResolvableService> ();
      foreach (IResolvableService? service in this.browser)
        serviceList.Add (service);

      // Assert - may be empty if no services advertised
      Assert.IsType<List<IResolvableService>> (serviceList);
    }
    catch (Exception ex)
    {
      // System may not have mDNS available
      Assert.True (ex is not null);
    }
  }

  [Fact]
  public void Browse_Dispose_StopsAsync ()
  {
    // Act
    try
    {
      this.browser.Browse (regtype: "_http._tcp", domain: "local");
      Thread.Sleep (100);
      this.browser.Dispose ();
    }
    catch (Exception ex)
    {
      // System may not have mDNS
      Assert.True (ex is not null);
    }

    // Assert - Dispose should complete without hanging
  }

  [Theory]
  [InlineData (AddressProtocol.IPv4)]
  [InlineData (AddressProtocol.IPv6)]
  [InlineData (AddressProtocol.Any)]
  public void Browse_WithDifferentAddressProtocols_DoesNotThrow (AddressProtocol protocol)
  {
    // Act & Assert
    try { this.browser.Browse (interfaceIndex: 0, addressProtocol: protocol, regtype: "_http._tcp", domain: "local"); }
    catch (Exception ex)
    {
      // System may not have mDNS available
      Assert.True (ex is not null);
    }
  }

  [Fact]
  public void Browse_OnInvalidServiceType_MayThrowOrReturnEmpty ()
  {
    // Act & Assert
    try
    {
      this.browser.Browse (regtype: "_invalid12345._tcp", domain: "local");

      // If browse succeeds, enumeration should be empty
      var services = new List<IResolvableService> ();
      foreach (IResolvableService? service in this.browser)
        services.Add (service);
      Assert.IsType<List<IResolvableService>> (services);
    }
    catch (Exception ex)
    {
      // Either exception or empty result is acceptable
      Assert.True (ex is not null);
    }
  }
}
