using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.FeaturesTestFramework.Daemon;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.TestFramework;
using NUnit.Framework;

namespace JetBrains.ReSharper.Plugins.CyclomaticComplexity.Tests
{
  [TestSettingsKey(typeof(ComplexityAnalysisSettings))]
  public class TypeScriptComplexityHighlightingTest : TypeScriptHighlightingTestBase
  {
    protected override string RelativeTestDataPath { get { return "TypeScript"; } }

    protected override bool HighlightingPredicate(IHighlighting highlighting, IPsiSourceFile sourceFile)
    {
      return highlighting is IComplexityHighlighting;
    }

    [Test]
    public void TestClassMethodWithDefaultSettings()
    {
      DoOneTest("ClassMethodWithDefaultSettings");
    }

    [Test]
    [TestSettings("{ Threshold: [ 10, 21, 30] }")]
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
    [TestSettings("{ Threshold: [ 10, 21, 30] }")]
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
    [TestSettings("{ Threshold: [ 10, 21, 30] }")]
    public void TestAnonymousMethodWithNonDefaultSettings()
    {
      DoOneTest("AnonymousMethodWithModifiedSettings");
    }
  }
}