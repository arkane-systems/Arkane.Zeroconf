#region header

// Arkane.Zeroconf.Tests - AzClientE2ETests.cs

#endregion

#region using

using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

using ArkaneSystems.Arkane.Zeroconf.Tests.Helpers;

using Xunit;

#endregion

namespace ArkaneSystems.Arkane.Zeroconf.Tests.E2E;

/// <summary>
///   End-to-end tests using the azclient CLI tool.
///   These tests verify that the library works correctly through the CLI interface.
/// </summary>
public class AzClientE2ETests
{
  private const string TestDomain = "local";

  private string GetAzClientPath ()
  {
    string exeName  = RuntimeInformation.IsOSPlatform (OSPlatform.Windows) ? "azclient.exe" : "azclient";
    string basePath = Path.Combine (AppContext.BaseDirectory, "..", "..", "..", "..", "azclient", "bin", "Debug", "net10.0");
    string fullPath = Path.Combine (path1: basePath,          path2: exeName);

    return Path.GetFullPath (fullPath);
  }

  private async Task<(int ExitCode, string Output, string Error)> RunAzClientAsync (string arguments)
  {
    string azclientPath = this.GetAzClientPath ();

    if (!File.Exists (azclientPath))
      return (-1, string.Empty, $"azclient not found at {azclientPath}");

    var psi = new ProcessStartInfo
              {
                FileName               = azclientPath,
                Arguments              = arguments,
                UseShellExecute        = false,
                RedirectStandardOutput = true,
                RedirectStandardError  = true,
                CreateNoWindow         = true,
              };

    using Process? process = Process.Start (psi);

    if (process == null)
      return (-1, string.Empty, "Failed to start process");

    Task<string> outputTask = process.StandardOutput.ReadToEndAsync ();
    Task<string> errorTask  = process.StandardError.ReadToEndAsync ();

    bool completed = process.WaitForExit (TimeSpan.FromSeconds (10));

    if (!completed)
    {
      process.Kill (entireProcessTree: true);

      return (-1, string.Empty, "Process timeout");
    }

    string output = await outputTask;
    string error  = await errorTask;

    return (process.ExitCode, output, error);
  }

  private static RegisterService CreatePublishedService (string regType, string serviceName, short port)
    => new ()
       {
         Name = serviceName, RegType = regType, ReplyDomain = AzClientE2ETests.TestDomain, Port = port,
       };

  [Fact]
  public async Task AzClient_InvalidTimeout_Shows_Error ()
  {
    // Act
    var (exitCode, output, error) = await this.RunAzClientAsync ("--timeout -1");

    // Assert
    Assert.NotEqual (expected: 0, actual: exitCode);
    Assert.Contains (expectedSubstring: "invalid timeout", actualString: (output + error).ToLowerInvariant ());
  }

  [Fact]
  public async Task AzClient_InvalidAddressProtocol_Shows_Error ()
  {
    // Act
    var (exitCode, output, error) = await this.RunAzClientAsync ("--timeout 1 -a invalid");

    // Assert
    Assert.NotEqual (expected: 0, actual: exitCode);
    Assert.Contains (expectedSubstring: "invalid ip address protocol", actualString: (output + error).ToLowerInvariant ());
  }

  [Fact]
  public async Task AzClient_ResolveFlag_ResolvesPublishedService ()
  {
    ZeroconfTestHelper.SkipIfNoProvider ();
    ZeroconfTestHelper.SkipIfCannotPublish ();
    var regType     = "_arkanee2er._tcp";
    var serviceName = $"azclient-resolve-{Guid.NewGuid ():N}";

    using RegisterService service =
      AzClientE2ETests.CreatePublishedService (regType: regType, serviceName: serviceName, port: 28104);
    service.Register ();
    await Task.Delay (millisecondsDelay: 250, cancellationToken: TestContext.Current.CancellationToken);

    // Act
    var (exitCode, output, error) = await this.RunAzClientAsync ($"--timeout 8 --resolve -t {regType}");

    // Assert
    Assert.Equal (expected: 0, actual: exitCode);
    Assert.Contains (expectedSubstring: serviceName, actualString: output + error);
    Assert.Contains (expectedSubstring: "resolved name", actualString: (output + error).ToLowerInvariant ());
  }

  [Fact]
  public async Task AzClient_Publish_RegistersService ()
  {
    var regType     = "_arkanepublish._tcp";
    var serviceName = $"azclient-publish-{Guid.NewGuid ():N}";

    // Act
    var (exitCode, output, error) = await this.RunAzClientAsync ($"--timeout 1 --publish \"{regType} 28105 {serviceName}\"");

    // Assert
    if (ZeroconfSupport.CanPublish)
    {
      Assert.Equal (expected: 0, actual: exitCode);
      Assert.Contains (expectedSubstring: "registering name", actualString: (output + error).ToLowerInvariant ());
      Assert.Contains (expectedSubstring: serviceName, actualString: output + error);
    }
    else
    {
      Assert.Equal (expected: 2, actual: exitCode);
      Assert.Contains (expectedSubstring: "publishing is not supported", actualString: (output + error).ToLowerInvariant ());
    }
  }

