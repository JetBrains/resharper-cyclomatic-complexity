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
using JetBrains.Application.UI.Controls.TreeListView;
using JetBrains.Application.UI.Options;
using JetBrains.DataFlow;

namespace JetBrains.ReSharper.Plugins.CyclomaticComplexity.Options
{
  public class LanguageSpecificComplexityProperties : ObservableObject
  {
    public LanguageSpecificComplexityProperties(Lifetime lifetime, OptionsSettingsSmartContext settings, SettingsIndexedEntry settingsIndexedEntry, string index, string languagePresentableName, int defaultValue)
    {
      Name = languagePresentableName;

      Threshold = new Property<int>(lifetime, index);
      Threshold.Change.Advise(lifetime, () => OnPropertyChanged(nameof(Threshold)));
      settings.SetBinding(lifetime, settingsIndexedEntry, index, Threshold, defaultValue);
    }

    public string Name { get; private set; }
    public IProperty<int> Threshold { get; set; }
  }
}