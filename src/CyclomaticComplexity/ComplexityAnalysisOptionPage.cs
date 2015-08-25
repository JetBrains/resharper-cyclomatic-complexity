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
using System.Collections.ObjectModel;
using System.Linq;
using JetBrains.Application.Settings;
using JetBrains.DataFlow;
using JetBrains.ReSharper.Feature.Services.Daemon.OptionPages;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.ControlFlow;
using JetBrains.ReSharper.Psi.JavaScript.WinRT.LanguageImpl;
using JetBrains.UI.Avalon.TreeListView;
using JetBrains.UI.Options;
using JetBrains.UI.Options.OptionsDialog2.SimpleOptions;
using JetBrains.UI.Wpf;

namespace JetBrains.ReSharper.Plugins.CyclomaticComplexity
{
  public class ComplexityAnalysisOptionsViewModel : AAutomation
  {
    public ComplexityAnalysisOptionsViewModel(List<LanguageSpecificComplexityProperties> list)
    {
      PerLanguageProperties = new ObservableCollection<LanguageSpecificComplexityProperties>(list);
    }

    public ObservableCollection<LanguageSpecificComplexityProperties> PerLanguageProperties { get; private set; }
  }

  public class LanguageSpecificComplexityProperties : ObservableObject
  {
    public LanguageSpecificComplexityProperties(Lifetime lifetime, OptionsSettingsSmartContext settings, SettingsIndexedEntry settingsIndexedEntry, string index, string languagePresentableName, int defaultValue)
    {
      Name = languagePresentableName;

      Threshold = new Property<int>(lifetime, index);
      Threshold.Change.Advise(lifetime, () => OnPropertyChanged("Threshold"));
      settings.SetBinding(lifetime, settingsIndexedEntry, index, Threshold, defaultValue);
    }

    public string Name { get; private set; }
    public IProperty<int> Threshold { get; set; }
  }

  [OptionsPage(PageId, "Cyclomatic Complexity", typeof(CyclomaticComplexityThemedIcons.ComplexityOptionPage), ParentId = CodeInspectionPage.PID)]
  public class ComplexityAnalysisOptionPage : CustomSimpleOptionsPage
  {
    private const string PageId = "PowerToys.CyclomaticComplexity";

    public ComplexityAnalysisOptionPage(Lifetime lifetime, OptionsSettingsSmartContext optionsSettingsSmartContext, 
                                        ILanguages languages, ILanguageManager languageManager)
      : base(lifetime, optionsSettingsSmartContext)
    {
      AddText("Specify cyclomatic complexity thresholds:");

      var thresholds = OptionsSettingsSmartContext.Schema.GetIndexedEntry((CyclomaticComplexityAnalysisSettings s) => s.Thresholds);

      var list = new List<LanguageSpecificComplexityProperties>();
      foreach (var languageType in languages.All.Where(languageManager.HasService<IControlFlowBuilder>).OrderBy(GetPresentableName))
      {
        var presentableName = GetPresentableName(languageType);
        var thing = new LanguageSpecificComplexityProperties(lifetime, optionsSettingsSmartContext, thresholds, languageType.Name, presentableName, CyclomaticComplexityAnalysisSettings.DefaultThreshold);
        list.Add(thing);
      }
      AddCustomOption(new ComplexityAnalysisOptionsViewModel(list));

      // TODO: AddLink - What is a good complexity threshold value?
      // Link to wiki page on GitHub with details and links
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