  [Fact]
  public async Task AzClient_Publish_WithInvalidDescription_Fails ()
  {
    ZeroconfTestHelper.SkipIfNoProvider ();
    // Act
    var (exitCode, output, error) = await this.RunAzClientAsync ("--timeout 1 --publish \"invalid\"");

    // Assert
    Assert.NotEqual (expected: 0, actual: exitCode);
    Assert.Contains (expectedSubstring: "invalid service description syntax", actualString: (output + error).ToLowerInvariant ());
  }

  [Fact]
  public async Task AzClient_HelpFlag_Shows_Usage ()
  {
    // Act
    var (exitCode, output, error) = await this.RunAzClientAsync ("--help");

    // Assert
    Assert.NotEqual (expected: -1, actual: exitCode);
    Assert.Contains (expectedSubstring: "usage", actualString: (output + error).ToLowerInvariant ());
    Assert.Contains (expectedSubstring: "type",  actualString: (output + error).ToLowerInvariant ());
  }

  [Fact]
  public async Task AzClient_Browse_WithDefaultType_FindsServices ()
  {
    ZeroconfTestHelper.SkipIfNoProvider ();
    ZeroconfTestHelper.SkipIfCannotPublish ();
    var serviceName = $"azclient-default-{Guid.NewGuid ():N}";

    using RegisterService service =
      AzClientE2ETests.CreatePublishedService (regType: "_workstation._tcp", serviceName: serviceName, port: 28101);
    service.Register ();
    await Task.Delay (millisecondsDelay: 250, cancellationToken: TestContext.Current.CancellationToken);

    // Act
    var (exitCode, output, error) = await this.RunAzClientAsync ("--timeout 3");

    // Assert
    Assert.Equal (expected: 0, actual: exitCode);
    Assert.Contains (expectedSubstring: serviceName, actualString: output + error);
  }

  [Fact]
  public async Task AzClient_Browse_WithCustomType_Works ()
  {
    ZeroconfTestHelper.SkipIfNoProvider ();
    ZeroconfTestHelper.SkipIfCannotPublish ();
    var regType     = "_arkanee2e._tcp";
    var serviceName = $"azclient-custom-{Guid.NewGuid ():N}";

    using RegisterService service =
      AzClientE2ETests.CreatePublishedService (regType: regType, serviceName: serviceName, port: 28102);
    service.Register ();
    await Task.Delay (millisecondsDelay: 500, cancellationToken: TestContext.Current.CancellationToken);

    // Act
    var (exitCode, output, error) = await this.RunAzClientAsync ($"--timeout 3 -t {regType}");

    if (!(output + error).Contains (value: serviceName, comparisonType: StringComparison.Ordinal))
      (exitCode, output, error) = await this.RunAzClientAsync ($"--timeout 3 -t {regType}");

    // Assert
    Assert.Equal (expected: 0, actual: exitCode);
    Assert.Contains (expectedSubstring: serviceName, actualString: output + error);
  }

  [Fact]
  public async Task AzClient_InvalidArgument_Shows_Error ()
  {
    // Act
    var (exitCode, output, error) = await this.RunAzClientAsync ("--invalid-flag");

    // Assert
    Assert.NotEqual (expected: 0, actual: exitCode);
    Assert.Contains (expectedSubstring: "unknown option", actualString: (output + error).ToLowerInvariant ());
  }

  [Fact]
  public async Task AzClient_WithVerboseFlag_ProducesOutput ()
  {
    ZeroconfTestHelper.SkipIfNoProvider ();
    ZeroconfTestHelper.SkipIfCannotPublish ();
    var regType     = "_arkanee2ev._tcp";
    var serviceName = $"azclient-verbose-{Guid.NewGuid ():N}";

    using RegisterService service =
      AzClientE2ETests.CreatePublishedService (regType: regType, serviceName: serviceName, port: 28103);
    service.Register ();
    await Task.Delay (millisecondsDelay: 250, cancellationToken: TestContext.Current.CancellationToken);

    // Act
    var (exitCode, output, error) = await this.RunAzClientAsync ($"--timeout 3 -v -t {regType}");

    // Assert
    Assert.Equal (expected: 0, actual: exitCode);
    Assert.Contains (expectedSubstring: "creating a servicebrowser", actualString: (output + error).ToLowerInvariant ());
    Assert.Contains (expectedSubstring: serviceName,                 actualString: output + error);
  }

  [Fact]
  public async Task AzClient_WithIPv4Protocol_Works ()
  {
    ZeroconfTestHelper.SkipIfNoProvider ();
    // Act
    var (exitCode, output, error) = await this.RunAzClientAsync ("--timeout 2 -v -a ipv4 -t _workstation._tcp");

    // Assert
    Assert.Equal (expected: 0, actual: exitCode);
    Assert.Contains (expectedSubstring: "ipv4", actualString: (output + error).ToLowerInvariant ());
  }

  [Fact]
  public async Task AzClient_WithIPv6Protocol_Works ()
  {
    ZeroconfTestHelper.SkipIfNoProvider ();
    // Act
    var (exitCode, output, error) = await this.RunAzClientAsync ("--timeout 2 -v -a ipv6 -t _workstation._tcp");

    // Assert
    Assert.Equal (expected: 0, actual: exitCode);
    Assert.Contains (expectedSubstring: "ipv6", actualString: (output + error).ToLowerInvariant ());
  }
}
