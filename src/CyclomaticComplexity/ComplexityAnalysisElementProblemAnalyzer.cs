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
using JetBrains.DocumentModel;
using JetBrains.ReSharper.Daemon.Stages.Dispatcher;
using JetBrains.ReSharper.Daemon.Stages.Utils;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.ControlFlow;
using JetBrains.ReSharper.Psi.JavaScript.Tree;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.Util;

namespace JetBrains.ReSharper.Plugins.CyclomaticComplexity
{
  [ElementProblemAnalyzer(typeof(ITreeNode))]
  public class ComplexityAnalysisElementProblemAnalyzer : ElementProblemAnalyzer<ITreeNode>
  {
    private readonly Key<State> key = new Key<State>("ComplexityAnalyzerState");

    protected override void Run(ITreeNode element, ElementProblemAnalyzerData data, IHighlightingConsumer consumer)
    {
      // We get a fresh data for each file, so we can cache some state to make
      // things a bit more efficient
      var state = data.GetOrCreateData(key, () => new State
      {
        File = element.GetContainingFile(),
        ControlFlowBuilder = LanguageManager.Instance.TryGetService<IControlFlowBuilder>(element.Language)
      });

      if (state.File == null || state.ControlFlowBuilder == null || !state.ControlFlowBuilder.CanBuildFrom(element))
        return;

      // We can build control flow information for a JS file section (e.g. inline event handlers, or <src>
      // elements in html) and it would be nice to flag these up as being too complex, but there's nowhere
      // to put the warning - it highlights the whole section. Maybe that's ok for when we're over the threshold,
      // but it's definitely not ok for the info tooltip.
      if (element is IJavaScriptFileSection)
        return;

      var graph = ControlFlowDaemonUtil.GetOrBuild(element, data);
      if (graph == null)
        return;

      var threshold = data.SettingsStore.GetValue((ComplexityAnalysisSettings s) => s.Threshold);

      var complexity = CalculateCyclomaticComplexity(graph);
      var message = GetMessage(element, complexity, threshold);
      var documentRange = GetDocumentRange(element);

      var highlight = new ComplexityHighlight(message, documentRange);
      consumer.AddHighlighting(highlight, highlight.CalculateRange(), state.File,
        complexity > threshold ? Severity.WARNING : Severity.INFO);
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

    private static string GetMessage(ITreeNode element, int complexity, int threshold)
    {
      var type = "Element";
      IDeclaration declaration;
      GetBestTreeNode(element, out declaration);
      if (declaration != null && declaration.DeclaredElement != null)
      {
        var declaredElement = declaration.DeclaredElement;
        var declarationType = DeclaredElementPresenter.Format(declaration.Language,
          DeclaredElementPresenter.KIND_PRESENTER, declaredElement);
        var declaredElementName = DeclaredElementPresenter.Format(declaration.Language,
          DeclaredElementPresenter.NAME_PRESENTER, declaredElement);

        type = string.Format("{0} '{1}'", declarationType.Capitalize(), declaredElementName);
      }

      return string.Format("{0} has cyclomatic complexity of {1} ({2}% of threshold)", type,
        complexity, (int) (complexity*100.0/threshold));
    }

    private DocumentRange GetDocumentRange(ITreeNode element)
    {
      IDeclaration declaration;
      var node = GetBestTreeNode(element, out declaration);
      return declaration != null && declaration.DeclaredElement != null ? declaration.GetNameDocumentRange() : node.GetDocumentRange();
    }

    private static ITreeNode GetBestTreeNode(ITreeNode element, out IDeclaration declaration)
    {
      declaration = element as IDeclaration;
      if (declaration != null && declaration.DeclaredElement != null)
        return element;

      // Don't have a declared element to highlight. Going to have to guess. Try the 
      // first meaningful child
      return element.GetNextMeaningfulChild(null);
    }

    private class State
    {
      public IFile File;
      public IControlFlowBuilder ControlFlowBuilder;
    }
  }
}