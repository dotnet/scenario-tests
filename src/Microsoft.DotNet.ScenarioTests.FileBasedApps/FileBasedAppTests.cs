using System;
using System.IO;
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
            
            // Validate output contains expected strings
            if (!output.Contains("Hello World from file-based app!"))
                throw new InvalidOperationException("Expected output not found: Hello World from file-based app!");
            if (!output.Contains("Current time:"))
                throw new InvalidOperationException("Expected output not found: Current time:");

            // Test 2: File-based app with command line arguments
            string argsTestDir = Path.Combine(baseTestDir, "AppWithArgs");
            Directory.CreateDirectory(argsTestDir);
            
            string resourcesDir = Path.Combine(AppContext.BaseDirectory, "FileBasedApps");
            string argsCsFile = Path.Combine(resourcesDir, "AppWithArgs.cs");
            string argsOutput = _helper.ExecuteRunFile(argsCsFile, argsTestDir);
            
            // Validate args output
            if (!argsOutput.Contains("File-based app with arguments"))
                throw new InvalidOperationException("Expected output not found: File-based app with arguments");
            if (!argsOutput.Contains("Received 0 arguments:"))
                throw new InvalidOperationException("Expected output not found: Received 0 arguments:");

            // Test 3: Error handling for compilation errors
            string errorTestDir = Path.Combine(baseTestDir, "AppWithErrors");
            Directory.CreateDirectory(errorTestDir);
            
            string errorAppFile = Path.Combine(resourcesDir, "AppWithErrors.cs");
            
            // This should throw an exception due to compilation error
            bool exceptionThrown = false;
            try
            {
                _helper.ExecuteRunFile(errorAppFile, errorTestDir);
            }
            catch (Exception)
            {
                exceptionThrown = true;
            }
            
            if (!exceptionThrown)
                throw new InvalidOperationException("Expected compilation error exception was not thrown");

            // Test 4: Pre-made resource files
            string preMadeTestDir = Path.Combine(baseTestDir, "PreMadeApps");
            Directory.CreateDirectory(preMadeTestDir);
            
            if (Directory.Exists(resourcesDir))
            {
                string helloWorldFile = Path.Combine(resourcesDir, "HelloWorld.cs");
                if (File.Exists(helloWorldFile))
                {
                    string preMadeOutput = _helper.ExecuteRunFile(helloWorldFile, preMadeTestDir);
                    if (!preMadeOutput.Contains("Hello World from file-based app!"))
                        throw new InvalidOperationException("Expected output not found in pre-made resource: Hello World from file-based app!");
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
            
            string resourcesDir = Path.Combine(AppContext.BaseDirectory, "FileBasedApps");
            string packageSearchFile = Path.Combine(resourcesDir, "PackageSearchApp.cs");
            
            string searchOutput = _helper.ExecuteRunFile(packageSearchFile, packageSearchDir, millisecondTimeout: 120000);

            // Validate basic search output
            if (!searchOutput.Contains("File-based app: Enhanced Package Search Demo"))
                throw new InvalidOperationException("Expected output not found: File-based app: Enhanced Package Search Demo");
            if (!searchOutput.Contains("Searching for packages matching:"))
                throw new InvalidOperationException("Expected output not found: Searching for packages matching:");
            if (!searchOutput.Contains("Package search and analysis completed."))
                throw new InvalidOperationException("Expected output not found: Package search and analysis completed.");
            
            // Check if the package search was successful (may fail in offline environments)
            if (searchOutput.Contains("Found ") && searchOutput.Contains(" packages"))
            {
                if (!searchOutput.Contains("Validating package content:"))
                    throw new InvalidOperationException("Expected output not found: Validating package content:");
                if (!searchOutput.Contains("Converting JSON data to XML"))
                    throw new InvalidOperationException("Expected output not found: Converting JSON data to XML");
                if (!searchOutput.Contains("Analysis completed"))
                    throw new InvalidOperationException("Expected output not found: Analysis completed");
            }
            else
            {
                // If network is not available, at least verify that the app attempted to execute dotnet package search
                if (!searchOutput.Contains("Failed to execute dotnet package search") && !searchOutput.Contains("Error:"))
                    throw new InvalidOperationException("Expected error message not found when network unavailable");
            }

            // Test 2: Package download and inspection functionality
            string packageDownloadDir = Path.Combine(baseTestDir, "PackageDownloadApp");
            Directory.CreateDirectory(packageDownloadDir);
            
            string packageDownloadFile = Path.Combine(resourcesDir, "PackageDownloadApp.cs");
            
            if (File.Exists(packageDownloadFile))
            {
                string downloadOutput = _helper.ExecuteRunFile(packageDownloadFile, packageDownloadDir, millisecondTimeout: 180000);

                // Validate basic download output
                if (!downloadOutput.Contains("File-based app: Package Download and Inspection Demo"))
                    throw new InvalidOperationException("Expected output not found: File-based app: Package Download and Inspection Demo");
                if (!downloadOutput.Contains("Downloading package: Microsoft.NETCore.App.Ref version 8.0.0"))
                    throw new InvalidOperationException("Expected output not found: Downloading package: Microsoft.NETCore.App.Ref version 8.0.0");
                if (!downloadOutput.Contains("Package download demo completed."))
                    throw new InvalidOperationException("Expected output not found: Package download demo completed.");
                
                // Check if the package download was successful (may fail in offline environments)
                if (downloadOutput.Contains("Downloaded package size:"))
                {
                    if (!downloadOutput.Contains("Package contains"))
                        throw new InvalidOperationException("Expected output not found: Package contains");
                    if (!downloadOutput.Contains("files:"))
                        throw new InvalidOperationException("Expected output not found: files:");
                    if (!downloadOutput.Contains("Package inspection completed successfully."))
                        throw new InvalidOperationException("Expected output not found: Package inspection completed successfully.");
                }
                else
                {
                    // If network is not available, at least verify that the app attempted to make the request
                    if (!downloadOutput.Contains("Download URL:"))
                        throw new InvalidOperationException("Expected output not found: Download URL:");
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