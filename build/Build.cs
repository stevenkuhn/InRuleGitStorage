[CheckBuildProjectConfigurations]
[ShutdownDotNetAfterServerBuild]
class Build : NukeBuild
{
    public static int Main() => Execute<Build>(x => x.Default);

    [Parameter("Configuration to build. Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Parameter("Version of the InRule Repository SDK use. Default is 5.2.0.", Name = "inrule-version")]
    readonly string InRuleVersion = "5.2.0";

    [Parameter("GitHub access token used for creating a new or updating an existing release.")]
    readonly string GitHubAccessToken;

    [Parameter("NuGet source used for pushing the Sdk NuGet package. Default is NuGet.org.")]
    readonly string NuGetSource = "https://api.nuget.org/v3/index.json";

    [Parameter("NuGet API key used to pushing the Sdk NuGet package.")]
    readonly string NuGetApiKey;

    [Solution] readonly Solution Solution;
    [GitRepository] readonly GitRepository GitRepository;
    [GitVersion(Framework = "net6.0", NoFetch = true)] readonly GitVersion GitVersion;

    static AbsolutePath SourceDirectory => RootDirectory / "src";
    static AbsolutePath TestsDirectory => RootDirectory / "test";
    static AbsolutePath ArtifactsDirectory => RootDirectory / "artifacts";

    Project AuthoringProject => Solution.GetProject("Sknet.InRuleGitStorage.AuthoringExtension");
    Project SdkProject => Solution.GetProject("Sknet.InRuleGitStorage");
    Project SdkTestProject => Solution.GetProject("Sknet.InRuleGitStorage.Tests");

    const string GitHubRepositoryName = "InRuleGitStorage";
    const string GitHubRepositoryOwner = "stevenkuhn";

    readonly string[] NuGetRestoreSources = new[] {
        "https://api.nuget.org/v3/index.json"
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
            Logger.Normal($"Updating NuGet package InRule.Repository v{InRuleVersion} for SDK project.");
            DotNet($"add {SdkProject} package InRule.Repository --no-restore --version {InRuleVersion}");

            Logger.Normal("Restoring SDK project NuGet packages...");
            DotNetRestore(s => s
                .SetProjectFile(SdkProject)
                .SetSources(NuGetRestoreSources));

            Logger.Normal("Restoring SDK test project NuGet packages...");
            DotNetRestore(s => s
                .SetProjectFile(SdkTestProject)
                .SetSources(NuGetRestoreSources));
        });

    Target RestoreAuthoring => _ => _
        .DependsOn(Clean)
        .Requires(() => InRuleVersion)
        .OnlyWhenStatic(() => IsWin)
        .Executes(() =>
        {
            Logger.Normal($"Update NuGet package InRule.Authoring.SDK v{InRuleVersion} for Authoring project.");
            DotNet($"add {AuthoringProject} package InRule.Authoring.SDK --no-restore --version {InRuleVersion}");

            Logger.Normal("Restoring Authoring project NuGet packages...");
            DotNetRestore(s => s
                .SetProjectFile(AuthoringProject)
                .SetSources(NuGetRestoreSources));
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
                .SetProperty("RepositoryBranch", GitVersion.BranchName)
                .SetProperty("RepositoryCommit", GitVersion.Sha)
                .SetConfiguration(Configuration)
                .SetFramework(IsWin ? null : "netstandard2.0")
                .EnableNoRestore());

            Logger.Normal("Compiling SDK test project...");
            DotNetBuild(s => s
                .SetProjectFile(SdkTestProject)
                .SetVersion(GitVersion.FullSemVer)
                .SetAssemblyVersion(GitVersion.AssemblySemVer)
                .SetFileVersion(GitVersion.AssemblySemFileVer)
                .SetInformationalVersion(GitVersion.InformationalVersion)
                .SetConfiguration(Configuration)
                .SetFramework(IsWin ? null : "net6.0")
                .EnableNoRestore());
        });

    Target CompileAuthoring => _ => _
        .DependsOn(RestoreAuthoring)
        .OnlyWhenStatic(() => IsWin)
        .After(CompileSdk)
        .Before(TestSdk)
        .Executes(() =>
        {
            Logger.Normal("Compiling Authoring project...");
            DotNetBuild(s => s
                .SetProjectFile(AuthoringProject)
                .SetVersion(GitVersion.FullSemVer)
                .SetAssemblyVersion(GitVersion.AssemblySemVer)
                .SetFileVersion(GitVersion.AssemblySemFileVer)
                .SetInformationalVersion(GitVersion.InformationalVersion)
                .SetConfiguration(Configuration)
                .EnableNoRestore());
        });

    Target TestSdk => _ => _
        .DependsOn(CompileSdk)
        .Executes(() =>
        {
            Logger.Normal("Running SDK tests under .NET 6.0 runtime...");
            DotNetTest(s => s
                .SetProjectFile(SdkTestProject)
                .SetFramework("net6.0")
                .SetConfiguration(Configuration)
                .EnableNoBuild()
                .EnableNoRestore()
                .AddLoggers("trx;LogFileName=./net6.0/TestResult.trx"));

            if (IsWin)
            {
                Logger.Normal("Running SDK tests under .NET Framework 4.6.1 runtime...");
                DotNetTest(s => s
                    .SetProjectFile(SdkTestProject)
                    .SetFramework("net461")
                    .SetConfiguration(Configuration)
                    .EnableNoBuild()
                    .EnableNoRestore()
                    .AddLoggers("trx;LogFileName=./net461/TestResult.trx"));

                Logger.Normal("Running SDK tests under .NET Framework 4.7.2 runtime...");
                DotNetTest(s => s
                    .SetProjectFile(SdkTestProject)
                    .SetFramework("net472")
                    .SetConfiguration(Configuration)
                    .EnableNoBuild()
                    .EnableNoRestore()
                    .AddLoggers("trx;LogFileName=./net472/TestResult.trx"));
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

            AuthoringProject.Directory.GlobFiles($"bin/{Configuration}/net472/Sknet.InRuleGitStorage.*").ForEach(file => CopyFileToDirectory(file, authoringExtensionDirectory));
            AuthoringProject.Directory.GlobFiles($"bin/{Configuration}/net472/LibGit2Sharp.*").ForEach(file => CopyFileToDirectory(file, authoringExtensionDirectory));
            AuthoringProject.Directory.GlobDirectories($"bin/{Configuration}/net472/lib/win32/x64").ForEach(directory => CopyDirectoryRecursively(directory, authoringExtensionDirectory / "lib" / "win32" / "x64"));
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
            var irAuthorLocalDirectory = (AbsolutePath)$"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}/InRule/irAuthor";

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
            catch (Octokit.NotFoundException)
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