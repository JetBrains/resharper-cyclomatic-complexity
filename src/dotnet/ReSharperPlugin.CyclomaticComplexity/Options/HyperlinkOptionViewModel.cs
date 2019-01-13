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
using System.Windows.Input;
using JetBrains.Application.UI.Options;
using JetBrains.Application.UI.Options.OptionsDialog.SimpleOptions;
using JetBrains.DataFlow;

namespace JetBrains.ReSharper.Plugins.CyclomaticComplexity.Options
{
  public class HyperlinkOptionViewModel : OptionEntityPrimitive, IOptionCanBeEnabled, IOptionCanBeVisible
  {
    public HyperlinkOptionViewModel(Lifetime lifetime, string text, ICommand command)
      : base(lifetime)
    {
      Text = text;
      Command = command;

      IsEnabledProperty = new Property<bool>(lifetime, "IsEnabledProperty") { Value = true };
      IsVisibleProperty = new Property<bool>(lifetime, "IsVisibleProperty") { Value = true };
    }

    public string Text { get; }
    public ICommand Command { get; set; }

    public new IProperty<bool> IsEnabledProperty { get; }
    public new IProperty<bool> IsVisibleProperty { get; }

    public override IEnumerable<OptionsPageKeyword> GetKeywords()
    {
      yield return new OptionsPageKeyword(Text);
    }
  }
}