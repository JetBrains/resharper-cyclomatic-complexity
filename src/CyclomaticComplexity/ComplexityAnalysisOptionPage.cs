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

using System.Linq;
using JetBrains.Application.Settings;
using JetBrains.DataFlow;
using JetBrains.ReSharper.Feature.Services.Daemon.OptionPages;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.ControlFlow;
using JetBrains.UI.Options;
using JetBrains.UI.Options.OptionsDialog2.SimpleOptions;
using JetBrains.UI.Options.OptionsDialog2.SimpleOptions.ViewModel;

namespace JetBrains.ReSharper.Plugins.CyclomaticComplexity
{
  [OptionsPage(PageId, "Complexity Analysis", typeof(CyclomaticComplexityThemedIcons.ComplexityOptionPage), ParentId = CodeInspectionPage.PID)]
  public class ComplexityAnalysisOptionPage : SimpleOptionsPage
  {
    private const string PageId = "PowerToys.CyclomaticComplexity";

    public ComplexityAnalysisOptionPage(Lifetime lifetime, OptionsSettingsSmartContext optionsSettingsSmartContext, 
                                        ILanguages languages, ILanguageManager languageManager)
      : base(lifetime, optionsSettingsSmartContext)
    {
      var thresholds = OptionsSettingsSmartContext.Schema.GetIndexedEntry((CyclomaticComplexityAnalysisSettings s) => s.Thresholds);
      foreach (var languageType in languages.All.Where(languageManager.HasService<IControlFlowBuilder>).OrderBy(l => l.PresentableName))
      {
        var property = new Property<int>(lifetime, string.Format("{0}_IntOptionViewModel_{1}intValueProperty", GetType(), languageType.Name));
        OptionsSettingsSmartContext.SetBinding(lifetime, thresholds, languageType.Name, property, CyclomaticComplexityAnalysisSettings.DefaultThreshold);
        var option = new IntOptionViewModel(property, languageType.PresentableName, string.Format("Threshold for {0}", languageType.PresentableName), 2, 1);
        OptionEntities.Add(option);
      }
    }

#if false
    private readonly Lifetime myLifetime;
    private readonly OptionsSettingsSmartContext mySettings;
    private const string PID = "PowerToys.CyclomaticComplexity";

    public ComplexityAnalysisOptionPage(Lifetime lifetime, IUIApplication environment, OptionsSettingsSmartContext settings)
      : base(lifetime, environment, PID)
    {
      myLifetime = lifetime;
      mySettings = settings;
      InitControls();
    }

    private void InitControls()
    {
      Controls.Spin spin; // This variable may be reused if there's more than one spin on the page
      Controls.HorzStackPanel stack;

      // The upper cue banner, stacked in the first line of our page, docked to full width with word wrapping, as needed
      Controls.Add(new Controls.Label(Properties.Resources.Options_Banner));

      // Some spacing
      Controls.Add(UI.Options.Helpers.Controls.Separator.DefaultHeight);

      // A horizontal stack of a text label and a spin-edit
      Controls.Add(stack = new Controls.HorzStackPanel(Environment));
      stack.Controls.Add(new Controls.Label(Properties.Resources.Options_ThresholdLabel)); // The first column of the stack
      stack.Controls.Add(spin = new Controls.Spin());

      // Set up the spin we've just added
      spin.Maximum = new decimal(new[] {500, 0, 0, 0});
      spin.Minimum = new decimal(new[] {1, 0, 0, 0});
      spin.Value = new decimal(new[] {1, 0, 0, 0});

      // This binding will take the initial value from ComplexityAnalysisOptionPage, put it into the edit, and pass back from UI to the control if the OK button is hit
      //mySettings.SetBinding(myLifetime, (CyclomaticComplexityAnalysisSettings s) => s.Threshold, spin.IntegerValue);
    }
#endif
  }
}