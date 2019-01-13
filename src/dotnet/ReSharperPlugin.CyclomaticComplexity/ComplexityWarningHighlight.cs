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
using JetBrains.ReSharper.Plugins.CyclomaticComplexity;
using JetBrains.ReSharper.Psi;

[assembly: RegisterConfigurableSeverity(ComplexityWarningHighlight.SeverityId,
  null, HighlightingGroupIds.CodeSmell,
  "Element exceeds cyclomatic complexity",
  @"The cyclomatic complexity of the code element exceeds the configured threshold.
You can configure the thresholds in the Cyclomatic Complexity options page.",
  Severity.WARNING)]

namespace JetBrains.ReSharper.Plugins.CyclomaticComplexity
{
  [ConfigurableSeverityHighlighting(SeverityId, KnownLanguage.ANY_LANGUAGEID, OverlapResolve = OverlapResolveKind.WARNING)]
  public class ComplexityWarningHighlight : IComplexityHighlighting
  {
    public const string SeverityId = "CyclomaticComplexity";

    private readonly DocumentRange range;

    public ComplexityWarningHighlight(string tooltip, DocumentRange range)
    {
      ToolTip = tooltip;
      this.range = range;
    }

    public bool IsValid() => true;
    public DocumentRange CalculateRange() => range;
    public string ToolTip { get; }
    public string ErrorStripeToolTip => ToolTip;
  }
}