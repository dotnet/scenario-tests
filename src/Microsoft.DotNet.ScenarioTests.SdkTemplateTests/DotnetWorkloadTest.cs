// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Microsoft.DotNet.ScenarioTests.SdkTemplateTests;

internal class DotnetWorkloadTest
{
    public DotNetSdkActions Commands { get; }
    public DotNetLanguage Language { get; }
    public bool NoHttps { get => TargetRid.Contains("osx"); }
    public string TargetRid { get; set; }
    public string TargetArchitecture { get => TargetRid.Split('-')[1]; }
    public string ScenarioName { get; }

    public DotnetWorkloadTest(string scenarioName, string targetRid,  DotNetSdkActions commands = DotNetSdkActions.None)
    {
        ScenarioName = scenarioName;
        Commands = commands;
        TargetRid = targetRid;
    }

    internal void Execute(DotNetSdkHelper dotNetHelper, string testRoot)
    {
        string projectName = $"{ScenarioName}_Workload_{Commands.ToString()}";
        string projectDirectory = Path.Combine(testRoot, projectName);

        Directory.CreateDirectory(projectDirectory);

        if (Commands.HasFlag(DotNetSdkActions.Workload))
        {
            dotNetHelper.ExecuteWorkloadInstall(projectDirectory, "wasm-tools");
            dotNetHelper.ExecuteWorkloadList(projectDirectory, "wasm-tools", true);
            dotNetHelper.ExecuteWorkloadUninstall(projectDirectory, "wasm-tools");
            dotNetHelper.ExecuteWorkloadList(projectDirectory, "wasm-tools", false);
        }
    }
}
