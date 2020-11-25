using JetBrains.Application.Settings;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.FeaturesTestFramework.Daemon;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.TestFramework;
using NUnit.Framework;

namespace JetBrains.ReSharper.Plugins.CyclomaticComplexity.Tests
{
  [TestSettingsKey(typeof(CyclomaticComplexityAnalysisSettings))]
  public class TypeScriptComplexityHighlightingTest : TypeScriptHighlightingTestBase
  {
    protected override string RelativeTestDataPath => "TypeScript";

    protected override bool HighlightingPredicate(IHighlighting highlighting, IPsiSourceFile sourceFile,
      IContextBoundSettingsStore settingsStore)
    {
      return highlighting is IComplexityHighlighting;
    }

    [Test]
    public void TestClassMethodWithDefaultSettings()
    {
      DoOneTest("ClassMethodWithDefaultSettings");
    }

    [Test]
    [TestSettings("{ Thresholds: [ { 'TYPE_SCRIPT': 10 }, { 'TYPE_SCRIPT': 21 }, { 'TYPE_SCRIPT': 30 } ] }")]
    public void TestClassMethodWithNonDefaultSettings()
    {
      DoOneTest("ClassMethodWithModifiedSettings");
    }

    [Test]
    public void TestModuleMethodWithDefaultSettings()
    {
      DoOneTest("ModuleMethodWithDefaultSettings");
    }

    [Test]
    [TestSettings("{ Thresholds: [ { 'TYPE_SCRIPT': 10 }, { 'TYPE_SCRIPT': 21 }, { 'TYPE_SCRIPT': 30 } ] }")]
    public void TestModuleMethodWithNonDefaultSettings()
    {
      DoOneTest("ModuleMethodWithModifiedSettings");
    }

    [Test]
    public void TestAnonynousMethodWithDefaultSettings()
    {
      DoOneTest("AnonymousMethodWithDefaultSettings");
    }

    [Test]
    [TestSettings("{ Thresholds: [ { 'TYPE_SCRIPT': 10 }, { 'TYPE_SCRIPT': 21 }, { 'TYPE_SCRIPT': 30 } ] }")]
    public void TestAnonymousMethodWithNonDefaultSettings()
    {
      DoOneTest("AnonymousMethodWithModifiedSettings");
    }
  }
}