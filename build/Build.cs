using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
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
using static Nuke.Common.Tools.GitHub.GitHubTasks;

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
    readonly string NuGetSource = "https://www.myget.org/F/stevenkuhn/api/v2/package";

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

    string GitHubRepositoryName = "InRuleGitStorage";
    string GitHubRepositoryOwner = "stevenkuhn";

    string[] NuGetRestoreSources = new[] {
        "https://api.nuget.org/v3/index.json",
        "https://www.myget.org/F/stevenkuhn/api/v3/index.json"
    };

    protected override void OnBuildInitialized()
    {
        Logger.Info($"GitVersion settings:\n{JsonConvert.SerializeObject(GitVersion, Formatting.Indented)}");
    }

    Target Clean => _ => _
        .Before(RestoreSdk)
        .Before(RestoreAuthoring)
        .Executes(() =>
        {
            Logger.Normal("Deleting all bin/obj directories...");
            SourceDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);
            TestsDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);

            Logger.Normal("Cleaning artifacts directory...");
            EnsureCleanDirectory(ArtifactsDirectory);

            Logger.Normal("Deleting test results directories...");
            TestsDirectory.GlobDirectories("**/TestResults").ForEach(DeleteDirectory);
        });

    Target RestoreSdk => _ => _
        .DependsOn(Clean)
        .Requires(() => InRuleVersion)
        .Executes(() =>
        {
            Logger.Normal("Restoring SDK project NuGet packages...");
            DotNetRestore(s => s
                .SetProjectFile(SdkProject)
                .SetSources(NuGetRestoreSources));

            Logger.Normal("Restoring SDK test project NuGet packages...");
            DotNetRestore(s => s
                .SetProjectFile(SdkTestProject)
                .SetSources(NuGetRestoreSources));

            Logger.Normal($"Updating NuGet package InRule.Repository v{InRuleVersion} for SDK project.");
            DotNet($"add {SdkProject} package InRule.Repository --no-restore --version {InRuleVersion}");
        });
    
    Target RestoreAuthoring => _ => _
        .DependsOn(Clean)
        .Requires(() => InRuleVersion)
        .OnlyWhenStatic(() => IsWin)   
        .Executes(() =>
        {
            Logger.Normal("Restoring Authoring project NuGet packages...");
                NuGetRestore(s => s
                    .SetTargetPath(AuthoringProject)
                    .SetProcessWorkingDirectory(AuthoringProject.Directory)
                    .SetPackagesDirectory(RootDirectory / "packages")
                    .SetSource(NuGetRestoreSources));

            Logger.Normal($"Update NuGet package InRule.Authoring.SDK v{InRuleVersion} for Authoring project.");
            NuGetTasks.NuGet(
                $"update {AuthoringProject} -Id InRule.Authoring.SDK -RepositoryPath {RootDirectory / "packages"} -Source {string.Join(';', NuGetRestoreSources)} -Version {InRuleVersion}",
                workingDirectory: AuthoringProject.Directory);
        });

    Target CompileSdk => _ => _
        .DependsOn(RestoreSdk)
        .Executes(() =>
        {
            Logger.Normal("Compiling SDK project...");
            DotNetBuild(s => s
                .SetProjectFile(SdkProject)
                .SetVersion(GitVersion.FullSemVer)
                .SetAssemblyVersion(GitVersion.AssemblySemVer)
                .SetFileVersion(GitVersion.AssemblySemFileVer)
                .SetInformationalVersion(GitVersion.InformationalVersion)
                .SetConfiguration(Configuration)
                .EnableNoRestore()
                .SetFramework(IsWin ? null : "netstandard2.0"));

            Logger.Normal("Compiling SDK test project...");
            DotNetBuild(s => s
                .SetProjectFile(SdkTestProject)
                .SetVersion(GitVersion.FullSemVer)
                .SetAssemblyVersion(GitVersion.AssemblySemVer)
                .SetFileVersion(GitVersion.AssemblySemFileVer)
                .SetInformationalVersion(GitVersion.InformationalVersion)
                .SetConfiguration(Configuration)
                .EnableNoRestore()
                .SetFramework(IsWin ? null : "net5.0"));
        });

    Target CompileAuthoring => _ => _
        .DependsOn(RestoreAuthoring)
        .OnlyWhenStatic(() => IsWin)
        .After(CompileSdk)
        .Before(TestSdk)
        .Executes(() => 
        {
            Logger.Normal("Compiling Authoring project...");
            MSBuild(s => s
                .SetTargetPath(AuthoringProject)
                .SetConfiguration(Configuration)
                .SetAssemblyVersion(GitVersion.AssemblySemVer)
                .SetFileVersion(GitVersion.AssemblySemFileVer)
                .SetInformationalVersion(GitVersion.InformationalVersion)
                .DisableRestore());
        });

    Target TestSdk => _ => _
        .DependsOn(CompileSdk)
        .Executes(() =>
        {
            Logger.Normal("Running SDK tests under .NET 5.0 runtime...");
            DotNetTest(s => s
                .SetProjectFile(SdkTestProject)
                .SetFramework("net5.0")
                .SetConfiguration(Configuration)
                .EnableNoBuild()
                .EnableNoRestore()
                .SetLogger("trx;LogFileName=./net5.0/TestResult.trx"));

            if (IsWin)
            {
                Logger.Normal("Running SDK tests under .NET Framework 4.6.1 runtime...");
                DotNetTest(s => s
                    .SetProjectFile(SdkTestProject)
                    .SetFramework("net461")
                    .SetConfiguration(Configuration)
                    .EnableNoBuild()
                    .EnableNoRestore()
                    .SetLogger("trx;LogFileName=./net461/TestResult.trx"));

                Logger.Normal("Running SDK tests under .NET Framework 4.7.2 runtime...");
                DotNetTest(s => s
                    .SetProjectFile(SdkTestProject)
                    .SetFramework("net472")
                    .SetConfiguration(Configuration)
                    .EnableNoBuild()
                    .EnableNoRestore()
                    .SetLogger("trx;LogFileName=./net472/TestResult.trx"));
            }
        });

    Target PublishSdkArtifacts => _ => _
        .DependsOn(CompileSdk)
        .After(TestSdk)
        .Executes(() =>
        {
            Logger.Normal("Publishing SDK artifacts to the artifacts folder...");
            SourceDirectory.GlobFiles($"**/{Configuration}/**/Sknet.InRuleGitStorage.{GitVersion.SemVer}.*nupkg").ForEach(file => CopyFileToDirectory(file, ArtifactsDirectory));
        });

    Target PublishAuthoringArtifacts => _ => _
        .DependsOn(CompileAuthoring)
        .OnlyWhenStatic(() => IsWin)
        .After(TestSdk)
        .Executes(() =>
        {
            Logger.Normal("Publishing Authoring artifacts to the artifacts folder...");
            var authoringExtensionDirectory = ArtifactsDirectory / "Sknet.InRuleGitStorage.AuthoringExtension";

            AuthoringProject.Directory.GlobFiles($"bin/{Configuration}/Sknet.InRuleGitStorage.*").ForEach(file => CopyFileToDirectory(file, authoringExtensionDirectory));
            AuthoringProject.Directory.GlobFiles($"bin/{Configuration}/LibGit2Sharp.*").ForEach(file => CopyFileToDirectory(file, authoringExtensionDirectory));
            AuthoringProject.Directory.GlobDirectories($"bin/{Configuration}/lib/win32").ForEach(directory => CopyDirectoryRecursively(directory, authoringExtensionDirectory / "lib" / "win32"));
            authoringExtensionDirectory.GlobFiles("**/*.xml").ForEach(DeleteFile);

            CompressZip(authoringExtensionDirectory, ArtifactsDirectory / $"Sknet.InRuleGitStorage.AuthoringExtension.{GitVersion.SemVer}.zip");

            DeleteDirectory(authoringExtensionDirectory);
        });
    
    Target DeployToIrAuthor => _ => _
        .DependsOn(PublishAuthoringArtifacts)
        .OnlyWhenStatic(() => IsWin)
        .OnlyWhenStatic(() => IsLocalBuild)
        .Executes(() =>
        {
            Logger.Normal("Deploying Authoring extension to local irAuthor Extension Exchange folder (if exists)...");
            var irAuthorLocalDirectory = (AbsolutePath) $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}/InRule/irAuthor";

            if (DirectoryExists(irAuthorLocalDirectory))
            {
                var extensionDirectory = irAuthorLocalDirectory / "ExtensionExchange" / "Sknet.InRuleGitStorage";
                EnsureCleanDirectory(extensionDirectory);

                UncompressZip(ArtifactsDirectory / $"Sknet.InRuleGitStorage.AuthoringExtension.{GitVersion.SemVer}.zip", extensionDirectory);
            }
        });

    Target PublishToGitHub => _ => _
        .DependsOn(PublishSdkArtifacts)
        .DependsOn(PublishAuthoringArtifacts)
        .Requires(() => GitHubAccessToken)
        .Executes(async () =>
        {
            Logger.Normal($"Creating 'v{GitVersion.SemVer}' release in GitHub...");
            var github = new Octokit.GitHubClient(new Octokit.ProductHeaderValue("sknet.inrulegitstorage.build"))
            {
                Credentials = new Octokit.Credentials(GitHubAccessToken)
            };

            Octokit.Release release = null;
            try
            {
                Logger.Info($"Retrieving exisiting release tagged as 'v{GitVersion.SemVer}'...");
                release = await github.Repository.Release.Get(GitHubRepositoryOwner, GitHubRepositoryName, $"v{GitVersion.SemVer}");
            }
            catch(Octokit.NotFoundException)
            {
                Logger.Info("Release not found. Retrieving existing draft release...");
                var releases = await github.Repository.Release.GetAll(GitHubRepositoryOwner, GitHubRepositoryName);
                release = releases.SingleOrDefault(r => r.Draft && (r.TagName == $"v{GitVersion.SemVer}" || r.TagName.StartsWith($"v{GitVersion.MajorMinorPatch}-{GitVersion.PreReleaseLabel}")));
            }

            if (release != null)
            {
                Logger.Info($"Release '{release.Name}' found. Updating release...");
                release = await github.Repository.Release.Edit(GitHubRepositoryOwner, GitHubRepositoryName, release.Id, new Octokit.ReleaseUpdate
                {
                    Name = $"v{GitVersion.SemVer}",
                    TagName = $"v{GitVersion.SemVer}",
                    Body = !string.IsNullOrWhiteSpace(release.Body)
                        ? release.Body
                        : $"Release notes for `v{GitVersion.SemVer}` are not available at this time.",
                    Draft = release.Draft,
                    Prerelease = !string.IsNullOrWhiteSpace(GitVersion.PreReleaseTag),
                    TargetCommitish = GitRepository.Commit
                });
            }
            else
            {
                Logger.Info($"Release not found. Creating a new draft release...");
                release = await github.Repository.Release.Create(GitHubRepositoryOwner, GitHubRepositoryName, new Octokit.NewRelease($"v{GitVersion.SemVer}")
                {
                    Name = $"v{GitVersion.SemVer}",
                    Body = $"Release notes for `v{GitVersion.SemVer}` are not available at this time.",
                    Draft = true,
                    Prerelease = !string.IsNullOrWhiteSpace(GitVersion.PreReleaseTag),
                    TargetCommitish = GitRepository.Commit
                });
            }

            Logger.Info("Removing existing assets (if any)...");
            var assets = await github.Repository.Release.GetAllAssets(GitHubRepositoryOwner, GitHubRepositoryName, release.Id);
            foreach (var asset in assets)
            {
                await github.Repository.Release.DeleteAsset(GitHubRepositoryOwner, GitHubRepositoryName, asset.Id);
            }

            var artifacts = ArtifactsDirectory.GlobFiles($"Sknet.InRuleGitStorage*.{GitVersion.SemVer}.*");
            foreach (var artifact in artifacts)
            {
                var file = new FileInfo(artifact);
                using var stream = File.OpenRead(artifact);

                Logger.Info($"Uploading asset '{file.Name}'...");
                var asset = await github.Repository.Release.UploadAsset(release, new Octokit.ReleaseAssetUpload()
                {
                    ContentType = "application/zip",
                    FileName = file.Name,
                    RawData = stream,
                });
            }
        });

    Target PublishToNuGetFeed => _ => _
        .DependsOn(PublishSdkArtifacts)
        .Requires(() => NuGetSource)
        .Requires(() => NuGetApiKey)
        .After(PublishToGitHub)
        .Executes(() =>
        {
            Logger.Normal($"Uploading NuGet package(s) to '{NuGetSource}'...");

            NuGetPush(s => s
                .SetApiKey(NuGetApiKey)
                .SetSource(NuGetSource)
                .SetTargetPath(ArtifactsDirectory / $"Sknet.InRuleGitStorage.{GitVersion.SemVer}.nupkg")); 
        });

    Target Default => _ => _
        .DependsOn(TestSdk)
        .DependsOn(PublishSdkArtifacts)
        .DependsOn(DeployToIrAuthor);
}
