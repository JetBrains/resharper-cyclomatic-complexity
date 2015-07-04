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

using System;
using JetBrains.Application.Progress;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Files;

namespace JetBrains.ReSharper.Plugins.CyclomaticComplexity
{
  public class ComplexityAnalysisDaemonStageProcess : IDaemonStageProcess
  {
    private readonly IDaemonProcess myDaemonProcess;
    private readonly int myThreshold;

    public ComplexityAnalysisDaemonStageProcess(IDaemonProcess daemonProcess, int threshold)
    {
      myDaemonProcess = daemonProcess;
      myThreshold = threshold;
    }

    public void Execute(Action<DaemonStageResult> commiter)
    {
      // Getting PSI (AST) for the file being highlighted
      var sourceFile = myDaemonProcess.SourceFile;
      var file = sourceFile.GetPsiServices().Files.GetDominantPsiFile<CSharpLanguage>(sourceFile) as ICSharpFile;
      if (file == null)
        return;

      // Running visitor against the PSI
      var elementProcessor = new ComplexityAnalysisElementProcessor(myDaemonProcess, myThreshold);
      file.ProcessDescendants(elementProcessor);

      // Checking if the daemon is interrupted by user activity
      if (myDaemonProcess.InterruptFlag)
        throw new ProcessCancelledException();

      // Commit the result into document
      commiter(new DaemonStageResult(elementProcessor.Highlightings));
    }

    public IDaemonProcess DaemonProcess
    {
      get { return myDaemonProcess; }
    }
  }
}