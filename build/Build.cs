[ShutdownDotNetAfterServerBuild]
class Build : NukeBuild
{
    public static int Main() => Execute<Build>(x => x.Default);

    [Parameter("Configuration to build. Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Parameter("Version of the InRule Repository SDK use. Default is 5.6.0.", Name = "inrule-version")]
    readonly string InRuleVersion = "5.6.0";

    [Parameter("GitHub access token used for creating a new or updating an existing release.")]
    [Secret]
    readonly string GitHubAccessToken;

    [Parameter("GitHub repository owner and name used for creating a new or updating an existing release. For example: 'stevenkuhn/InRuleGitStorage'.")]
    readonly string GitHubRepository = "stevenkuhn/InRuleGitStorage";

    [Parameter("NuGet source used for pushing the Sdk NuGet package. Default is NuGet.org.")]
    readonly string NuGetSource = "https://api.nuget.org/v3/index.json";

    [Parameter("NuGet API key used to pushing the Sdk NuGet package.")]
    [Secret]
    readonly string NuGetApiKey;

    [Solution] readonly Solution Solution;
    [GitRepository] readonly GitRepository GitRepository;
    [GitVersion(Framework = "net8.0", NoFetch = true)] readonly GitVersion GitVersion;

    static AbsolutePath SourceDirectory => RootDirectory / "src";
    static AbsolutePath TestsDirectory => RootDirectory / "test";
    static AbsolutePath ArtifactsDirectory => RootDirectory / "artifacts";

    SolutionFolder SourceFolder => Solution.GetSolutionFolder("src");
    SolutionFolder TestFolder => Solution.GetSolutionFolder("test");

    Project AuthoringProject => SourceFolder.GetProject("Sknet.InRuleGitStorage.AuthoringExtension");
    Project SdkProject => SourceFolder.GetProject("Sknet.InRuleGitStorage");
    Project SdkTestProject => TestFolder.GetProject("Sknet.InRuleGitStorage.Tests");

    readonly string[] NuGetRestoreSources = new[] {
        "https://api.nuget.org/v3/index.json"
    };

    protected override void OnBuildInitialized()
    {
        Log.Information($"GitVersion settings:\n{JsonConvert.SerializeObject(GitVersion, Formatting.Indented)}");
    }

    Target Clean => _ => _
        .Before(RestoreSdk)
        .Before(RestoreAuthoring)
        .Executes(() =>
        {
            Log.Debug("Deleting all bin/obj directories...");
            SourceDirectory.GlobDirectories("**/bin", "**/obj").DeleteDirectories();
            TestsDirectory.GlobDirectories("**/bin", "**/obj").DeleteDirectories();

            Log.Debug("Cleaning artifacts directory...");
            ArtifactsDirectory.CreateOrCleanDirectory();

            Log.Debug("Deleting test results directories...");
            TestsDirectory.GlobDirectories("**/TestResults").DeleteDirectories();
        });

    Target RestoreSdk => _ => _
        .DependsOn(Clean)
        .Requires(() => InRuleVersion)
        .Executes(() =>
        {
            Log.Debug($"Updating NuGet package InRule.Repository v{InRuleVersion} for SDK project.");
            DotNet($"add {SdkProject} package InRule.Repository --no-restore --version {InRuleVersion}");

            Log.Debug("Restoring SDK project NuGet packages...");
            DotNetRestore(s => s
                .SetProjectFile(SdkProject)
                .SetSources(NuGetRestoreSources));

            Log.Debug("Restoring SDK test project NuGet packages...");
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
            Log.Debug($"Update NuGet package InRule.Authoring.SDK v{InRuleVersion} for Authoring project.");
            DotNet($"add {AuthoringProject} package InRule.Authoring.SDK --no-restore --version {InRuleVersion}");

            Log.Debug("Restoring Authoring project NuGet packages...");
            DotNetRestore(s => s
                .SetProjectFile(AuthoringProject)
                .SetSources(NuGetRestoreSources));
        });

    Target CompileSdk => _ => _
        .DependsOn(RestoreSdk)
        .Executes(() =>
        {
            Log.Debug("Compiling SDK project...");
            DotNetBuild(s => s
                .SetProjectFile(SdkProject)
                .SetVersion(GitVersion.FullSemVer)
                .SetAssemblyVersion(GitVersion.AssemblySemVer)
                .SetFileVersion(GitVersion.AssemblySemFileVer)
                .SetInformationalVersion(GitVersion.InformationalVersion)
                .SetProperty("RepositoryBranch", GitVersion.BranchName)
                .SetProperty("RepositoryCommit", GitVersion.Sha)
                .SetConfiguration(Configuration)
                .SetFramework(IsWin ? null : "net8.0")
                .EnableNoRestore());

            Log.Debug("Compiling SDK test project...");
            DotNetBuild(s => s
                .SetProjectFile(SdkTestProject)
                .SetVersion(GitVersion.FullSemVer)
                .SetAssemblyVersion(GitVersion.AssemblySemVer)
                .SetFileVersion(GitVersion.AssemblySemFileVer)
                .SetInformationalVersion(GitVersion.InformationalVersion)
                .SetConfiguration(Configuration)
                .SetFramework(IsWin ? null : "net8.0")
                .EnableNoRestore());
        });

    Target CompileAuthoring => _ => _
        .DependsOn(RestoreAuthoring)
        .OnlyWhenStatic(() => IsWin)
        .After(CompileSdk)
        .Before(TestSdk)
        .Executes(() =>
        {
            Log.Debug("Compiling Authoring project...");
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
            Log.Debug("Running SDK tests under .NET 8.0 runtime...");
            DotNetTest(s => s
                .SetProjectFile(SdkTestProject)
                .SetFramework("net8.0")
                .SetConfiguration(Configuration)
                .EnableNoBuild()
                .EnableNoRestore()
                .AddLoggers("trx;LogFileName=./net6.0/TestResult.trx"));

            if (IsWin)
            { 
                Log.Debug("Running SDK tests under .NET Framework 4.7.2 runtime...");
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
            Log.Debug("Publishing SDK artifacts to the artifacts folder...");
            SourceDirectory
                .GlobFiles($"**/{Configuration}/**/Sknet.*.{GitVersion.SemVer}.*nupkg")
                .ForEach(file => file.CopyToDirectory(ArtifactsDirectory));
        });

    Target PublishAuthoringArtifacts => _ => _
        .DependsOn(CompileAuthoring)
        .OnlyWhenStatic(() => IsWin)
        .After(TestSdk)
        .Executes(() =>
        {
            Log.Debug("Publishing Authoring artifacts to the artifacts folder...");
            var authoringExtensionDirectory = ArtifactsDirectory / "Sknet.InRuleGitStorage.AuthoringExtension";

            AuthoringProject.Directory.GlobFiles($"bin/{Configuration}/net472/Sknet.InRuleGitStorage.*").ForEach(file => file.CopyToDirectory(authoringExtensionDirectory));
            AuthoringProject.Directory.GlobFiles($"bin/{Configuration}/net472/LibGit2Sharp.*").ForEach(file => file.CopyToDirectory(authoringExtensionDirectory));
            AuthoringProject.Directory.GlobDirectories($"bin/{Configuration}/net472/lib/win32/x64").ForEach(directory => directory.Copy(authoringExtensionDirectory / "lib" / "win32" / "x64"));
            authoringExtensionDirectory.GlobFiles("**/*.xml").DeleteFiles();

            authoringExtensionDirectory.ZipTo(ArtifactsDirectory / $"Sknet.InRuleGitStorage.AuthoringExtension.{GitVersion.SemVer}.zip");

            authoringExtensionDirectory.DeleteDirectory();
        });

    Target DeployToIrAuthor => _ => _
        .DependsOn(PublishAuthoringArtifacts)
        .OnlyWhenStatic(() => IsWin)
        .OnlyWhenStatic(() => IsLocalBuild)
        .Executes(() =>
        {
            Log.Debug("Deploying Authoring extension to local irAuthor Extension Exchange folder (if exists)...");
            var irAuthorLocalDirectory = (AbsolutePath)$"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}/InRule/irAuthor";

            if (irAuthorLocalDirectory.DirectoryExists())
            {
                var extensionDirectory = irAuthorLocalDirectory / "ExtensionExchange" / "Sknet.InRuleGitStorage";
                extensionDirectory.CreateOrCleanDirectory();

                (ArtifactsDirectory / $"Sknet.InRuleGitStorage.AuthoringExtension.{GitVersion.SemVer}.zip").UnZipTo(extensionDirectory);
            }
        });

    Target PublishToGitHub => _ => _
        .DependsOn(PublishSdkArtifacts)
        .DependsOn(PublishAuthoringArtifacts)
        .Requires(() => GitHubAccessToken)
        .Executes(async () =>
        {
            Log.Debug($"Creating 'v{GitVersion.SemVer}' release in GitHub...");
            (string repositoryOwner, string repositoryName) = GitHubRepository.Split('/');

            var github = new Octokit.GitHubClient(new Octokit.ProductHeaderValue("sknet.inrulegitstorage.build"))
            {
                Credentials = new Octokit.Credentials(GitHubAccessToken)
            };

            Octokit.Release release = null;
            try
            {
                Log.Information($"Retrieving exisiting release tagged as 'v{GitVersion.SemVer}'...");
                release = await github.Repository.Release.Get(repositoryOwner, repositoryName, $"v{GitVersion.SemVer}");
            }
            catch (Octokit.NotFoundException)
            {
                Log.Information("Release not found. Retrieving existing draft release...");
                var releases = await github.Repository.Release.GetAll(repositoryOwner, repositoryName);
                release = releases.SingleOrDefault(r => r.Draft && (r.TagName == $"v{GitVersion.SemVer}" || r.TagName.StartsWith($"v{GitVersion.MajorMinorPatch}-{GitVersion.PreReleaseLabel}")));
            }

            if (release != null)
            {
                Log.Information($"Release '{release.Name}' found. Updating release...");
                release = await github.Repository.Release.Edit(repositoryOwner, repositoryName, release.Id, new Octokit.ReleaseUpdate
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
                Log.Information($"Release not found. Creating a new draft release...");
                release = await github.Repository.Release.Create(repositoryOwner, repositoryName, new Octokit.NewRelease($"v{GitVersion.SemVer}")
                {
                    Name = $"v{GitVersion.SemVer}",
                    Body = $"Release notes for `v{GitVersion.SemVer}` are not available at this time.",
                    Draft = true,
                    Prerelease = !string.IsNullOrWhiteSpace(GitVersion.PreReleaseTag),
                    TargetCommitish = GitRepository.Commit
                });
            }

            Log.Information("Removing existing assets (if any)...");
            var assets = await github.Repository.Release.GetAllAssets(repositoryOwner, repositoryName, release.Id);
            foreach (var asset in assets)
            {
                await github.Repository.Release.DeleteAsset(repositoryOwner, repositoryName, asset.Id);
            }

            var artifacts = ArtifactsDirectory.GlobFiles($"Sknet.*.{GitVersion.SemVer}.*");
            foreach (var artifact in artifacts)
            {
                var file = new FileInfo(artifact);
                using var stream = File.OpenRead(artifact);

                Log.Information($"Uploading asset '{file.Name}'...");
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
            Log.Debug($"Uploading NuGet package(s) to '{NuGetSource}'...");

            DotNetNuGetPush(s => s
                .SetApiKey(NuGetApiKey)
                .SetSkipDuplicate(true)
                .SetSource(NuGetSource)
                .SetTargetPath(ArtifactsDirectory / $"Sknet.*.{GitVersion.SemVer}.nupkg"));
        });

    Target Default => _ => _
        .DependsOn(TestSdk)
        .DependsOn(PublishSdkArtifacts)
        .DependsOn(DeployToIrAuthor);
}