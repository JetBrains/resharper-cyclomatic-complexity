using JetBrains.Application.Settings;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.FeaturesTestFramework.Daemon;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.TestFramework;
using NUnit.Framework;

namespace JetBrains.ReSharper.Plugins.CyclomaticComplexity.Tests
{
  [TestSettingsKey(typeof(ComplexityAnalysisSettings))]
  public class CSharpComplexityHighlightingTest : CSharpHighlightingTestNet4Base
  {
    protected override string RelativeTestDataPath { get { return "CSharp"; } }

    protected override bool HighlightingPredicate(IHighlighting highlighting, IPsiSourceFile sourceFile)
    {
      return highlighting is ComplexityWarning;
    }

    [Test]
    public void TestComplexMethodWithDefaultSettings()
    {
      DoOneTest("ComplexMethodWithDefaultSettings");
    }

    [Test]
    [TestSettings("{ Threshold: [ 10, 21, 30] }")]
    public void TestComplexMethodWithNonDefaultSettings()
    {
      DoOneTest("ComplexMethodWithModifiedSettings");
    }
  }
}