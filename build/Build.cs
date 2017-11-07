// /*
//  * Copyright 2007-2015 JetBrains
//  *
//  * Licensed under the Apache License, Version 2.0 (the "License");
//  * you may not use this file except in compliance with the License.
//  * You may obtain a copy of the License at
//  *
//  * http://www.apache.org/licenses/LICENSE-2.0
//  *
//  * Unless required by applicable law or agreed to in writing, software
//  * distributed under the License is distributed on an "AS IS" BASIS,
//  * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  * See the License for the specific language governing permissions and
//  * limitations under the License.
//  */

using System;
using System.Linq;
using Nuke.Common.Tools;
using Nuke.Common.Tools.GitVersion;
using Nuke.Common.Tools.NuGet;
using Nuke.Common.Tools.Nunit;
using Nuke.Core;
using Nuke.Core.Tooling;
using Nuke.Core.Utilities;
using Nuke.Core.Utilities.Collections;
using static Nuke.Common.Tools.MSBuild.MSBuildTasks;
using static Nuke.Common.Tools.NuGet.NuGetTasks;
using static Nuke.Common.Tools.Nunit.NunitTasks;
using static Nuke.Core.IO.FileSystemTasks;
using static Nuke.Core.IO.PathConstruction;

class Build : NukeBuild
{
  public static int Main () => Execute<Build>(x => x.Pack);

  [GitVersion] readonly GitVersion GitVersion;
  [Parameter] readonly string ReSharperGalleryApiKey;

  Target Clean => _ => _
      .Executes(() =>
      {
        DeleteDirectories(GlobDirectories(SourceDirectory, "**/bin", "**/obj"));
        EnsureCleanDirectory(OutputDirectory);
      });

  Target Restore => _ => _
      .DependsOn(Clean)
      .Executes(() =>
      {
        MSBuild(s => DefaultMSBuildRestore);
      });

  Target Compile => _ => _
      .DependsOn(Restore)
      .Executes(() =>
      {
        MSBuild(s => DefaultMSBuildCompile);
      });

  Target Test => _ => _
      .DependsOn(Compile)
      .Executes(() =>
      {
        Nunit3(s => s
            .AddInputFiles(GlobFiles(RootDirectory / "test", $"**/bin/{Configuration}/tests.dll"))
            .EnableNoResults()
            .SetToolPath(ToolPathResolver.GetPackageExecutable("NUnit.ConsoleRunner", "nunit3-console.exe")));
      });

  Target Pack => _ => _
      .DependsOn(Compile)
      .Executes(() =>
      {
        GlobFiles(RootDirectory / "install", "*.nuspec")
            .ForEach(x => NuGetPack(s => DefaultNuGetPack
                .SetTargetPath(x)
                .SetBasePath(RootDirectory / "install")
                .SetOutputDirectory(OutputDirectory)));
      });

  Target Push => _ => _
      .Requires(() => Configuration.EqualsOrdinalIgnoreCase("Release"))
      .Requires(() => ReSharperGalleryApiKey)
      .DependsOn(Test, Pack)
      .Executes(() =>
      {
        GlobFiles(OutputDirectory, "*.nupkg")
            .ForEach(x => NuGetPush(s => s
                .SetTargetPath(x)
                .SetSource("https://resharper-plugins.jetbrains.com/api/v2/package")
                .SetApiKey(ReSharperGalleryApiKey)));
      });
}