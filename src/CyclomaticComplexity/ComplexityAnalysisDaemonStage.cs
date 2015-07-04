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
using System.Collections.Generic;
using JetBrains.Application.Settings;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Psi;

namespace JetBrains.ReSharper.Plugins.CyclomaticComplexity
{
  /// <summary>
  /// Daemon stage for comlexity analysis. This class is automatically loaded by ReSharper daemon 
  /// because it's marked with the attribute.
  /// </summary>
  [DaemonStage]
  public class ComplexityAnalysisDaemonStage : IDaemonStage
  {
    /// <summary>
    /// This method provides a <see cref="IDaemonStageProcess"/> instance which is assigned to highlighting a single document
    /// </summary>
    public IEnumerable<IDaemonStageProcess> CreateProcess(IDaemonProcess process, IContextBoundSettingsStore settings, DaemonProcessKind kind)
    {
      if (process == null)
        throw new ArgumentNullException("process");

      return new[]
      {
        new ComplexityAnalysisDaemonStageProcess(process,
          settings.GetValue((ComplexityAnalysisSettings s) => s.Threshold))
      };
    }

    public ErrorStripeRequest NeedsErrorStripe(IPsiSourceFile sourceFile, IContextBoundSettingsStore settings)
    {
      // We want to add markers to the right-side stripe as well as contribute to document errors
      return ErrorStripeRequest.STRIPE_AND_ERRORS;
    }
  }
}