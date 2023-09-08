﻿using Microsoft.DotNet.ScenarioTests.Common;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.DotNet.ScenarioTests.SdkTemplateTests;

public class SdkTemplateTests : IClassFixture<ScenarioTestFixture>
{
    ScenarioTestFixture _scenarioTestInput;
    ITestOutputHelper _testOutputHelper;
    DotNetSdkHelper _sdkHelper;

    public SdkTemplateTests(ScenarioTestFixture testInput, ITestOutputHelper outputHelper)
    {
        if (string.IsNullOrEmpty(testInput.DotNetRoot))
        {
            throw new ArgumentException("sdk root must be set for sdk tests");
        }

        _scenarioTestInput = testInput;
        _testOutputHelper = outputHelper;
        _sdkHelper = new DotNetSdkHelper(outputHelper, _scenarioTestInput.DotNetRoot, _scenarioTestInput.SdkVersion);
    }

    [Theory]
    [MemberData(nameof(GetLanguages))]
    public void VerifyConsoleTemplateComplex(DotNetLanguage language)
    {
        var newTest = new SdkTemplateTest(
            nameof(SdkTemplateTests) + "Complex", language, _scenarioTestInput.TargetRid, DotNetSdkTemplate.Console,
            DotNetSdkActions.Build | DotNetSdkActions.Run | DotNetSdkActions.PublishComplex | DotNetSdkActions.PublishR2R);
        newTest.Execute(_sdkHelper, _scenarioTestInput.TestRoot);
    }

    [Theory]
    [MemberData(nameof(GetLanguages))]
    [Trait("Category", "Offline")]
    public void VerifyConsoleTemplate(DotNetLanguage language)
    {
        var newTest = new SdkTemplateTest(
            nameof(SdkTemplateTests), language, _scenarioTestInput.TargetRid, DotNetSdkTemplate.Console,
            DotNetSdkActions.Build | DotNetSdkActions.Run | DotNetSdkActions.Publish);
        newTest.Execute(_sdkHelper, _scenarioTestInput.TestRoot);
    }

    [Theory]
    [MemberData(nameof(GetLanguages))]
    [Trait("Category", "Offline")]
    public void VerifyClasslibTemplate(DotNetLanguage language)
    {
        var newTest = new SdkTemplateTest(
            nameof(SdkTemplateTests), language, _scenarioTestInput.TargetRid, DotNetSdkTemplate.ClassLib,
            DotNetSdkActions.Build | DotNetSdkActions.Publish);
        newTest.Execute(_sdkHelper, _scenarioTestInput.TestRoot);
    }

    [Theory]
    [MemberData(nameof(GetLanguages))]
    public void VerifyXUnitTemplate(DotNetLanguage language)
    {
        var newTest = new SdkTemplateTest(
            nameof(SdkTemplateTests), language, _scenarioTestInput.TargetRid, DotNetSdkTemplate.XUnit,
            DotNetSdkActions.Test);
        newTest.Execute(_sdkHelper, _scenarioTestInput.TestRoot);
    }

    [Theory]
    [MemberData(nameof(GetLanguages))]
    public void VerifyNUnitTemplate(DotNetLanguage language)
    {
        var newTest = new SdkTemplateTest(
            nameof(SdkTemplateTests), language, _scenarioTestInput.TargetRid, DotNetSdkTemplate.NUnit,
            DotNetSdkActions.Test);
        newTest.Execute(_sdkHelper, _scenarioTestInput.TestRoot);
    }

    [Theory]
    [MemberData(nameof(GetLanguages))]
    public void VerifyMSTestTemplate(DotNetLanguage language)
    {
        var newTest = new SdkTemplateTest(
            nameof(SdkTemplateTests), language, _scenarioTestInput.TargetRid, DotNetSdkTemplate.MSTest,
            DotNetSdkActions.Test);
        newTest.Execute(_sdkHelper, _scenarioTestInput.TestRoot);
    }
    
    [Theory]
    [InlineData(DotNetLanguage.CSharp)]
    [InlineData(DotNetLanguage.VB)]
    [Trait("Category", "Offline")]
    [Trait("SkipIfPlatform", "LINUX")]
    [Trait("SkipIfPlatform", "OSX")]
    public void VerifyWPFTemplate(DotNetLanguage language)
    {
        var newTest = new SdkTemplateTest(
            nameof(SdkTemplateTest), language, _scenarioTestInput.TargetRid, DotNetSdkTemplate.Wpf,
            DotNetSdkActions.Test | DotNetSdkActions.Run | DotNetSdkActions.Publish);
        newTest.Execute(_sdkHelper, _scenarioTestInput.TestRoot);
    }
    
