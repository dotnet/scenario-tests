using Microsoft.DotNet.ScenarioTests.Common;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.DotNet.ScenarioTests.FileBasedApps;

public class FileBasedAppTests : IClassFixture<ScenarioTestFixture>
{
    private readonly ScenarioTestFixture _scenarioTestInput;
    private readonly FileBasedAppHelper _helper;

    public FileBasedAppTests(ScenarioTestFixture testInput, ITestOutputHelper outputHelper)
    {
        if (string.IsNullOrEmpty(testInput.DotNetRoot))
        {
            throw new ArgumentException("sdk root must be set for file-based app tests");
        }

        _scenarioTestInput = testInput;
        _helper = new FileBasedAppHelper(outputHelper, _scenarioTestInput.DotNetRoot, _scenarioTestInput.SdkVersion, _scenarioTestInput.BinlogDir);
    }

    [Fact]
    [Trait("Category", "Offline")]
    public void VerifySimpleFileBasedApp()
    {
        // Test simple HelloWorld.cs file execution
        string testDir = Path.Combine(_scenarioTestInput.TestRoot, nameof(FileBasedAppTests), "SimpleApp");
        Directory.CreateDirectory(testDir);

        try
        {
            // Create a simple C# file
            string csContent = @"using System;

Console.WriteLine(""Hello World from file-based app!"");
Console.WriteLine($""Current time: {DateTime.Now}"");";

            string csFile = _helper.CreateCsFile("HelloWorld.cs", csContent, testDir);

            // Execute the file using dotnet run
            string output = _helper.ExecuteRunFile(csFile, testDir);

            // Verify the output
            Assert.Contains("Hello World from file-based app!", output);
            Assert.Contains("Current time:", output);
        }
        finally
        {
            // Clean up
            if (Directory.Exists(testDir))
            {
                Directory.Delete(testDir, true);
            }
        }
    }

    [Fact]
    public void VerifyFileBasedAppWithPackageSearch()
    {
        // Test complex file-based app that searches for packages
        string testDir = Path.Combine(_scenarioTestInput.TestRoot, nameof(FileBasedAppTests), "PackageSearchApp");
        Directory.CreateDirectory(testDir);

        try
        {
            // Create a complex C# file that searches for NuGet packages
            string csContent = @"using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

// Complex file-based app that searches for NuGet packages and retrieves information
class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine(""File-based app: Package Search Demo"");
        
        string packageId = ""Microsoft.NETCore.App.Ref"";
        Console.WriteLine($""Searching for package: {packageId}"");
        
        try
        {
            using var httpClient = new HttpClient();
            
            // Search for the package using NuGet API
            string searchUrl = $""https://api.nuget.org/v3-flatcontainer/{packageId.ToLowerInvariant()}/index.json"";
            Console.WriteLine($""Querying: {searchUrl}"");
            
            string response = await httpClient.GetStringAsync(searchUrl);
            
            using JsonDocument doc = JsonDocument.Parse(response);
            JsonElement root = doc.RootElement;
            
            if (root.TryGetProperty(""versions"", out JsonElement versionsElement))
            {
                Console.WriteLine($""Found package: {packageId}"");
                Console.WriteLine(""Available versions:"");
                
                int count = 0;
                foreach (JsonElement version in versionsElement.EnumerateArray())
                {
                    if (count < 5) // Show first 5 versions
                    {
                        Console.WriteLine($""  - {version.GetString()}"");
                    }
                    count++;
                }
                
                if (count > 5)
                {
                    Console.WriteLine($""  ... and {count - 5} more versions"");
                }
                
                Console.WriteLine($""Total versions found: {count}"");
                
                // Get the latest version info
                if (versionsElement.GetArrayLength() > 0)
                {
                    var latestVersion = versionsElement[versionsElement.GetArrayLength() - 1].GetString();
                    Console.WriteLine($""Latest version: {latestVersion}"");
                    
                    // Get package metadata
                    string metadataUrl = $""https://api.nuget.org/v3-flatcontainer/{packageId.ToLowerInvariant()}/{latestVersion}/{packageId.ToLowerInvariant()}.nuspec"";
                    Console.WriteLine($""Package metadata URL: {metadataUrl}"");
                }
            }
            else
            {
                Console.WriteLine(""No versions found for this package."");
            }
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($""HTTP Error: {ex.Message}"");
        }
        catch (JsonException ex)
        {
            Console.WriteLine($""JSON parsing error: {ex.Message}"");
        }
        catch (Exception ex)
        {
            Console.WriteLine($""Error: {ex.Message}"");
        }
        
        Console.WriteLine(""Package search completed."");
    }
}";

            string csFile = _helper.CreateCsFile("PackageSearchApp.cs", csContent, testDir);

            // Execute the file using dotnet run (with longer timeout for network operations)
            string output = _helper.ExecuteRunFile(csFile, testDir, millisecondTimeout: 120000);

            // Verify the output contains expected content
            Assert.Contains("File-based app: Package Search Demo", output);
            Assert.Contains("Searching for package: Microsoft.NETCore.App.Ref", output);
            Assert.Contains("Package search completed.", output);
            
