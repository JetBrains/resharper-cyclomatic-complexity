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

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JetBrains.Application.UI.Commands;
using JetBrains.Application.UI.Options;
using JetBrains.Application.UI.Options.OptionsDialog;
using JetBrains.DataFlow;
using JetBrains.IDE.UI.Extensions;
using JetBrains.IDE.UI.Extensions.Properties;
using JetBrains.IDE.UI.Options;
using JetBrains.ReSharper.Feature.Services.Daemon.OptionPages;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.ControlFlow;
using JetBrains.ReSharper.Psi.JavaScript.WinRT.LanguageImpl;
using JetBrains.Rider.Model.UIAutomation;
using ReSharperPlugin.CyclomaticComplexity;

namespace JetBrains.ReSharper.Plugins.CyclomaticComplexity.Options
{
  [OptionsPage(PageId, "Cyclomatic Complexity", typeof(CyclomaticComplexityThemedIcons.CyclomaticComplexity), ParentId = CodeInspectionPage.PID)]
  public class ComplexityAnalysisOptionPage : BeSimpleOptionsPage
  {
    private const string PageId = "PowerToys.CyclomaticComplexity";

    public ComplexityAnalysisOptionPage(
      Lifetime lifetime,
      OptionsPageContext optionsPageContext,
      OptionsSettingsSmartContext optionsSettingsSmartContext,
      ILanguages languages,
      ILanguageManager languageManager)
      : base(lifetime, optionsPageContext, optionsSettingsSmartContext)
    {
      AddText("Specify cyclomatic complexity thresholds:");

      var thresholds = OptionsSettingsSmartContext.Schema.GetIndexedEntry((CyclomaticComplexityAnalysisSettings s) => s.Thresholds);

      var list = new List<LanguageSpecificComplexityProperty>();
      foreach (var languageType in languages.All.Where(languageManager.HasService<IControlFlowBuilder>).OrderBy(GetPresentableName))
      {
        var presentableName = GetPresentableName(languageType);
        var thing = new LanguageSpecificComplexityProperty(lifetime, optionsSettingsSmartContext, thresholds, languageType.Name, presentableName, CyclomaticComplexityAnalysisSettings.DefaultThreshold);
        list.Add(thing);
      }

      var treeGrid = list.GetBeList(lifetime,
        (l, e, p) => new List<BeControl>
        {
          e.Name.GetBeLabel(),
          e.Threshold.GetBeSpinner(lifetime, min: 1)
        },
        new TreeConfiguration(new []{"Language,*", "Threshold,auto"}));
      
      AddControl(treeGrid, isStar: true);
    }

    private static string GetPresentableName(PsiLanguageType psiLanguageType)
    {
      // Bah, WinRT JS is a different language, that supports control flow,
      // but has the same presentable name as normal JS. I don't like
      // adding language specific fixes...
      if (psiLanguageType is JavaScriptWinRTLanguage)
        return "JavaScript (WinRT)";
      return psiLanguageType.PresentableName;
    }
  }
}