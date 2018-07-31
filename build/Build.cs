using System;
using System.IO;
using System.Linq;
using Nuke.Common;
using Nuke.Common.Git;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.MSBuild;
using Nuke.Common.Tools.NuGet;
using Nuke.Common.Tools.Nunit;
using Nuke.Common.Utilities;
using Nuke.Common.Utilities.Collections;
using static Nuke.Common.ChangeLog.ChangelogTasks;
using static Nuke.Common.Tools.MSBuild.MSBuildTasks;
using static Nuke.Common.Tools.NuGet.NuGetTasks;
using static Nuke.Common.Tooling.NuGetPackageResolver;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;
using static Nuke.Common.Tooling.ProcessTasks;
using static Nuke.Common.Tools.Git.GitTasks;
using static Nuke.Common.Tools.Nunit.NunitTasks;

// ReSharper disable ArrangeThisQualifier

class Build : NukeBuild
{
  [Parameter] readonly string ApiKey;

  [Parameter] readonly string Configuration = "Release";

  [GitRepository] readonly GitRepository GitRepository;
  [Solution] readonly Solution Solution;
  [Parameter] readonly string Source = "https://resharper-plugins.jetbrains.com/api/v2/package";
  [Parameter] readonly string Version;

  string PackagesConfigFile => Solution.GetProject("CyclomaticComplexity.RD");
  AbsolutePath SourceDirectory => RootDirectory / "src";
  AbsolutePath OutputDirectory => RootDirectory / "output";

  AbsolutePath RiderDirectory => SourceDirectory / "RiderPlugin";

  Target Clean => _ => _
    .Executes(() =>
    {
      DeleteDirectories(GlobDirectories(SourceDirectory, "**/bin", "**/obj"));
      DeleteDirectory(RiderDirectory / "build");
      EnsureCleanDirectory(OutputDirectory);
    });

  Target Restore => _ => _
    .DependsOn(Clean)
    .Executes(() =>
    {
      NuGetRestore(s => s
        .SetTargetPath(Solution));
    });

  Target Compile => _ => _
    .DependsOn(Restore)
    .Executes(() =>
    {
      MSBuild(s => s
        .SetSolutionFile(Solution)
        .SetTargets("Rebuild")
        .SetConfiguration(Configuration)
        .DisableNodeReuse());

      StartProcess(
          RiderDirectory / "gradlew.bat",
          $"buildPlugin -Pconfiguration={Configuration} -Pversion={Version ?? "9.9.9.9"}",
          RiderDirectory)
        .AssertZeroExitCode();
    });

  Target Test => _ => _
    .DependsOn(Compile)
    .Executes(() =>
    {
      Nunit3(s => s
        .AddInputFiles(GlobFiles(RootDirectory / "test", $"**/bin/{Configuration}/tests.dll").NotEmpty())
        .EnableNoResults());
    });

  string ChangelogFile => RootDirectory / "CHANGELOG.md";

  Target Pack => _ => _
    .DependsOn(Compile)
    .Requires(() => Version)
    .Executes(() =>
    {
      GlobFiles(RootDirectory / "install", "*.nuspec")
        .ForEach(x => NuGetPack(s => s
          .SetTargetPath(x)
          .SetConfiguration(Configuration)
          .SetVersion(Version)
          .SetBasePath(RootDirectory)
          .SetOutputDirectory(OutputDirectory)
          .SetProperty("wave", GetWaveVersion(PackagesConfigFile) + ".0")
          .SetProperty("currentyear", DateTime.Now.Year.ToString())
          .SetProperty("releasenotes", GetNuGetReleaseNotes(ChangelogFile, GitRepository))
          .EnableNoPackageAnalysis()));

      var riderPlugin = GlobFiles(RiderDirectory / "build" / "distributions", "*.zip").Single();
      File.Copy(riderPlugin, OutputDirectory / Path.GetFileName(riderPlugin));
    });

  Target Push => _ => _
    .DependsOn(Pack)
    .Requires(() => ExtractChangelogSectionNotes(ChangelogFile, "vNext").Any())
    .Requires(() => ApiKey)
    .Requires(() => Configuration.EqualsOrdinalIgnoreCase("Release"))
    .Executes(() =>
    {
      GlobFiles(OutputDirectory, "*.nupkg")
        .ForEach(x => NuGetPush(s => s
          .SetTargetPath(x)
          .SetSource(Source)
          .SetApiKey(ApiKey)));

      if (!Version.Contains("-"))
      {
        FinalizeChangelog(ChangelogFile, Version, GitRepository);
        Git($"add {ChangelogFile}");
        Git($"commit -m \"Finalize {Path.GetFileName(ChangelogFile)} for {Version}\"");

        Git($"tag {Version}");
      }
    });

  public static int Main() => Execute<Build>(x => x.Pack);

  static string GetWaveVersion(string packagesConfigFile)
  {
    var fullWaveVersion = GetLocalInstalledPackages(packagesConfigFile, true)
      .SingleOrDefault(x => x.Id == "Wave").NotNull("fullWaveVersion != null").Version.ToString();
    return fullWaveVersion.Substring(0, fullWaveVersion.IndexOf('.'));
  }
}