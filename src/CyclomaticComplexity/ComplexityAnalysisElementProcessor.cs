/*
 * Copyright 2007-2014 JetBrains
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
using JetBrains.ReSharper.Daemon;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.ControlFlow;
using JetBrains.ReSharper.Psi.ControlFlow.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Tree;

namespace JetBrains.ReSharper.PowerToys.CyclomaticComplexity
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
        var message = string.Format("Member has cyclomatic complexity of {0} ({1}%)", complexity, (int)(complexity * 100.0 / myThreshold));
        var warning = new ComplexityWarning(message);
        myHighlightings.Add(new HighlightingInfo(declaration.GetNameDocumentRange(), warning));
      }
    }

    /// <summary>
    /// This method walks the control flow graph counting edges and nodes. Cyclomatic complexity is then calculated from the two values.
    /// </summary>
    private static int CalculateCyclomaticComplexity(ICSharpFunctionDeclaration declaration)
    {
      var graph = CSharpControlFlowBuilder.Build(declaration);
      var edges = GetEdges(graph);
      var nodeCount = GetNodeCount(edges);

      return edges.Count - nodeCount + 2;
    }

    private static HashSet<IControlFlowRib> GetEdges(IControlFlowGraf graph)
    {
      var edges = new HashSet<IControlFlowRib>();
      foreach(var element in graph.AllElements)
      {
        foreach(var edge in element.Exits)
          edges.Add(edge);
        foreach(var edge in element.Entries)
          edges.Add(edge);
      }
      return edges;
    }

    private static int GetNodeCount(IEnumerable<IControlFlowRib> edges)
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