            // Check if the package search was successful (may fail in offline environments)
            if (output.Contains("Found package: Microsoft.NETCore.App.Ref"))
            {
                Assert.Contains("Available versions:", output);
                Assert.Contains("Total versions found:", output);
                Assert.Contains("Latest version:", output);
            }
            else
            {
                // If network is not available, at least verify that the app attempted to make the request
                Assert.Contains("Querying:", output);
            }
        }
        finally
        {
            // Clean up
            if (Directory.Exists(testDir))
            {
                Directory.Delete(testDir, true);
            }
        }
    }

    [Fact]
    [Trait("Category", "Offline")]
    public void VerifyFileBasedAppWithArguments()
    {
        // Test file-based app that accepts command line arguments
        string testDir = Path.Combine(_scenarioTestInput.TestRoot, nameof(FileBasedAppTests), "AppWithArgs");
        Directory.CreateDirectory(testDir);

        try
        {
            // Create a C# file that processes command line arguments
            string csContent = @"using System;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine(""File-based app with arguments"");
        Console.WriteLine($""Received {args.Length} arguments:"");
        
        for (int i = 0; i < args.Length; i++)
        {
            Console.WriteLine($""  arg[{i}]: {args[i]}"");
        }
        
        if (args.Length == 0)
        {
            Console.WriteLine(""No arguments provided"");
        }
    }
}";

            string csFile = _helper.CreateCsFile("AppWithArgs.cs", csContent, testDir);

            // Execute the file using dotnet run (without arguments)
            string output = _helper.ExecuteRunFile(csFile, testDir);

            // Verify the output
            Assert.Contains("File-based app with arguments", output);
            Assert.Contains("Received 0 arguments:", output);
            Assert.Contains("No arguments provided", output);
        }
        finally
        {
            // Clean up
            if (Directory.Exists(testDir))
            {
                Directory.Delete(testDir, true);
            }
        }
    }

    [Fact]
    [Trait("Category", "Offline")]
    public void VerifyFileBasedAppErrorHandling()
    {
        // Test file-based app that has compilation errors
        string testDir = Path.Combine(_scenarioTestInput.TestRoot, nameof(FileBasedAppTests), "AppWithErrors");
        Directory.CreateDirectory(testDir);

        try
        {
            // Create a C# file with syntax errors
            string csContent = @"using System;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine(""This will cause a compilation error"");
        // Missing semicolon to cause error
        var x = ""test""
    }
}";

            string csFile = _helper.CreateCsFile("ErrorApp.cs", csContent, testDir);

            // Execute the file using dotnet run - this should fail
            var exception = Assert.Throws<InvalidOperationException>(() =>
            {
                _helper.ExecuteRunFile(csFile, testDir);
            });

            // Verify that the error message indicates compilation failure
            Assert.Contains("Failed to execute", exception.Message);
        }
        finally
        {
            // Clean up
            if (Directory.Exists(testDir))
            {
                Directory.Delete(testDir, true);
            }
        }
    }

    [Fact]
    [Trait("Category", "Offline")]
    public void VerifyPreMadeFileBasedApps()
    {
        // Test using pre-made resource files
        string testDir = Path.Combine(_scenarioTestInput.TestRoot, nameof(FileBasedAppTests), "PreMadeApps");
        Directory.CreateDirectory(testDir);

        try
        {
            // Test HelloWorld.cs from resources
            string resourcesDir = Path.Combine(AppContext.BaseDirectory, "resources", "FileBasedApps");
            if (Directory.Exists(resourcesDir))
            {
                string helloWorldFile = Path.Combine(resourcesDir, "HelloWorld.cs");
                if (File.Exists(helloWorldFile))
                {
                    string output = _helper.ExecuteRunFile(helloWorldFile, testDir);
                    Assert.Contains("Hello World from file-based app!", output);
                    Assert.Contains("Current time:", output);
                }
            }
        }
        finally
        {
            // Clean up
            if (Directory.Exists(testDir))
            {
                Directory.Delete(testDir, true);
            }
        }
    }

    [Fact]
    public void VerifyFileBasedAppWithPackageDownload()
    {
        // Test complex file-based app that downloads and inspects packages
        string testDir = Path.Combine(_scenarioTestInput.TestRoot, nameof(FileBasedAppTests), "PackageDownloadApp");
        Directory.CreateDirectory(testDir);

        try
        {
            // Use the resource file we created
            string resourcesDir = Path.Combine(AppContext.BaseDirectory, "resources", "FileBasedApps");
            string packageDownloadFile = Path.Combine(resourcesDir, "PackageDownloadApp.cs");
            
            if (File.Exists(packageDownloadFile))
            {
                // Execute the file using dotnet run (with longer timeout for download operations)
                string output = _helper.ExecuteRunFile(packageDownloadFile, testDir, millisecondTimeout: 180000);

                // Verify the output contains expected content
                Assert.Contains("File-based app: Package Download and Inspection Demo", output);
                Assert.Contains("Downloading package: Microsoft.NETCore.App.Ref version 8.0.0", output);
                Assert.Contains("Package download demo completed.", output);
                
                // Check if the package download was successful (may fail in offline environments)
                if (output.Contains("Downloaded package size:"))
                {
                    Assert.Contains("Package contains", output);
                    Assert.Contains("files:", output);
                    Assert.Contains("Package inspection completed successfully.", output);
                }
                else
                {
                    // If network is not available, at least verify that the app attempted to make the request
                    Assert.Contains("Download URL:", output);
                }
            }
        }
        finally
        {
            // Clean up
            if (Directory.Exists(testDir))
            {
                Directory.Delete(testDir, true);
            }
        }
    }
}