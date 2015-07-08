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
using JetBrains.Application.Settings;
using JetBrains.ReSharper.Daemon.Stages.Dispatcher;
using JetBrains.ReSharper.Daemon.Stages.Utils;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.ControlFlow;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.Util;

namespace JetBrains.ReSharper.Plugins.CyclomaticComplexity
{
  [ElementProblemAnalyzer(typeof(ITreeNode))]
  public class ComplexityAnalysisElementProblemAnalyzer : ElementProblemAnalyzer<ITreeNode>
  {
    protected override void Run(ITreeNode element, ElementProblemAnalyzerData data, IHighlightingConsumer consumer)
    {
      var file = element.GetContainingFile();
      if (file == null)
        return;

      // ElementProblemAnalyzers only run on token nodes. Control flow can be built on non-tokens
      // C# tends to build on IDeclaration
      var service = LanguageManager.Instance.TryGetService<IControlFlowBuilder>(element.Language);
      if (service == null || !service.CanBuildFrom(element))
        return;

      var graph = ControlFlowDaemonUtil.GetOrBuild(element, data);
      if (graph == null)
        return;

      var threshold = data.SettingsStore.GetValue((ComplexityAnalysisSettings s) => s.Threshold);

      var complexity = CalculateCyclomaticComplexity(graph);
      if (complexity > threshold)
      {
        var declaration = element as IDeclaration;
        if (declaration != null && declaration.DeclaredElement != null)
        {
          var declaredElement = declaration.DeclaredElement;
          var declarationType = DeclaredElementPresenter.Format(declaration.Language,
            DeclaredElementPresenter.KIND_PRESENTER, declaredElement);
          var declaredElementName = DeclaredElementPresenter.Format(declaration.Language,
            DeclaredElementPresenter.NAME_PRESENTER, declaredElement);
          var message = string.Format("{0} '{1}' has cyclomatic complexity of {2} ({3}% of threshold)",
            declarationType.Capitalize(),
            declaredElementName, complexity, (int) (complexity*100.0/threshold));

          var documentRange = declaration.GetNameDocumentRange();
          var warning = new ComplexityWarning(message, documentRange);

          consumer.AddHighlighting(warning, file);
        }
        else
        {
          // Don't have a declared element to highlight. Going to have to guess. Try the 
          // first meaningful child
          var bestGuess = element.GetNextMeaningfulChild(null);
          var documentRange = bestGuess.GetDocumentRange();
          var message = string.Format("Element has cyclomatic complexity of {0} ({1}% of threshold)", complexity,
            (int) (complexity*100.0/threshold));
          var warning = new ComplexityWarning(message, documentRange);
          consumer.AddHighlighting(warning, file);
        }
      }
    }

    private static int CalculateCyclomaticComplexity(IControlFlowGraph graph)
    {
      var edges = GetEdges(graph);
      var nodeCount = GetNodeCount(edges);

      return edges.Count - nodeCount + 2;
    }

    private static HashSet<IControlFlowEdge> GetEdges(IControlFlowGraph graph)
    {
      var edges = new HashSet<IControlFlowEdge>();
      foreach (var element in graph.AllElements)
      {
        foreach (var edge in element.Exits)
          edges.Add(edge);
        foreach (var edge in element.Entries)
          edges.Add(edge);
      }
      return edges;
    }

    private static int GetNodeCount(IEnumerable<IControlFlowEdge> edges)
    {
      var hasNullSource = false;
      var hasNullDestination = false;

      var nodes = new HashSet<IControlFlowElement>();
      foreach (var edge in edges)
      {
        if (edge.Source != null)
          nodes.Add(edge.Source);
        else
          hasNullSource = true;

        if (edge.Target != null)
          nodes.Add(edge.Target);
        else
          hasNullDestination = true;
      }
      return nodes.Count + (hasNullDestination ? 1 : 0) + (hasNullSource ? 1 : 0);
    }
  }
}