﻿namespace Sknet.InRuleGitStorage.Tests.Fixtures;

public class TemporaryDirectoryFixture : IDisposable
{
    public string DirectoryPath { get; }

    public TemporaryDirectoryFixture()
    {
        DirectoryPath = Path.Combine(Environment.CurrentDirectory, "Data", $"repo-{Guid.NewGuid().ToString("N")}");
        Directory.CreateDirectory(DirectoryPath);
    }

    public void Dispose()
    {
        DeleteDirectoryPath();
    }

    private void DeleteDirectoryPath()
    {
        if (!string.IsNullOrWhiteSpace(DirectoryPath) && Directory.Exists(DirectoryPath))
        {
            var directoryInfo = new DirectoryInfo(DirectoryPath);
            foreach (var file in directoryInfo.GetFiles("*", SearchOption.AllDirectories))
            {
                file.Attributes &= ~FileAttributes.ReadOnly;
            }

            directoryInfo.Attributes &= ~FileAttributes.ReadOnly;

            Directory.Delete(DirectoryPath, true);
        }
    }
}

public class GitRepositoryFixture : IDisposable
{
    private readonly string _repositoryPath;
    public IRepository Repository { get; }

    public GitRepositoryFixture() : this(true)
    {

    }

    public GitRepositoryFixture(bool isBare)
    {
        try
        {
            _repositoryPath = Path.Combine(Environment.CurrentDirectory, "Data", $"repo-{Guid.NewGuid().ToString("N")}");
            Directory.CreateDirectory(_repositoryPath);
            LibGit2Sharp.Repository.Init(_repositoryPath, isBare);
            Repository = new LibGit2Sharp.Repository(_repositoryPath);
        }
        catch
        {
            DeleteRepositoryPath();

            throw;
        }
    }

    public void Dispose()
    {
        Repository.Dispose();
        DeleteRepositoryPath();
    }

    private void DeleteRepositoryPath()
    {
        if (!string.IsNullOrWhiteSpace(_repositoryPath) && Directory.Exists(_repositoryPath))
        {
            var directoryInfo = new DirectoryInfo(_repositoryPath);
            foreach (var file in directoryInfo.GetFiles("*", SearchOption.AllDirectories))
            {
                file.Attributes &= ~FileAttributes.ReadOnly;
            }

            directoryInfo.Attributes &= ~FileAttributes.ReadOnly;

            Directory.Delete(_repositoryPath, true);
        }
    }
}
