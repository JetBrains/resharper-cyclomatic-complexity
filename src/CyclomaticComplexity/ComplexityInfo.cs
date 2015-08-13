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

using JetBrains.DocumentModel;
using JetBrains.ReSharper.Feature.Services.Daemon;

namespace JetBrains.ReSharper.Plugins.CyclomaticComplexity
{
  [StaticSeverityHighlighting(Severity.INFO, "CSharpInfo")]
  public class ComplexityInfo : IComplexityHighlighting
  {
    private readonly string myTooltip;
    private readonly DocumentRange range;

    public ComplexityInfo(string toolTip, DocumentRange range)
    {
      myTooltip = toolTip;
      this.range = range;
    }

    public DocumentRange CalculateRange()
    {
      return range;
    }

    public string ToolTip
    {
      get { return myTooltip; }
    }

    public string ErrorStripeToolTip
    {
      get { return myTooltip; }
    }

    public int NavigationOffsetPatch
    {
      get { return 0; }
    }

    public bool IsValid()
    {
      return true;
    }
  }
}