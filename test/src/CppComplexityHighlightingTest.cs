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

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using JetBrains.ProjectModel;
using JetBrains.ProjectModel.Properties;
using JetBrains.ProjectModel.Properties.VCXProj;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.FeaturesTestFramework.Daemon;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Cpp.Language;
using JetBrains.ReSharper.TestFramework;
using NUnit.Framework;
using PlatformID = JetBrains.Application.platforms.PlatformID;

namespace JetBrains.ReSharper.Plugins.CyclomaticComplexity.Tests
{
  [TestSettingsKey(typeof(CyclomaticComplexityAnalysisSettings))]
  [TestFileExtension(CppProjectFileType.CPP_EXTENSION)]
  public class CppComplexityHighlightingTest : HighlightingTestBase
  {
    [NotNull]
    protected override string RelativeTestDataPath { get { return "Cpp"; } }

    protected override bool HighlightingPredicate(IHighlighting highlighting, IPsiSourceFile sourceFile)
    {
      return highlighting is ComplexityHighlight;
    }

    public override IProjectProperties GetProjectProperties(PlatformID platformId, ICollection<Guid> flavours)
    {
      return VCXProjectPropertiesFactory.CreateVCXProjectProperties(platformId, flavours);
    }

    protected override PsiLanguageType CompilerIdsLanguage
    {
      get { return CppLanguage.Instance; }
    }

    [Test]
    public void TestMethodWithDefaultSettings()
    {
      DoOneTest("MethodWithDefaultSettings");
    }

    [Test]
    [TestSettings("{ Thresholds: [ { 'CPP': 10 }, { 'CPP': 21 }, { 'CPP': 30 } ] }")]
    public void TestMethodWithNonDefaultSettings()
    {
      DoOneTest("MethodWithModifiedSettings");
    }
  }
}