/*
 * Copyright 2007-2014 JetBrains
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
using JetBrains.DataFlow;
using JetBrains.ReSharper.Features.Environment.Options.Inspections;
using JetBrains.UI.Application;
using JetBrains.UI.Options;
using JetBrains.UI.Options.Helpers;

namespace JetBrains.ReSharper.PowerToys.CyclomaticComplexity
{
  /// <summary>
  /// Implements an options page that holds a set of setting editors stacked in lines from top to bottom.
  /// </summary>
  [OptionsPage(PID, "Complexity Analysis", typeof(CyclomaticComplexityThemedIcons.ComplexityOptionPage), ParentId = CodeInspectionPage.PID)]
  public class ComplexityAnalysisOptionPage : AStackPanelOptionsPage
  {
    private readonly Lifetime myLifetime;
    private readonly OptionsSettingsSmartContext mySettings;
    private const string PID = "PowerToys.CyclomaticComplexity";

    public ComplexityAnalysisOptionPage(Lifetime lifetime, UIApplication environment, OptionsSettingsSmartContext settings)
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
      mySettings.SetBinding(myLifetime, (ComplexityAnalysisSettings s) => s.Threshold, spin.IntegerValue);
    }
  }
}