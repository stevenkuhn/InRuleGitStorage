using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xunit;

namespace Sknet.InRuleGitStorage.Tests.InRuleGitRepositoryFactoryTests
{
    public class InitTests
    {
        [Fact]
        public void WithNullPath_ShouldThrowException()
        {
            // Act/Assert
            Assert.Throws<ArgumentNullException>(() => InRuleGitRepository.Init(null));
        }

        [Theory]
        [InlineData("")]
        [InlineData("  ")]
        public void WithWhiteSpacePath_ShouldThrowException(string path)
        {
            // Act/Assert
            Assert.Throws<ArgumentException>(() => InRuleGitRepository.Init(path));
        }

        [Fact]
        public void WithFilePath_ShouldThrowException()
        {
            var path = Path.GetTempFileName();

            try
            {
                // Act/Assert
                Assert.Throws<ArgumentException>(() => InRuleGitRepository.Init(path));
            }
            finally
            {
                File.Delete(path);
            }
        }

        [Fact]
        public void WithEmptyDirectory_ShouldInitAndReturnRepository()
        {
            var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(path);
            Assert.True(Directory.Exists(path));

            try
            {
                // Act
                var repository = InRuleGitRepository.Init(path);

                // Assert
                Assert.NotNull(repository);
                Assert.True(new LibGit2Sharp.Repository(path).Info.IsBare);
            }
            finally
            {
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
                Assert.Throws<ArgumentException>(() => InRuleGitRepository.Init(path));
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
                Assert.Throws<ArgumentException>(() => InRuleGitRepository.Init(path));
            }
            finally
            {
                Directory.Delete(path, true);
            }
        }
    }
}
