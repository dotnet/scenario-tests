using Microsoft.DotNet.ScenarioTests.Common;
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
    [Trait("Category", "Offline")]
    public void VerifyConsoleTemplate(DotNetLanguage language)
    {
        var newTest = new SdkTemplateTest(
            nameof(SdkTemplateTests), language, _scenarioTestInput.TargetRid, DotNetSdkTemplate.Console,
            DotNetSdkActions.Build | DotNetSdkActions.Run | DotNetSdkActions.PublishComplex | DotNetSdkActions.PublishR2R);
        newTest.Execute(_sdkHelper, _scenarioTestInput.TestRoot);
    }

    [Theory]
    [MemberData(nameof(GetLanguages))]
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

    private static IEnumerable<object[]> GetLanguages() => Enum.GetValues<DotNetLanguage>().Select(lang => new object[] { lang });
}
