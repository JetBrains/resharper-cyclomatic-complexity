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

using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.FeaturesTestFramework.Daemon;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.TestFramework;
using NUnit.Framework;

namespace JetBrains.ReSharper.Plugins.CyclomaticComplexity.Tests
{
  [TestSettingsKey(typeof(ComplexityAnalysisSettings))]
  public class JsComplexityHighlightingTest : JavaScriptHighlightingTestBase
  {
    protected override string RelativeTestDataPath { get { return "JS"; } }

    protected override bool HighlightingPredicate(IHighlighting highlighting, IPsiSourceFile sourceFile)
    {
      return highlighting is IComplexityHighlighting;
    }

    [Test]
    public void TestNamedMethodWithDefaultSettings()
    {
      DoOneTest("NamedMethodWithDefaultSettings");
    }

    [Test]
    [TestSettings("{ Threshold: [ 10, 21, 30] }")]
    public void TestNamedMethodWithNonDefaultSettings()
    {
      DoOneTest("NamedMethodWithModifiedSettings");
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

    [Test]
    public void TestAssignedFunctionWithDefaultSettings()
    {
      DoOneTest("AssignedFunctionWithDefaultSettings");
    }

    [Test]
    [TestSettings("{ Threshold: [ 10, 21, 30] }")]
    public void TestAssignedFunctionWithNonDefaultSettings()
    {
      DoOneTest("AssignedFunctionWithModifiedSettings");
    }

    [Test]
    public void TestNestedFunctionWithDefaultSettings()
    {
      DoOneTest("NestedFunctionWithDefaultSettings");
    }

    [Test]
    [TestSettings("{ Threshold: [ 10, 21, 30] }")]
    public void TestNestedFunctionWithNonDefaultSettings()
    {
      DoOneTest("NestedFunctionWithModifiedSettings");
    }
  }
}