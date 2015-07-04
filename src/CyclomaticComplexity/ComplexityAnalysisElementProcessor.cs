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
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.ControlFlow;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.Util;

namespace JetBrains.ReSharper.Plugins.CyclomaticComplexity
{
  public class ComplexityAnalysisElementProcessor : IRecursiveElementProcessor
  {
    private readonly List<HighlightingInfo> myHighlightings = new List<HighlightingInfo>();
    private readonly IDaemonProcess myProcess;
    private readonly int myThreshold;

    public ComplexityAnalysisElementProcessor(IDaemonProcess process, int threshold)
    {
      myProcess = process;
      myThreshold = threshold;
    }

    public List<HighlightingInfo> Highlightings
    {
      get { return myHighlightings; }
    }

    public bool InteriorShouldBeProcessed(ITreeNode element)
    {
      return true;
    }

    public void ProcessBeforeInterior(ITreeNode element)
    {
    }

    public void ProcessAfterInterior(ITreeNode element)
    {
      // We are only interested in function declarations (methods, property accessors, etc.)
      var functionDeclaration = element as ICSharpFunctionDeclaration;
      if (functionDeclaration != null)
        ProcessFunctionDeclaration(functionDeclaration);
    }

    public bool ProcessingIsFinished
    {
      get { return myProcess.InterruptFlag; }
    }

    private void ProcessFunctionDeclaration(ICSharpFunctionDeclaration declaration)
    {
      if(declaration.Body == null)
        return;

      var complexity = CalculateCyclomaticComplexity(declaration);
      if(complexity > myThreshold)
      {
        var declaredElement = declaration.DeclaredElement;
        var declarationType = DeclaredElementPresenter.Format(declaration.Language, DeclaredElementPresenter.KIND_PRESENTER, declaredElement);
        var declaredElementName = DeclaredElementPresenter.Format(declaration.Language, DeclaredElementPresenter.NAME_PRESENTER, declaredElement);
        var message = string.Format("{0} '{1}' has cyclomatic complexity of {2} ({3}% of threshold)", declarationType.Capitalize(), 
          declaredElementName, complexity, (int)(complexity * 100.0 / myThreshold));

        var documentRange = declaration.GetNameDocumentRange();
        var warning = new ComplexityWarning(message, documentRange);
        myHighlightings.Add(new HighlightingInfo(documentRange, warning));
      }
    }

    /// <summary>
    /// This method walks the control flow graph counting edges and nodes. Cyclomatic complexity is then calculated from the two values.
    /// </summary>
    private static int CalculateCyclomaticComplexity(ICSharpFunctionDeclaration declaration)
    {
      var graph = ControlFlowBuilder.GetGraph(declaration);
      var edges = GetEdges(graph);
      var nodeCount = GetNodeCount(edges);

      return edges.Count - nodeCount + 2;
    }

    private static HashSet<IControlFlowEdge> GetEdges(IControlFlowGraph graph)
    {
      var edges = new HashSet<IControlFlowEdge>();
      foreach(var element in graph.AllElements)
      {
        foreach(var edge in element.Exits)
          edges.Add(edge);
        foreach(var edge in element.Entries)
          edges.Add(edge);
      }
      return edges;
    }

    private static int GetNodeCount(IEnumerable<IControlFlowEdge> edges)
    {
      var hasNullSource = false;
      var hasNullDestination = false;

      var nodes = new HashSet<IControlFlowElement>();
      foreach(var edge in edges)
      {
        if(edge.Source != null)
          nodes.Add(edge.Source);
        else
          hasNullSource = true;

        if(edge.Target != null)
          nodes.Add(edge.Target);
        else
          hasNullDestination = true;
      }
      return nodes.Count + (hasNullDestination ? 1 : 0) + (hasNullSource ? 1 : 0);
    }
  }
}