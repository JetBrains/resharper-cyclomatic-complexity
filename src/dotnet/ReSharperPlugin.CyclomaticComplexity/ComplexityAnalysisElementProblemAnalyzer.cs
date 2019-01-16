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
using System.Linq;
using JetBrains.Application.Settings;
using JetBrains.DocumentModel;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.ControlFlow;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.JavaScript.Tree;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.UI.RichText;
using JetBrains.Util;
#if RIDER
using JetBrains.Application.UI.Icons.ComposedIcons;
using JetBrains.ReSharper.Host.Platform.Icons;
using JetBrains.ReSharper.Features.SolBuilderDuo.Src;
#endif

namespace JetBrains.ReSharper.Plugins.CyclomaticComplexity
{
  [ElementProblemAnalyzer(typeof(ITreeNode))]
  public class ComplexityAnalysisElementProblemAnalyzer : ElementProblemAnalyzer<ITreeNode>
  {
    private readonly Key<State> key = new Key<State>("ComplexityAnalyzerState");
    
#if RIDER
    private readonly ComplexityCodeInsightsProvider _provider;
    private readonly IconHost _iconHost;

    public ComplexityAnalysisElementProblemAnalyzer(ComplexityCodeInsightsProvider provider, IconHost iconHost)
    {
      _provider = provider;
      _iconHost = iconHost;
    }
#endif
    
    protected override void Run(ITreeNode element, ElementProblemAnalyzerData data, IHighlightingConsumer consumer)
    {
      // We get a fresh data for each file, so we can cache some state to make
      // things a bit more efficient
      var state = data.GetOrCreateDataUnderLock(key, () => new State
      {
        ControlFlowBuilder = LanguageManager.Instance.TryGetService<IControlFlowBuilder>(element.Language),
        Threshold = GetThreshold(data, element.Language)
      });

      if (state.ControlFlowBuilder == null || !state.ControlFlowBuilder.CanBuildFrom(element))
        return;

      // We can build control flow information for a JS file section (e.g. inline event handlers, or <src>
      // elements in html) and it would be nice to flag these up as being too complex, but there's nowhere
      // to put the warning - it highlights the whole section. Maybe that's ok for when we're over the threshold,
      // but it's definitely not ok for the info tooltip.
      if (element is IJavaScriptFileSection)
        return;

      var graph = data.GetOrBuildControlFlowGraph(element);
      if (graph == null)
        return;

      var complexity = CalculateCyclomaticComplexity(graph);
      var documentRange = GetDocumentRange(element);

      if (complexity > state.Threshold)
      {
        consumer.AddHighlighting(
          new ComplexityWarningHighlight(
            GetMessage(element, complexity, state.Threshold),
            documentRange));
      }
#if !RIDER
      else
        consumer.AddHighlighting(new ComplexityInfoHighlight(GetMessage(element, complexity, state.Threshold), documentRange));
#else
      if (element is ITypeMemberDeclaration memberDeclaration && memberDeclaration.DeclaredElement != null)
      {
        var percentage = GetPercentage(complexity, state.Threshold);
        consumer.AddHighlighting(new ComplexityCodeInsightsHighlight(
          memberDeclaration,
          complexity,
          percentage,
          _provider,
          _iconHost));
      }
#endif
    }

    private static int GetThreshold(ElementProblemAnalyzerData data, PsiLanguageType language)
    {
      var threshold = data.SettingsStore.GetIndexedValue((CyclomaticComplexityAnalysisSettings s) => s.Thresholds, language.Name);
      if (threshold < 1)
      {
        data.SettingsStore.SetIndexedValue((CyclomaticComplexityAnalysisSettings s) => s.Thresholds, language.Name, CyclomaticComplexityAnalysisSettings.DefaultThreshold);
        threshold = CyclomaticComplexityAnalysisSettings.DefaultThreshold;
      }
      return threshold;
    }

    private static int CalculateCyclomaticComplexity(IControlFlowGraph graph)
    {
      var edges = GetEdges(graph);
      var nodeCount = GetNodeCount(edges);

      // Standard CC formula (edges - nodes + 2)
      return edges.Count - nodeCount + 2;
    }

    // ReSharper's C# graph treats boolean values as conditions, meaning we
    // get two (duplicate) edges from one node to the next. This comparer
    // will remove the duplicates. I.e. any edges that have the same source
    // and target are considered equal
    private class EdgeEqualityComparer : EqualityComparer<IControlFlowEdge>
    {
      public override bool Equals(IControlFlowEdge x, IControlFlowEdge y)
      {
        if (x == y)
          return true;
        if (x == null || y == null)
          return false;
        return x.Source == y.Source && x.Target == y.Target;
      }

