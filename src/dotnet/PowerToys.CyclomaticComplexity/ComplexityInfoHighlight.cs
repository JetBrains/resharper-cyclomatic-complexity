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
using JetBrains.TextControl.DocumentMarkup;
using Severity = JetBrains.ReSharper.Feature.Services.Daemon.Severity;
#if RIDER
using System.Collections.Generic;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Daemon.CodeInsights;
using JetBrains.ReSharper.Features.SolBuilderDuo.Src;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.Rider.Backend.Platform.Icons;
using JetBrains.Rider.Model;
using JetBrains.UI.Icons;
#endif

namespace JetBrains.ReSharper.Plugins.CyclomaticComplexity
{
#if RESHARPER
  [RegisterHighlighter(HighlightAttributeId)]
  [StaticSeverityHighlighting(Severity.INFO, typeof(HighlightingGroupIds.CodeSmellStatic), AttributeId = HighlightAttributeId)]
  public class ComplexityInfoHighlight : IComplexityHighlighting
  {
    public const string HighlightAttributeId = "Cyclomatic Complexity Highlight";

    private readonly DocumentRange range;

    public ComplexityInfoHighlight(string toolTip, DocumentRange range)
    {
      ToolTip = toolTip;
      this.range = range;
    }

    public DocumentRange CalculateRange() => range;
    public string ToolTip { get; }
    public string ErrorStripeToolTip => ToolTip;
    public bool IsValid() => true;
  }
#else
  [SolutionComponent]
  public class ComplexityCodeInsightsProvider : ICodeInsightsProvider
  {
    public bool IsAvailableIn(ISolution solution)
    {
      return true;
    }

    public void OnClick(CodeInsightHighlightInfo highlightInfo, ISolution solution)
    {
    }

    public void OnExtraActionClick(CodeInsightHighlightInfo highlightInfo, string actionId, ISolution solution)
    {
    }

    public string ProviderId => nameof(ComplexityCodeInsightsProvider);
    public string DisplayName => "Cyclomatic Complexity";
    public CodeVisionAnchorKind DefaultAnchor => CodeVisionAnchorKind.Top;

    public ICollection<CodeVisionRelativeOrdering> RelativeOrderings => new CodeVisionRelativeOrdering[]
      {new CodeVisionRelativeOrderingFirst()};
  }

  [RegisterHighlighter(
    HighlightAttributeId,
    EffectType = EffectType.NONE,
    TransmitUpdates = true,
    Layer = HighlighterLayer.SYNTAX + 1,
    GroupId = HighlighterGroupIds.HIDDEN)]
  [StaticSeverityHighlighting(Severity.INFO, typeof(HighlightingGroupIds.CodeInsights), AttributeId = HighlightAttributeId)]
  public class ComplexityCodeInsightsHighlight : CodeInsightsHighlighting
  {
    public const string HighlightAttributeId = "Cyclomatic Complexity Code Insight Highlight";

    private const int c_warningThreshold = 80;

    private static string GetLensText(int percentage)
      => (percentage < c_warningThreshold
        ? "simple enough"
        : percentage <= 100
          ? "mildly complex"
          : percentage <= 200
            ? "very complex"
            : "refactor me?!") + $" ({percentage}%)";

    private static IconId GetIconId(int percentage)
      => percentage < c_warningThreshold
        ? SolBuilderDuoThemedIcons.SolBuilderDuoRunningBuild.Id
        : percentage <= 100
          ? SolBuilderDuoThemedIcons.SolBuilderDuoRunningBuildWarning.Id
          : SolBuilderDuoThemedIcons.SolBuilderDuoRunningBuildError.Id;

    private static string GetMoreText(int complexity, int percentage)
      => $"Cyclomatic complexity of {complexity} ({percentage}% of threshold)";

    public ComplexityCodeInsightsHighlight(
      ITypeMemberDeclaration declaration,
      int complexity,
      int percentage,
      ICodeInsightsProvider provider,
      IconHost iconHost)
      : base(
        declaration.GetNameDocumentRange(),
        GetLensText(percentage),
        GetMoreText(complexity, percentage),
        GetMoreText(complexity, percentage),
        provider,
        declaration.DeclaredElement,
        iconHost.Transform(GetIconId(percentage)))
    {
    }
  }
#endif
}
