/*
 * Copyright 2007-2011 JetBrains s.r.o.
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
using System.Drawing;
using System.Reflection;

using JetBrains.ReSharper.Daemon;
using JetBrains.ReSharper.TextControl.Markup;

namespace JetBrains.ReSharper.PowerToys.CyclomaticComplexity
{
  public class ComplexityWarningGutterMark : IconGutterMark
  {
    private static Image ourImage;

    static ComplexityWarningGutterMark()
    {
      ourImage = Image.FromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream(typeof (ComplexityWarning), "ComplexityWarning.png"));
    }

    public ComplexityWarningGutterMark()
      : base(ourImage)
    {
    }

    public override void OnClick(IHighlighter highlighter)
    {
    }

    public override bool IsClickable
    {
      get { return false; }
    }
  }
}