      public override int GetHashCode(IControlFlowEdge obj)
      {
        if (obj == null)
          return 0;
        var hashCode = obj.Source.GetHashCode();
        if (obj.Target != null)
          hashCode ^= obj.Target.GetHashCode();
        return hashCode;
      }
    }

    private static HashSet<IControlFlowEdge> GetEdges(IControlFlowGraph graph)
    {
      var edges = new HashSet<IControlFlowEdge>(new EdgeEqualityComparer());
      foreach (var element in graph.AllElements)
      {
        foreach (var edge in element.Exits)
          edges.Add(edge);
        foreach (var edge in element.Entries)
          edges.Add(edge);
      }
      return FudgeGraph(graph, edges);
    }

    private static HashSet<IControlFlowEdge> FudgeGraph(IControlFlowGraph graph, HashSet<IControlFlowEdge> edges)
    {
      // The C# graph treats the unary negation operator `!` as a conditional. It's not,
      // but having it so means that the CC can be way higher than it should be.
      var dodgyEdges = new HashSet<IControlFlowEdge>(new EdgeEqualityComparer());

      // Look at all of the non-leaf elements (the graph is a tree as well as a graph.
      // The tree represents the structure of the code, with the control flow graph
      // running through it)
      foreach (var element in graph.AllElements.Where(e => e.Children.Count != 0))
      {
        var unaryOperatorExpression = element.SourceElement as IUnaryOperatorExpression;
        if (unaryOperatorExpression != null && unaryOperatorExpression.UnaryOperatorType == UnaryOperatorType.EXCL)
        {
          // The unary operator shouldn't have 2 exits. It's not a conditional. If it does,
          // remove one of the edges, and it's path, from the collected edges.
          if (element.Exits.Count == 2)
          {
            var edge = element.Exits[0];
            do
            {
              dodgyEdges.Add(edge);

              // Get the source of the exit edge. This is the node in the control flow graph,
              // not the element in the program structure tree.
              var source = edge.Source;
              edge = null;

              // Walk back to the control flow graph node that represents the exit of the
              // dodgy condition, so keep going until we have an element with 2 exits.
              if (source.Entries.Count == 1 && source.Exits.Count == 1)
                edge = source.Entries[0];
            } while (edge != null);
          }
        }
      }

      edges.ExceptWith(dodgyEdges);
      return edges;
    }

    private static int GetNodeCount(IEnumerable<IControlFlowEdge> edges)
    {
      var hasNullDestination = false;

      var nodes = new HashSet<IControlFlowElement>();
      foreach (var edge in edges)
      {
        nodes.Add(edge.Source);

        if (edge.Target != null)
          nodes.Add(edge.Target);
        else
          hasNullDestination = true;
      }
      return nodes.Count + (hasNullDestination ? 1 : 0);
    }

    private static string GetMessage(ITreeNode element, int complexity, int threshold)
    {
      var type = "Element";
      IDeclaration declaration;
      GetBestTreeNode(element, out declaration);
      if (declaration?.DeclaredElement != null)
      {
        var declaredElement = declaration.DeclaredElement;
        var declarationType = DeclaredElementPresenter.Format(declaration.Language,
          DeclaredElementPresenter.KIND_PRESENTER, declaredElement);
        var declaredElementName = DeclaredElementPresenter.Format(declaration.Language,
          DeclaredElementPresenter.NAME_PRESENTER, declaredElement);

        type = $"{declarationType.Capitalize()} '{declaredElementName}'";
      }

      return $"{type} has cyclomatic complexity of {complexity} ({GetPercentage(complexity, threshold)}% of threshold)";
    }

    private static int GetPercentage(int complexity, int threshold)
    {
      return (int) (complexity*100.0/threshold);
    }

    private DocumentRange GetDocumentRange(ITreeNode element)
    {
      IDeclaration declaration;
      var node = GetBestTreeNode(element, out declaration);
      return declaration?.DeclaredElement != null ? declaration.GetNameDocumentRange() : node.GetDocumentRange();
    }

    private static ITreeNode GetBestTreeNode(ITreeNode element, out IDeclaration declaration)
    {
      declaration = element as IDeclaration;
      if (declaration?.DeclaredElement != null)
        return element;

      // Don't have a declared element to highlight. Going to have to guess. Try the 
      // first meaningful child
      return element.GetNextMeaningfulChild(null);
    }

    private class State
    {
      public IControlFlowBuilder ControlFlowBuilder;
      public int Threshold;
    }
  }
}