    [Fact]
    [Trait("Category", "Offline")]
    [Trait("SkipIfPlatform", "LINUX")]
    [Trait("SkipIfPlatform", "OSX")]
    public void VerifyWebAppTemplate()
    {
        var newTest = new SdkTemplateTest(
            nameof(SdkTemplateTest), DotNetLanguage.CSharp, _scenarioTestInput.TargetRid, DotNetSdkTemplate.WebApp,
            DotNetSdkActions.Test | DotNetSdkActions.Run | DotNetSdkActions.Publish);
        newTest.Execute(_sdkHelper, _scenarioTestInput.TestRoot);
    }
    
    [Theory]
    [InlineData(DotNetLanguage.CSharp)]
    [InlineData(DotNetLanguage.VB)]
    [Trait("Category", "Offline")]
    [Trait("SkipIfPlatform", "LINUX")]
    [Trait("SkipIfPlatform", "OSX")]
    public void VerifyWinformsTemplate(DotNetLanguage language)
    {
        var newTest = new SdkTemplateTest(
            nameof(SdkTemplateTest), language, _scenarioTestInput.TargetRid, DotNetSdkTemplate.Winforms,
            DotNetSdkActions.Test | DotNetSdkActions.Run | DotNetSdkActions.Publish);
        newTest.Execute(_sdkHelper, _scenarioTestInput.TestRoot);
    }
    
    [Theory]
    [InlineData(DotNetLanguage.CSharp)]
    [InlineData(DotNetLanguage.VB)]
    [Trait("Category", "Offline")]
    [Trait("SkipIfPlatform", "OSX")]
    public void VerifyReferenceInConsoleTemplate(DotNetLanguage language)
    {
        var referenceTest = new SdkTemplateTest(
            nameof(SdkTemplateTest), language, _scenarioTestInput.TargetRid, DotNetSdkTemplate.ClassLib,
            DotNetSdkActions.Build);
        referenceTest.Execute(_sdkHelper, _scenarioTestInput.TestRoot);
        var newTest = new SdkTemplateTest(
            nameof(SdkTemplateTest), language, _scenarioTestInput.TargetRid, DotNetSdkTemplate.Console,
            DotNetSdkActions.AddClassLibRef | DotNetSdkActions.Build | DotNetSdkActions.Test |
            DotNetSdkActions.Run | DotNetSdkActions.Publish);
        newTest.Execute(_sdkHelper, _scenarioTestInput.TestRoot);
    }
    
    [Theory]
    [Trait("Category", "MultiTFM")]
    [MemberData(nameof(GetLanguages))]
    public void VerifyMultiTFMInConsoleTemplate(DotNetLanguage language)
    {
        var newTest = new SdkTemplateTest(
            nameof(SdkTemplateTests) + "MultiTFM", language, _scenarioTestInput.TargetRid, DotNetSdkTemplate.Console,
            DotNetSdkActions.Build | DotNetSdkActions.Run | DotNetSdkActions.AddMultiTFM | DotNetSdkActions.Publish);
        newTest.Execute(_sdkHelper, _scenarioTestInput.TestRoot);
    }
    
    /*
     * v-masche note: Requires ASP.NET runtimes for .NET6 and .NET7. To be enabled if we decide to 
     * download that as part of the build like we do the normal .NET runtimes
    [Fact]
    [Trait("Category", "MultiTFM")]
    public void VerifyMultiTFMInWebAppTemplate()
    {
        var newTest = new SdkTemplateTest(
            nameof(SdkTemplateTest) + "MultiTFM", DotNetLanguage.CSharp, _scenarioTestInput.TargetRid, DotNetSdkTemplate.WebApp,
            DotNetSdkActions.Build | DotNetSdkActions.Run | DotNetSdkActions.AddMultiTFM | DotNetSdkActions.Publish);
        newTest.Execute(_sdkHelper, _scenarioTestInput.TestRoot);
    }*/
    
    private static IEnumerable<object[]> GetLanguages() => Enum.GetValues<DotNetLanguage>().Select(lang => new object[] { lang });

}
