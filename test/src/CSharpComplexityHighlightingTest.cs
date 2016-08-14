/*
 * Copyright 2007-2015 JetBrains
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using JetBrains.Application.Settings;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.FeaturesTestFramework.Daemon;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.TestFramework;
using NUnit.Framework;

namespace JetBrains.ReSharper.Plugins.CyclomaticComplexity.Tests
{
  [TestSettingsKey(typeof(CyclomaticComplexityAnalysisSettings))]
  public class CSharpComplexityHighlightingTest : CSharpHighlightingTestBase
  {
    protected override string RelativeTestDataPath => "CSharp";

    protected override bool HighlightingPredicate(IHighlighting highlighting, IPsiSourceFile sourceFile)
    {
      return highlighting is IComplexityHighlighting;
    }

    protected override void DoNamedTest2(params string[] auxFiles)
    {
      ExecuteWithinSettingsTransaction(store =>
      {
        // This will be done in the plugin by adding a .dotSettings file
        RunGuarded(() => store.SetIndexedValue(HighlightingSettingsAccessor.InspectionSeverities, "FunctionComplexityOverflow", Severity.WARNING));
        base.DoNamedTest2(auxFiles);
      });
    }

    [Test]
    public void TestComplexMethodWithDefaultSettings()
    {
      DoOneTest("ComplexMethodWithDefaultSettings");
    }

    [Test]
    [TestSettings("{ Thresholds: [ { 'CSHARP': 10 }, { 'CSHARP': 21 }, { 'CSHARP': 30 } ] }")]
    public void TestComplexMethodWithNonDefaultSettings()
    {
      DoOneTest("ComplexMethodWithModifiedSettings");
    }

    [Test]
    public void TestMethodBodyTooComplexForValueAnalysisAndHighCyclomaticComplexity()
    {
      DoNamedTest2();
    }

    [Test]
    public void TestMethodBodyTooComplexForValueAnalysisButLowCyclomaticComplexity()
    {
      DoNamedTest2();
    }

    [Test]
    public void TestWorkaroundBoolAsConditional()
    {
      DoNamedTest2();
    }
  }
}