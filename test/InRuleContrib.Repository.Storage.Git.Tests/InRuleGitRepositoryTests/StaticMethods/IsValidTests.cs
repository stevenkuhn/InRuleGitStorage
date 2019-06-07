using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xunit;

namespace InRuleContrib.Repository.Storage.Git.Tests.InRuleGitRepositoryFactoryTests
{
    public class IsValidTests
    {
        [Fact]
        public void WithNullPath_ShouldThrowException()
        {
            // Act/Assert
            Assert.Throws<ArgumentNullException>(() => InRuleGitRepository.IsValid(null));
        }

        [Theory]
        [InlineData("")]
        [InlineData("  ")]
        public void WithWhiteSpacePath_ShouldReturnFalse(string path)
        {
            // Act
            var result = InRuleGitRepository.IsValid(path);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void WithFilePath_ShouldReturnFalse()
        {
            var path = Path.GetTempFileName();

            try
            {
                // Act
                var result = InRuleGitRepository.IsValid(path);

                // Assert
                Assert.False(result);
            }
            finally
            {
                File.Delete(path);
            }
        }

        [Fact]
        public void WithEmptyDirectory_ShouldReturnFalse()
        {
            var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(path);
            Assert.True(Directory.Exists(path));

            try
            {
                // Act
                var result = InRuleGitRepository.IsValid(path);

                // Assert
                Assert.False(result);
            }
            finally
            {
                Directory.Delete(path);
            }
        }

        [Fact]
        public void WithGitRepository_ShouldReturnTrue()
        {
            var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(path);
            Assert.True(Directory.Exists(path));

            try
            {
                // Arrange
                LibGit2Sharp.Repository.Init(path);

                // Act
                var result = InRuleGitRepository.IsValid(path);

                // Assert
                Assert.True(result);
            }
            finally
            {
                Directory.Delete(path, true);
            }
        }
    }
}
