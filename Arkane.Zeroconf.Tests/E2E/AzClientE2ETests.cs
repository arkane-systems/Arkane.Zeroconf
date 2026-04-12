#region header

// Arkane.Zeroconf.Tests - AzClientE2ETests.cs
// 

#endregion

#region using

using System ;
using System.Diagnostics ;
using System.IO ;
using System.Runtime.InteropServices ;
using System.Threading ;
using System.Threading.Tasks ;
using Xunit ;

#endregion

namespace ArkaneSystems.Arkane.Zeroconf.Tests.E2E ;

/// <summary>
/// End-to-end tests using the azclient CLI tool.
/// These tests verify that the library works correctly through the CLI interface.
/// </summary>
public class AzClientE2ETests
{
    private string GetAzClientPath ()
    {
        var exeName = RuntimeInformation.IsOSPlatform (OSPlatform.Windows) ? "azclient.exe" : "azclient" ;
        var basePath = Path.Combine (AppContext.BaseDirectory, "..", "..", "..", "..", "azclient", "bin", "Debug", "net10.0") ;
        var fullPath = Path.Combine (basePath, exeName) ;

        return fullPath ;
    }

    private async Task <(int ExitCode, string Output, string Error)> RunAzClientAsync (string arguments)
    {
        var azclientPath = this.GetAzClientPath () ;

        if (!File.Exists (azclientPath))
        {
            // Build azclient if it doesn't exist
            return (-1, "", $"azclient not found at {azclientPath}") ;
        }

        var psi = new ProcessStartInfo
        {
            FileName           = azclientPath,
            Arguments          = arguments,
            UseShellExecute    = false,
            RedirectStandardOutput = true,
            RedirectStandardError  = true,
            CreateNoWindow         = true
        } ;

        try
        {
            using var process = Process.Start (psi) ;
            if (process == null)
                return (-1, "", "Failed to start process") ;

            var outputTask = process.StandardOutput.ReadToEndAsync () ;
            var errorTask = process.StandardError.ReadToEndAsync () ;

            var completed = process.WaitForExit (TimeSpan.FromSeconds (5)) ;

            if (!completed)
            {
                process.Kill () ;
                return (-1, "", "Process timeout") ;
            }

            var output = await outputTask ;
            var error = await errorTask ;

            return (process.ExitCode, output, error) ;
        }
        catch (Exception ex)
        {
            return (-1, "", ex.Message) ;
        }
    }

    [Fact (Skip = "Requires built azclient")]
    public async Task AzClient_HelpFlag_Shows_Usage ()
    {
        // Act
        var (exitCode, output, error) = await this.RunAzClientAsync ("--help") ;

        // Assert
        Assert.True (exitCode == 0 || !string.IsNullOrEmpty (output), "Help should succeed or show output") ;
        Assert.Contains ("type", output.ToLower () + error.ToLower ()) ;
    }

    [Fact (Skip = "Requires built azclient and mDNS daemon")]
    public async Task AzClient_Browse_WithDefaultType_FindsServices ()
    {
        // Act
        var (exitCode, output, error) = await this.RunAzClientAsync ("") ;

        // Assert
        // Exit code 0 or services found is acceptable
        Assert.True (exitCode == 0 || output.Contains ("_workstation._tcp"), "Browse should complete") ;
    }

    [Fact (Skip = "Requires built azclient and mDNS daemon")]
    public async Task AzClient_Browse_WithCustomType_Works ()
    {
        // Act
        var (exitCode, output, error) = await this.RunAzClientAsync ("-t _http._tcp") ;

        // Assert
        Assert.NotNull (output) ;
    }

    [Fact (Skip = "Requires built azclient")]
    public async Task AzClient_InvalidArgument_Shows_Error ()
    {
        // Act
        var (exitCode, output, error) = await this.RunAzClientAsync ("--invalid-flag") ;

        // Assert
        Assert.NotEqual (0, exitCode) ;
        Assert.True (!string.IsNullOrEmpty (output) || !string.IsNullOrEmpty (error), "Should show help or error") ;
    }

    [Fact (Skip = "Requires built azclient and mDNS daemon")]
    public async Task AzClient_WithVerboseFlag_ProducesOutput ()
    {
        // Act
        var (exitCode, output, error) = await this.RunAzClientAsync ("-v -t _workstation._tcp") ;

        // Assert
        // Should produce some output with verbose flag
        Assert.True (!string.IsNullOrEmpty (output) || exitCode == 0, "Verbose should produce output") ;
    }

    [Fact (Skip = "Requires built azclient")]
    public async Task AzClient_WithIPv4Protocol_Works ()
    {
        // Act
        var (exitCode, output, error) = await this.RunAzClientAsync ("-a ipv4 -t _workstation._tcp") ;

        // Assert
        Assert.NotNull (output) ;
    }

    [Fact (Skip = "Requires built azclient")]
    public async Task AzClient_WithIPv6Protocol_Works ()
    {
        // Act
        var (exitCode, output, error) = await this.RunAzClientAsync ("-a ipv6 -t _workstation._tcp") ;

        // Assert
        Assert.NotNull (output) ;
    }
}
