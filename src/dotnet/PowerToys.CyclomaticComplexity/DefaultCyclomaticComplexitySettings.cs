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

using System.IO;
using System.Reflection;
using JetBrains.Application;
using JetBrains.Application.Settings;
using JetBrains.DataFlow;
using JetBrains.Diagnostics;
using JetBrains.Lifetimes;
using JetBrains.Util;

namespace JetBrains.ReSharper.Plugins.CyclomaticComplexity
{
  [ShellComponent]
  public class DefaultCyclomaticComplexitySettings : IHaveDefaultSettingsStream
  {
    public string Name => "Default Cyclomatic Complexity Settings";

    public Stream GetDefaultSettingsStream(Lifetime lifetime)
    {
      var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("PowerToys.CyclomaticComplexity.CyclomaticComplexity.dotSettings");
      Assertion.Assert(stream != null, "stream != null");
      lifetime.AddDispose(stream);
      return stream;
    }
  }
}