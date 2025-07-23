// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.DotNet.ScenarioTests.Common;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Xunit.Abstractions;

namespace Microsoft.DotNet.ScenarioTests.FileBasedApps;

internal class FileBasedAppHelper
{
    private readonly string? _binlogDir;
    private readonly ITestOutputHelper _outputHelper;

    public string DotNetRoot { get; }

    public string? SdkVersion { get; }

    public string DotNetExecutablePath =>
        RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? Path.Combine(DotNetRoot, "dotnet.exe") : Path.Combine(DotNetRoot, "dotnet");

    public FileBasedAppHelper(ITestOutputHelper outputHelper, string dotnetRoot, string? sdkVersion, string? binlogDir)
    {
        _outputHelper = outputHelper;
        DotNetRoot = dotnetRoot;
        SdkVersion = sdkVersion;
        _binlogDir = binlogDir;
    }

    /// <summary>
    /// Execute a .cs file directly using dotnet run
    /// </summary>
    public string ExecuteRunFile(string csFilePath, string workingDirectory, int millisecondTimeout = 60000)
    {
        if (!File.Exists(csFilePath))
        {
            throw new FileNotFoundException($"C# file not found: {csFilePath}");
        }

        // Use the absolute path to avoid any path resolution issues
        string absoluteCsFilePath = Path.GetFullPath(csFilePath);
        string args = $"run \"{absoluteCsFilePath}\"";
        
        _outputHelper.WriteLine($"Executing file-based app: {absoluteCsFilePath}");
        
        (Process Process, string StdOut, string StdErr) executeResult = ExecuteHelper.ExecuteProcess(
            DotNetExecutablePath,
            args,
            _outputHelper,
            configure: (process) => ConfigureProcess(process, workingDirectory),
            millisecondTimeout: millisecondTimeout);

        ExecuteHelper.ValidateExitCode(executeResult);

        return executeResult.StdOut;
    }

    /// <summary>
    /// Create a C# file with specified content for testing
    /// </summary>
    public string CreateCsFile(string fileName, string content, string directory)
    {
        string filePath = Path.Combine(directory, fileName);
        Directory.CreateDirectory(directory);
        File.WriteAllText(filePath, content);
        _outputHelper.WriteLine($"Created C# file: {filePath}");
        return filePath;
    }

    private void ConfigureProcess(Process process, string workingDirectory)
    {
        process.StartInfo.WorkingDirectory = workingDirectory;

        // Clear dotnet and MSBuild environment variables to avoid side effects
        foreach (string key in process.StartInfo.Environment.Keys.Where(key => key.StartsWith("DOTNET_", StringComparison.OrdinalIgnoreCase) || key.StartsWith("MSBUILD", StringComparison.OrdinalIgnoreCase)).ToArray())
        {
            process.StartInfo.Environment.Remove(key);
        }

        process.StartInfo.EnvironmentVariables["DOTNET_CLI_TELEMETRY_OPTOUT"] = "1";
        process.StartInfo.EnvironmentVariables["DOTNET_SKIP_FIRST_TIME_EXPERIENCE"] = "1";
        process.StartInfo.EnvironmentVariables["DOTNET_ROOT"] = DotNetRoot;
        process.StartInfo.EnvironmentVariables["DOTNET_ROLL_FORWARD"] = "Major";
        
        // Don't use the repo infrastructure
        process.StartInfo.EnvironmentVariables["ImportDirectoryBuildProps"] = "false";
        process.StartInfo.EnvironmentVariables["ImportDirectoryBuildTargets"] = "false";
        process.StartInfo.EnvironmentVariables["ImportDirectoryPackagesProps"] = "false";

        // Create global.json if SDK version is specified
        if (!string.IsNullOrEmpty(SdkVersion) && !File.Exists(Path.Combine(workingDirectory, "global.json")))
        {
            ExecuteCmd($"new globaljson --sdk-version {SdkVersion}", workingDirectory);
        }
    }

    private void ExecuteCmd(string args, string workingDirectory, int expectedExitCode = 0)
    {
        (Process Process, string StdOut, string StdErr) executeResult = ExecuteHelper.ExecuteProcess(
            DotNetExecutablePath,
            args,
            _outputHelper,
            configure: (process) => ConfigureProcess(process, workingDirectory));

        ExecuteHelper.ValidateExitCode(executeResult, expectedExitCode);
    }
}