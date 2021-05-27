using System;
using System.Linq;
using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.Execution;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.GitVersion;
using Nuke.Common.Tools.MSBuild;
using Nuke.Common.Tools.NuGet;
using Nuke.Common.Utilities.Collections;
using static Nuke.Common.EnvironmentInfo;
using static Nuke.Common.IO.CompressionTasks;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static Nuke.Common.Tools.MSBuild.MSBuildTasks;
using static Nuke.Common.Tools.NuGet.NuGetTasks;


[CheckBuildProjectConfigurations]
[ShutdownDotNetAfterServerBuild]
class Build : NukeBuild
{
    public static int Main () => Execute<Build>(x => x.Default);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Parameter(Name = "inrule-version")]
    readonly string InRuleVersion = "5.2.0";

    [Parameter]
    readonly string GitHubAccessToken;

    [Parameter]
    readonly string NuGetApiKey;

    [Solution] readonly Solution Solution;
    [GitRepository] readonly GitRepository GitRepository;
    [GitVersion(Framework = "net5.0", NoFetch = true)] readonly GitVersion GitVersion;

    AbsolutePath SourceDirectory => RootDirectory / "src";
    AbsolutePath TestsDirectory => RootDirectory / "test";
    AbsolutePath ArtifactsDirectory => RootDirectory / "artifacts";

    Project AuthoringProject  => Solution.GetProject("Sknet.InRuleGitStorage.AuthoringExtension");
    Project SdkProject => Solution.GetProject("Sknet.InRuleGitStorage");
    Project SdkTestProject => Solution.GetProject("Sknet.InRuleGitStorage.Tests");

    string[] NuGetSources = new [] {
        "https://api.nuget.org/v3/index.json"
    };

    Target Clean => _ => _
        .Before(Restore)
        .Executes(() =>
        {
            SourceDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);
            TestsDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);
        });

    Target CleanArtifacts => _ => _
        .Before(Restore)
        .TriggeredBy(Clean)
        .Executes(() =>
        {
            EnsureCleanDirectory(ArtifactsDirectory);
        });

    Target CleanTestResults => _ => _
        .Before(Restore)
        .TriggeredBy(Clean)
        .Executes(() => 
        {
            TestsDirectory.GlobDirectories("**/TestResults").ForEach(DeleteDirectory);
        });

    Target Restore => _ => _
        .Executes(() =>
        {
            Logger.Normal("Restoring SDK project NuGet packages...");
            DotNetRestore(s => s
                .SetProjectFile(SdkProject)
                .SetSources(NuGetSources));

            Logger.Normal("Restoring SDK test project NuGet packages...");
            DotNetRestore(s => s
                .SetProjectFile(SdkTestProject)
                .SetSources(NuGetSources));

            Logger.Normal($"Updating NuGet package InRule.Repository v{InRuleVersion} for SDK project.");
            DotNet($"add {SdkProject} package InRule.Repository --no-restore --version {InRuleVersion}");

            if (IsWin)
            {
                Logger.Normal("Restoring Authoring project NuGet packages...");
                NuGetRestore(s => s
                    .SetTargetPath(AuthoringProject)
                    .SetProcessWorkingDirectory(AuthoringProject.Directory)
                    .SetPackagesDirectory(RootDirectory / "packages")
                    .SetSource(NuGetSources));

                Logger.Normal($"Update NuGet package InRule.Authoring.SDK v{InRuleVersion} for Authoring project.");
                NuGetTasks.NuGet(
                    $"update {AuthoringProject} -Id InRule.Authoring.SDK -RepositoryPath {RootDirectory / "packages"} -Source {string.Join(';', NuGetSources)} -Version {InRuleVersion}",
                    workingDirectory: AuthoringProject.Directory);
            }
        });

    Target Compile => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            DotNetBuild(s => s
                .SetProjectFile(SdkProject)
                .SetVersion(GitVersion.FullSemVer)
                .SetAssemblyVersion(GitVersion.AssemblySemVer)
                .SetFileVersion(GitVersion.AssemblySemFileVer)
                .SetInformationalVersion(GitVersion.InformationalVersion)
                .SetConfiguration(Configuration)
                .EnableNoRestore()
                .SetFramework(IsWin ? null : "netstandard2.0"));

            DotNetBuild(s => s
                .SetProjectFile(SdkTestProject)
                .SetVersion(GitVersion.FullSemVer)
                .SetAssemblyVersion(GitVersion.AssemblySemVer)
                .SetFileVersion(GitVersion.AssemblySemFileVer)
                .SetInformationalVersion(GitVersion.InformationalVersion)
                .SetConfiguration(Configuration)
                .EnableNoRestore()
                .SetFramework(IsWin ? null : "net5.0"));

            if (IsWin)
            {
                MSBuild(s => s
                    .SetTargetPath(AuthoringProject)
                    .SetConfiguration(Configuration)
                    .SetAssemblyVersion(GitVersion.AssemblySemVer)
                    .SetFileVersion(GitVersion.AssemblySemFileVer)
                    .SetInformationalVersion(GitVersion.InformationalVersion)
                    .DisableRestore());
            }
        });

    Target Test => _ => _
        .DependsOn(CleanTestResults)
        .DependsOn(Compile)
        .Executes(() =>
        {
            DotNetTest(s => s
                .SetProjectFile(SdkTestProject)
                .SetFramework("net5.0")
                .SetConfiguration(Configuration)
                .EnableNoBuild()
                .EnableNoRestore()
                .SetLogger("trx;LogFileName=./net5.0/TestResult.trx"));

            if (IsWin)
            {
                DotNetTest(s => s
                    .SetProjectFile(SdkTestProject)
                    .SetFramework("net461")
                    .SetConfiguration(Configuration)
                    .EnableNoBuild()
                    .EnableNoRestore()
                    .SetLogger("trx;LogFileName=./net461/TestResult.trx"));

                DotNetTest(s => s
                    .SetProjectFile(SdkTestProject)
                    .SetFramework("net472")
                    .SetConfiguration(Configuration)
                    .EnableNoBuild()
                    .EnableNoRestore()
                    .SetLogger("trx;LogFileName=./net472/TestResult.trx"));
            }
        });

    Target PublishArtifacts => _ => _
        .DependsOn(CleanArtifacts)
        .DependsOn(Compile)
        .Executes(() =>
        {
            SourceDirectory.GlobFiles($"**/{Configuration}/**/Sknet.InRuleGitStorage.{GitVersion.SemVer}.*nupkg").ForEach(file => CopyFileToDirectory(file, ArtifactsDirectory));

            if (IsWin)
            {
                var authoringExtensionDirectory = ArtifactsDirectory / "Sknet.InRuleGitStorage.AuthoringExtension";

                AuthoringProject.Directory.GlobFiles($"bin/{Configuration}/Sknet.InRuleGitStorage.*").ForEach(file => CopyFileToDirectory(file, authoringExtensionDirectory));
                AuthoringProject.Directory.GlobFiles($"bin/{Configuration}/LibGit2Sharp.*").ForEach(file => CopyFileToDirectory(file, authoringExtensionDirectory));
                AuthoringProject.Directory.GlobDirectories($"bin/{Configuration}/lib/win32").ForEach(directory => CopyDirectoryRecursively(directory, authoringExtensionDirectory / "lib" / "win32"));
                authoringExtensionDirectory.GlobFiles("**/*.xml").ForEach(DeleteFile);

                CompressZip(authoringExtensionDirectory, ArtifactsDirectory / $"Sknet.InRuleGitStorage.AuthoringExtension.{GitVersion.SemVer}.zip");

                DeleteDirectory(authoringExtensionDirectory);
            }
        });
    
    Target DeployToIrAuthor => _ => _
        .DependsOn(PublishArtifacts)
        .Executes(() =>
        {
            var irAuthorLocalDirectory = (AbsolutePath) $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}/InRule/irAuthor";

            if (DirectoryExists(irAuthorLocalDirectory))
            {
                var extensionDirectory = irAuthorLocalDirectory / "ExtensionExchange" / "Sknet.InRuleGitStorage";
                EnsureCleanDirectory(extensionDirectory);

                UncompressZip(ArtifactsDirectory / $"Sknet.InRuleGitStorage.AuthoringExtension.{GitVersion.SemVer}.zip", extensionDirectory);
            }
        });

    Target PublishToGitHub => _ => _
        .DependsOn(PublishArtifacts)
        .Executes(() =>
        {

        });

    Target PublishToNuGetFeed => _ => _
        .DependsOn(PublishArtifacts)
        .Executes(() =>
        {
            
        });

    Target Default => _ => _
        .DependsOn(Test)
        .DependsOn(DeployToIrAuthor);

    Target CI => _ => _
        .DependsOn(Clean)
        .DependsOn(Test)
        .DependsOn(PublishArtifacts);

    Target Release => _ => _
        .DependsOn(Clean)
        .DependsOn(Test)
        .DependsOn(PublishToGitHub)
        .DependsOn(PublishToNuGetFeed);
}
