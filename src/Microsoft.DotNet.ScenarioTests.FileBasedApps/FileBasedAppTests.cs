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
        string resourcesDir = Path.Combine(AppContext.BaseDirectory, "FileBasedApps");

        try
        {
            // Test 1: Simple HelloWorld.cs file execution
            string simpleTestDir = Path.Combine(baseTestDir, "SimpleApp");
            Directory.CreateDirectory(simpleTestDir);
            
            string helloWorldResource = Path.Combine(resourcesDir, "HelloWorld.cs");
            
            string output = _helper.ExecuteRunFile(helloWorldResource, simpleTestDir);
            
            // Validate output contains expected strings
            if (!output.Contains("Hello World from file-based app!"))
                throw new InvalidOperationException("Expected output not found: Hello World from file-based app!");
            if (!output.Contains("Current time:"))
                throw new InvalidOperationException("Expected output not found: Current time:");

            // Test 2: File-based app with command line arguments
            string argsTestDir = Path.Combine(baseTestDir, "AppWithArgs");
            Directory.CreateDirectory(argsTestDir);
            
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
            // Test: Package demo functionality (search, analysis, and project creation)
            string packageDemoDir = Path.Combine(baseTestDir, "PackageDemoApp");
            Directory.CreateDirectory(packageDemoDir);
            
            string resourcesDir = Path.Combine(AppContext.BaseDirectory, "FileBasedApps");
            string packageDemoFile = Path.Combine(resourcesDir, "PackageDemoApp.cs");
            
            string demoOutput = _helper.ExecuteRunFile(packageDemoFile, packageDemoDir, millisecondTimeout: 300000); // 5 minutes for full demo

            // Validate basic demo output
            if (!demoOutput.Contains("File-based app: Package Demo with Project Creation"))
                throw new InvalidOperationException("Expected output not found: File-based app: Package Demo with Project Creation");
            if (!demoOutput.Contains("Searching for packages matching: Microsoft.Extensions"))
                throw new InvalidOperationException("Expected output not found: Searching for packages matching: Microsoft.Extensions");
            if (!demoOutput.Contains("Package demo completed."))
                throw new InvalidOperationException("Expected output not found: Package demo completed.");
            
            // Check if the package search was successful (may fail in offline environments)
            if (demoOutput.Contains("Found ") && demoOutput.Contains(" packages"))
            {
                if (!demoOutput.Contains("Validating package content:"))
                    throw new InvalidOperationException("Expected output not found: Validating package content:");
                if (!demoOutput.Contains("Converting JSON data to XML"))
                    throw new InvalidOperationException("Expected output not found: Converting JSON data to XML");
                if (!demoOutput.Contains("Analysis completed"))
                    throw new InvalidOperationException("Expected output not found: Analysis completed");
                if (!demoOutput.Contains("Creating console project and adding most downloaded package:"))
                    throw new InvalidOperationException("Expected output not found: Creating console project and adding most downloaded package:");
            }
            else
            {
                // If network is not available, at least verify that the app attempted to execute dotnet package search
                if (!demoOutput.Contains("Failed to execute dotnet package search") && !demoOutput.Contains("Error:"))
                    throw new InvalidOperationException("Expected error message not found when network unavailable");
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