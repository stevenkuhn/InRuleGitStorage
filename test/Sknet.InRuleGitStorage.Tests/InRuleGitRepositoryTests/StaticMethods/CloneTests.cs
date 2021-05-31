using Sknet.InRuleGitStorage.Tests.Fixtures;
using System;
using System.IO;
using Xunit;

namespace Sknet.InRuleGitStorage.Tests.InRuleGitRepositoryTests.StaticMethods
{
    public class CloneTests : IDisposable
    {
        private readonly GitRepositoryFixture _fixture;

        public CloneTests()
        {
            _fixture = new GitRepositoryFixture();
        }

        public void Dispose()
        {
            _fixture.Dispose();
        }

        [Fact]
        public void WithNullSourceUrl_ShouldThrowException()
        {
            // Act/Assert
            Assert.Throws<ArgumentNullException>(() => InRuleGitRepository.Clone(null, "destinationPath", new CloneOptions()));
        }

        [Theory]
        [InlineData("")]
        [InlineData("  ")]
        public void WithWhiteSpaceSourceUrl_ShouldThrowException(string sourceUrl)
        {
            // Act/Assert
            Assert.Throws<ArgumentException>(() => InRuleGitRepository.Clone(sourceUrl, "destinationPath", new CloneOptions()));
        }

        [Fact]
        public void WithNullDestinationPath_ShouldThrowException()
        {
            // Act/Assert
            Assert.Throws<ArgumentNullException>(() => InRuleGitRepository.Clone("sourceUrl", null, new CloneOptions()));
        }

        [Theory]
        [InlineData("")]
        [InlineData("  ")]
        public void WithWhiteSpaceDestinationPath_ShouldThrowException(string destinationPath)
        {
            // Act/Assert
            Assert.Throws<ArgumentException>(() => InRuleGitRepository.Clone("sourceUrl", destinationPath, new CloneOptions()));
        }

        [Fact]
        public void WithEmptyDirectory_ShouldCloneAndReturnRepository()
        {
            var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(path);
            Assert.True(Directory.Exists(path));

            try
            {
                // Act
                var clonedPath = InRuleGitRepository.Clone(_fixture.Repository.Info.Path, path, new CloneOptions());

                // Assert
                Assert.NotNull(clonedPath);
                Assert.Equal(Path.GetFullPath(path), Path.GetFullPath(clonedPath));
                Assert.True(new LibGit2Sharp.Repository(path).Info.IsBare);
            }
            finally
            {
                new DirectoryInfo(path).Attributes &= ~FileAttributes.ReadOnly;
                Directory.Delete(path, true);
            }
        }

        [Fact]
        public void WithDirectoryWithFiles_ShouldThrowException()
        {
            var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(path);
            var tempFilePath = Path.GetTempFileName();
            File.Move(tempFilePath, Path.Combine(path, Path.GetFileName(tempFilePath)));

            try
            {
                // Act/Assert
                Assert.Throws<ArgumentException>(() => InRuleGitRepository.Clone(_fixture.Repository.Info.Path, path, new CloneOptions()));
            }
            finally
            {
                Directory.Delete(path, true);
            }
        }

        [Fact]
        public void WithExistingGitRepository_ShouldThrowException()
        {
            var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));

            try
            {
                // Arrange
                LibGit2Sharp.Repository.Init(path);

                // Act
                Assert.Throws<ArgumentException>(() => InRuleGitRepository.Clone(_fixture.Repository.Info.Path, path, new CloneOptions()));
            }
            finally
            {
                Directory.Delete(path, true);
            }
        }
    }
}
