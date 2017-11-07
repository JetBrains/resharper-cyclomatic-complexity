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
using JetBrains.Application.UI.UIAutomation;

namespace JetBrains.ReSharper.Plugins.CyclomaticComplexity.Options
{
  public class ComplexityAnalysisOptionsViewModel : AAutomation
  {
    public ComplexityAnalysisOptionsViewModel(List<LanguageSpecificComplexityProperties> list)
    {
      PerLanguageProperties = new ObservableCollection<LanguageSpecificComplexityProperties>(list);
    }

    public ObservableCollection<LanguageSpecificComplexityProperties> PerLanguageProperties { get; private set; }
  }
}