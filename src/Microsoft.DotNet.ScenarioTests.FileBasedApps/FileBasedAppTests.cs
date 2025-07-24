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
    public void VerifyFileBasedAppsOffline()
    {
        // Comprehensive offline test that executes a wide swath of file-based app functionality
        string baseTestDir = Path.Combine(_scenarioTestInput.TestRoot, nameof(FileBasedAppTests));
        Directory.CreateDirectory(baseTestDir);

        try
        {
            // Test 1: Simple HelloWorld.cs file execution
            string simpleTestDir = Path.Combine(baseTestDir, "SimpleApp");
            Directory.CreateDirectory(simpleTestDir);
            
            string csContent = @"using System;

Console.WriteLine(""Hello World from file-based app!"");
Console.WriteLine($""Current time: {DateTime.Now}"");";

            string csFile = _helper.CreateCsFile("HelloWorld.cs", csContent, simpleTestDir);
            string output = _helper.ExecuteRunFile(csFile, simpleTestDir);
            
            Assert.Contains("Hello World from file-based app!", output);
            Assert.Contains("Current time:", output);

            // Test 2: File-based app with command line arguments
            string argsTestDir = Path.Combine(baseTestDir, "AppWithArgs");
            Directory.CreateDirectory(argsTestDir);
            
            string argsCsContent = @"using System;

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

            string argsCsFile = _helper.CreateCsFile("AppWithArgs.cs", argsCsContent, argsTestDir);
            string argsOutput = _helper.ExecuteRunFile(argsCsFile, argsTestDir);
            
            Assert.Contains("File-based app with arguments", argsOutput);
            Assert.Contains("Received 0 arguments:", argsOutput);
            Assert.Contains("No arguments provided", argsOutput);

            // Test 3: Error handling for compilation errors
            string errorTestDir = Path.Combine(baseTestDir, "AppWithErrors");
            Directory.CreateDirectory(errorTestDir);
            
            string errorCsContent = @"using System;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine(""This will cause a compilation error"");
        // Missing semicolon to cause error
        var x = ""test""
    }
}";

            string errorCsFile = _helper.CreateCsFile("ErrorApp.cs", errorCsContent, errorTestDir);
            
            var exception = Assert.Throws<InvalidOperationException>(() =>
            {
                _helper.ExecuteRunFile(errorCsFile, errorTestDir);
            });
            Assert.Contains("Failed to execute", exception.Message);

            // Test 4: Pre-made resource files
            string preMadeTestDir = Path.Combine(baseTestDir, "PreMadeApps");
            Directory.CreateDirectory(preMadeTestDir);
            
            string resourcesDir = Path.Combine(AppContext.BaseDirectory, "resources", "FileBasedApps");
            if (Directory.Exists(resourcesDir))
            {
                string helloWorldFile = Path.Combine(resourcesDir, "HelloWorld.cs");
                if (File.Exists(helloWorldFile))
                {
                    string preMadeOutput = _helper.ExecuteRunFile(helloWorldFile, preMadeTestDir);
                    Assert.Contains("Hello World from file-based app!", preMadeOutput);
                    Assert.Contains("Current time:", preMadeOutput);
                }
            }
        }
        finally
        {
            // Clean up
            if (Directory.Exists(baseTestDir))
            {
                Directory.Delete(baseTestDir, true);
            }
        }
    }

    [Fact]
    public void VerifyFileBasedAppsOnline()
    {
        // Comprehensive online test that executes a wide swath of file-based app functionality with network operations
        string baseTestDir = Path.Combine(_scenarioTestInput.TestRoot, nameof(FileBasedAppTests));
        Directory.CreateDirectory(baseTestDir);

        try
        {
            // Test 1: Package search functionality
            string packageSearchDir = Path.Combine(baseTestDir, "PackageSearchApp");
            Directory.CreateDirectory(packageSearchDir);
            
            string resourcesDir = Path.Combine(AppContext.BaseDirectory, "resources", "FileBasedApps");
            string packageSearchFile = Path.Combine(resourcesDir, "PackageSearchApp.cs");
            
            string searchOutput = _helper.ExecuteRunFile(packageSearchFile, packageSearchDir, millisecondTimeout: 120000);

            Assert.Contains("File-based app: Package Search Demo", searchOutput);
            Assert.Contains("Searching for package: Microsoft.NETCore.App.Ref", searchOutput);
            Assert.Contains("Package search completed.", searchOutput);
            
            // Check if the package search was successful (may fail in offline environments)
            if (searchOutput.Contains("Found package: Microsoft.NETCore.App.Ref"))
            {
                Assert.Contains("Available versions:", searchOutput);
                Assert.Contains("Total versions found:", searchOutput);
                Assert.Contains("Latest version:", searchOutput);
            }
            else
            {
                // If network is not available, at least verify that the app attempted to make the request
                Assert.Contains("Querying:", searchOutput);
            }

            // Test 2: Package download and inspection functionality
            string packageDownloadDir = Path.Combine(baseTestDir, "PackageDownloadApp");
            Directory.CreateDirectory(packageDownloadDir);
            
            string packageDownloadFile = Path.Combine(resourcesDir, "PackageDownloadApp.cs");
            
            if (File.Exists(packageDownloadFile))
            {
                string downloadOutput = _helper.ExecuteRunFile(packageDownloadFile, packageDownloadDir, millisecondTimeout: 180000);

                Assert.Contains("File-based app: Package Download and Inspection Demo", downloadOutput);
                Assert.Contains("Downloading package: Microsoft.NETCore.App.Ref version 8.0.0", downloadOutput);
                Assert.Contains("Package download demo completed.", downloadOutput);
                
                // Check if the package download was successful (may fail in offline environments)
                if (downloadOutput.Contains("Downloaded package size:"))
                {
                    Assert.Contains("Package contains", downloadOutput);
                    Assert.Contains("files:", downloadOutput);
                    Assert.Contains("Package inspection completed successfully.", downloadOutput);
                }
                else
                {
                    // If network is not available, at least verify that the app attempted to make the request
                    Assert.Contains("Download URL:", downloadOutput);
                }
            }
        }
        finally
        {
            // Clean up
            if (Directory.Exists(baseTestDir))
            {
                Directory.Delete(baseTestDir, true);
            }
        }
    }
}