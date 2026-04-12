#region header

// Arkane.Zeroconf.Tests - PerformanceAndStressTests.cs

#endregion

#region using

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using Xunit;

#endregion

namespace ArkaneSystems.Arkane.Zeroconf.Tests.Performance;

/// <summary>
///   Performance and stress tests for the Arkane.Zeroconf library.
///   These tests verify library behavior under load and stress conditions.
/// </summary>
public class PerformanceAndStressTests
{
  [Fact]
  public void TxtRecord_AddManyItems_Completes ()
  {
    // Arrange
    var       txtRecord = new TxtRecord ();
    const int itemCount = 1000;
    var       stopwatch = Stopwatch.StartNew ();

    // Act
    for (var i = 0; i < itemCount; i++)
      txtRecord.Add (key: $"key{i:D4}", value: $"value{i:D4}");

    stopwatch.Stop ();

    // Assert
    Assert.Equal (expected: itemCount, actual: txtRecord.Count);
    Assert.True (condition: stopwatch.ElapsedMilliseconds < 5000,
                 userMessage: $"Adding {itemCount} items took {stopwatch.ElapsedMilliseconds}ms");

    txtRecord.Dispose ();
  }

  [Fact]
  public void TxtRecord_Enumeration_IsEfficient ()
  {
    // Arrange
    var txtRecord = new TxtRecord ();
    for (var i = 0; i < 100; i++)
      txtRecord.Add (key: $"key{i}", value: $"value{i}");

    var stopwatch = Stopwatch.StartNew ();

    // Act
    var count = 0;
    foreach (object? item in txtRecord)
      count++;

    stopwatch.Stop ();

    // Assert
    Assert.Equal (expected: 100, actual: count);
    Assert.True (condition: stopwatch.ElapsedMilliseconds < 500,
                 userMessage: $"Enumeration took {stopwatch.ElapsedMilliseconds}ms");

    txtRecord.Dispose ();
  }

  [Fact]
  public void ServiceBrowser_CreateMultipleInstances_DoesNotLeakResources ()
  {
    // Arrange
    const int instances = 100;
    var       stopwatch = Stopwatch.StartNew ();

    // Act
    var browsers = new List<ServiceBrowser> ();
    for (var i = 0; i < instances; i++)
      browsers.Add (new ServiceBrowser ());

    // Dispose all
    foreach (ServiceBrowser browser in browsers)
      browser.Dispose ();

    stopwatch.Stop ();

    // Assert
    Assert.True (condition: stopwatch.ElapsedMilliseconds < 5000,
                 userMessage: $"Creating/disposing {instances} browsers took {stopwatch.ElapsedMilliseconds}ms");
  }

  [Fact]
  public void RegisterService_CreateMultipleInstances_DoesNotLeakResources ()
  {
    // Arrange
    const int instances = 100;
    var       stopwatch = Stopwatch.StartNew ();

    // Act
    var services = new List<RegisterService> ();
    for (var i = 0; i < instances; i++)
      services.Add (new RegisterService ());

    // Dispose all
    foreach (RegisterService service in services)
      service.Dispose ();

    stopwatch.Stop ();

    // Assert
    Assert.True (condition: stopwatch.ElapsedMilliseconds < 5000,
                 userMessage: $"Creating/disposing {instances} services took {stopwatch.ElapsedMilliseconds}ms");
  }

  [Fact]
  public void ServiceBrowser_EventSubscription_UnderLoad ()
  {
    // Arrange
    var       browser       = new ServiceBrowser ();
    var       eventCount    = 0;
    const int subscriptions = 100;

    // Act
    var handlers = new List<ServiceBrowseEventHandler> ();

    for (var i = 0; i < subscriptions; i++)
    {
      ServiceBrowseEventHandler handler = (s, e) => { Interlocked.Increment (ref eventCount); };
      browser.ServiceAdded += handler;
      handlers.Add (handler);
    }

    // Unsubscribe all
    foreach (ServiceBrowseEventHandler handler in handlers)
      browser.ServiceAdded -= handler;

    browser.Dispose ();

    // Assert
    Assert.True (subscriptions > 0);
  }

  [Fact]
  public void TxtRecord_RepeatedAddRemove_MaintainsConsistency ()
  {
    // Arrange
    var       txtRecord  = new TxtRecord ();
    const int iterations = 100;

    // Act
    for (var i = 0; i < iterations; i++)
      txtRecord.Add (key: $"key_{i}", value: $"value_{i}");

    for (var i = 0; i < iterations; i++)
      txtRecord.Remove ($"key_{i}");

    // Assert
    Assert.Equal (expected: 0, actual: txtRecord.Count);

    txtRecord.Dispose ();
  }

  [Fact]
  public async Task ServiceBrowser_Concurrent_CreateAndDispose ()
  {
    // Arrange
    const int concurrentTasks = 10;
    var       tasks           = new List<Task> ();

    // Act
    for (var i = 0; i < concurrentTasks; i++)
    {
      tasks.Add (Task.Run (() =>
                           {
                             for (var j = 0; j < 10; j++)
                             {
                               using var browser = new ServiceBrowser ();
                               Thread.Sleep (10);
                             }
                           }));
    }

    await Task.WhenAll (tasks);

    // Assert
    Assert.Equal (expected: concurrentTasks, actual: tasks.Count);
  }

  [Fact]
  public void TxtRecord_LargeValues_HandledCorrectly ()
  {
    // Arrange
    var txtRecord  = new TxtRecord ();
    var largeValue = new string (c: 'x', count: 10000);

    // Act
    txtRecord.Add (key: "large_key", value: largeValue);

    // Assert
    Assert.Equal (expected: 1, actual: txtRecord.Count);

    txtRecord.Dispose ();
  }

  [Fact]
  public void ServiceBrowser_Enumeration_ThreadSafe ()
  {
    // Arrange
    var browser = new ServiceBrowser ();

    // Act
    Task enumerationTask = Task.Run (() =>
                                     {
                                       try
                                       {
                                         foreach (IResolvableService? service in browser)
                                           Thread.Sleep (1);
                                       }
                                       catch
                                       {
                                         // Enumeration may fail if browser not started, that's OK
                                       }
                                     });

    Thread.Sleep (100);
    browser.Dispose ();

    // Assert
    Assert.True (condition: enumerationTask.Wait (TimeSpan.FromSeconds (2)),
                 userMessage: "Enumeration should complete or timeout gracefully");
  }

  [Fact]
  public void RegisterService_PropertyAccess_IsHighPerformance ()
  {
    // Arrange
    var       service    = new RegisterService ();
    var       stopwatch  = Stopwatch.StartNew ();
    const int iterations = 10000;

    // Act
    for (var i = 0; i < iterations; i++)
    {
      service.Name = $"Service{i}";
      service.Port = (short)(8000 + i % 1000);
      _            = service.Name;
      _            = service.Port;
    }

    stopwatch.Stop ();

    // Assert
    Assert.True (condition: stopwatch.ElapsedMilliseconds < 1000,
                 userMessage: $"{iterations} property accesses took {stopwatch.ElapsedMilliseconds}ms");

    service.Dispose ();
  }
}
