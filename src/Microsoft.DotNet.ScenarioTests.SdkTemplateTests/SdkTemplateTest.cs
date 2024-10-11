﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Microsoft.DotNet.ScenarioTests.SdkTemplateTests;

public class SdkTemplateTest
{
    public DotNetSdkActions Commands { get; }
    public DotNetLanguage Language { get; }
    public bool NoHttps { get => TargetRid.Contains("osx"); }
    public string TargetRid { get; set; }
    public string TargetArchitecture { get => TargetRid.Split('-')[1]; }
    public string ScenarioName { get; }
    public DotNetSdkTemplate Template { get; }

    public SdkTemplateTest(string scenarioName, DotNetLanguage language, string targetRid, DotNetSdkTemplate template, DotNetSdkActions commands = DotNetSdkActions.None)
    {
        ScenarioName = scenarioName;
        Template = template;
        Language = language;
        Commands = commands;
        TargetRid = targetRid;
    }

    internal void Execute(DotNetSdkHelper dotNetHelper, string testRoot, string[]? frameworks = null, string? PreMadeSolution = null)
    {
        // Don't use the cli language name in the project name because it may contain '#': https://github.com/dotnet/roslyn/issues/51692
        string projectName = $"{ScenarioName}_{Template}_{Language}";
        string customNewArgs = Template.IsAspNetCore() && NoHttps ? "--no-https" : string.Empty;
        string projectDirectory = Path.Combine(testRoot, projectName);

        if (PreMadeSolution == null)
        {
            Directory.CreateDirectory(projectDirectory);
            dotNetHelper.ExecuteNew(Template.GetName(), projectName, projectDirectory, Language.ToCliName(), customArgs: customNewArgs);
        }
        else
        {
            string PreMadeName;
            if (PreMadeSolution.Contains(Path.DirectorySeparatorChar))
            {
                PreMadeName = PreMadeSolution.Split(Path.DirectorySeparatorChar).Last();
            }
            else
            {
                PreMadeName = PreMadeSolution;
            }
            projectDirectory = Path.Combine(testRoot, PreMadeName);
            string fullPreMadePath = Path.Combine(AppContext.BaseDirectory, PreMadeSolution);
            dotNetHelper.CopyHelper(projectDirectory, fullPreMadePath, true);
        }

        if (frameworks != null)
        {
            dotNetHelper.ExecuteAddMultiTFM(projectName, projectDirectory, Language, frameworks);
        }

        if (Commands.HasFlag(DotNetSdkActions.AddClassLibRef))
        {
            dotNetHelper.ExecuteAddClassReference(projectDirectory);
        }
        if (Commands.HasFlag(DotNetSdkActions.Build))
        {
            dotNetHelper.ExecuteBuild(projectDirectory);
        }
        if (Commands.HasFlag(DotNetSdkActions.Run))
        {
            if (Template.IsAspNetCore())
            {
                dotNetHelper.ExecuteRunWeb(projectDirectory);
            }
            else if (Template.isUIApp())
            {
                dotNetHelper.ExecuteRunUIApp(projectDirectory, frameworks);
            }
            else
            {
                dotNetHelper.ExecuteRun(projectDirectory, frameworks);
            }
        }
        if (Commands.HasFlag(DotNetSdkActions.Publish))
        {
            dotNetHelper.ExecutePublish(projectDirectory, frameworks: frameworks);
        }
        if (Commands.HasFlag(DotNetSdkActions.PublishComplex))
        {
            dotNetHelper.ExecutePublish(projectDirectory, selfContained: false);        
            dotNetHelper.ExecutePublish(projectDirectory, TargetRid, selfContained: true);
            dotNetHelper.ExecutePublish(projectDirectory, $"linux-{TargetArchitecture}", selfContained: true);
        }
        if (Commands.HasFlag(DotNetSdkActions.PublishR2R))
        {
            dotNetHelper.ExecutePublish(projectDirectory, $"linux-{TargetArchitecture}", selfContained: true, trimmed: true, readyToRun: true);
        }
        if (Commands.HasFlag(DotNetSdkActions.PublishAot))
        {
            dotNetHelper.ExecutePublish(projectDirectory, TargetRid, aot: true);
        }
        if (Commands.HasFlag(DotNetSdkActions.Test))
        {
            dotNetHelper.ExecuteTest(projectDirectory);
        }
    }
}
