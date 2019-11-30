using InRule.Repository;
using InRule.Repository.EndPoints;
using InRule.Repository.Vocabulary;
using LibGit2Sharp;
using Sknet.InRuleGitStorage.Tests.Fixtures;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Sknet.InRuleGitStorage.Tests.InRuleGitSerializerTests
{
    public class ContainsEntitiesTests : IDisposable
    {
        private readonly GitRepositoryFixture _fixture;

        public ContainsEntitiesTests()
        {
            _fixture = new GitRepositoryFixture();
        }

        public void Dispose()
        {
            _fixture.Dispose();
        }

        [Fact]
        public void WithRuleAppThatHasEntities_ShouldReturnEntitiesInTree()
        {
            // Arrange
            var serializer = new InRuleGitSerializer();
            var ruleApp = new RuleApplicationDef("RuleAppWithEntities");
            ruleApp.Entities.Add(new EntityDef("Entity1"));

            // Act
            var tree = serializer.Serialize(ruleApp, _fixture.Repository.ObjectDatabase);

            // Assert
            var entitiesTreeEntry = tree["Entities"];
            Assert.NotNull(entitiesTreeEntry);

            var entitiesTree = (Tree)entitiesTreeEntry.Target;
            Assert.NotNull(entitiesTree);

            var entity1TreeEntry = entitiesTree["Entity1"];
            Assert.NotNull(entity1TreeEntry);

            var entity1Tree = (Tree)entity1TreeEntry.Target;
            Assert.NotNull(entity1Tree);
            Assert.NotNull(entity1Tree["Entity1.xml"]);

            Assert.NotEmpty(ruleApp.Entities);
            Assert.All(ruleApp.Entities, (RuleRepositoryDefBase def) => Assert.Equal(def.Guid.ToString(), def.Name));
        }
    }

    public class ContainsFieldsTests : IDisposable
    {
        private readonly GitRepositoryFixture _fixture;

        public ContainsFieldsTests()
        {
            _fixture = new GitRepositoryFixture();
        }

        public void Dispose()
        {
            _fixture.Dispose();
        }

        [Fact]
        public void WithEntityThatHasFields_ShouldReturnFieldsInTree()
        {
            // Arrange
            var serializer = new InRuleGitSerializer();
            var ruleApp = new RuleApplicationDef("RuleApp1");
            var entityDef = ruleApp.Entities.Add(new EntityDef("EntityWithFields"));
            entityDef.Fields.Add(new FieldDef("Field1", DataType.String));

            // Act
            var tree = serializer.Serialize(ruleApp, _fixture.Repository.ObjectDatabase);
            tree = (Tree)tree["Entities/EntityWithFields"].Target;

            // Assert
            var fieldsTreeEntry = tree["Fields"];
            Assert.NotNull(fieldsTreeEntry);

            var fieldsTree = (Tree)fieldsTreeEntry.Target;
            Assert.NotNull(fieldsTree);

            var field1TreeEntry = fieldsTree["Field1"];
            Assert.NotNull(field1TreeEntry);

            var field1Tree = (Tree)field1TreeEntry.Target;
            Assert.NotNull(field1Tree);
            Assert.NotNull(field1Tree["Field1.xml"]);

            Assert.NotEmpty(entityDef.Fields);
            Assert.All(entityDef.Fields, (RuleRepositoryDefBase def) => Assert.Equal(def.Guid.ToString(), def.Name));
        }

        [Fact]
        public void WithEntityThatHasClassifications_ShouldReturnClassificationsInTree()
        {
            // Arrange
            var serializer = new InRuleGitSerializer();
            var ruleApp = new RuleApplicationDef("RuleApp1");
            var entityDef = ruleApp.Entities.Add(new EntityDef("EntityWithClassifications"));
            entityDef.Classifications.Add(new InRule.Repository.Classifications.ClassificationDef() { Name = "Classification1" });

            // Act
            var tree = serializer.Serialize(ruleApp, _fixture.Repository.ObjectDatabase);
            tree = (Tree)tree["Entities/EntityWithClassifications"].Target;

            // Assert
            var classificationsTreeEntry = tree["Classifications"];
            Assert.NotNull(classificationsTreeEntry);

            var classificationsTree = (Tree)classificationsTreeEntry.Target;
            Assert.NotNull(classificationsTree);

            var classification1TreeEntry = classificationsTree["Classification1"];
            Assert.NotNull(classification1TreeEntry);

            var classification1Tree = (Tree)classification1TreeEntry.Target;
            Assert.NotNull(classification1Tree);
            Assert.NotNull(classification1Tree["Classification1.xml"]);

            Assert.NotEmpty(entityDef.Classifications);
            Assert.All(entityDef.Classifications, (RuleRepositoryDefBase def) => Assert.Equal(def.Guid.ToString(), def.Name));
        }
    }

    public class ContainsDataElementsTests : IDisposable
    {
        private readonly GitRepositoryFixture _fixture;

        public ContainsDataElementsTests()
        {
            _fixture = new GitRepositoryFixture();
        }

        public void Dispose()
        {
            _fixture.Dispose();
        }

        [Fact]
        public void WithRuleAppThatHasDataElements_ShouldReturnDataElementsInTree()
        {
            // Arrange
            var serializer = new InRuleGitSerializer();
            var ruleApp = new RuleApplicationDef("RuleAppWithDataElements");
            ruleApp.DataElements.Add(new TableDef("Table1"));

            // Act
            var tree = serializer.Serialize(ruleApp, _fixture.Repository.ObjectDatabase);

            // Assert
            var dataElementsTreeEntry = tree["DataElements"];
            Assert.NotNull(dataElementsTreeEntry);

            var dataElementsTree = (Tree)dataElementsTreeEntry.Target;
            Assert.NotNull(dataElementsTree);
            Assert.NotNull(dataElementsTree["Table1.xml"]);

            Assert.NotEmpty(ruleApp.DataElements);
            Assert.All(ruleApp.DataElements, (RuleRepositoryDefBase def) => Assert.Equal(def.Guid.ToString(), def.Name));
        }
    }

    public class ContainsEndPointsTests : IDisposable
    {
        private readonly GitRepositoryFixture _fixture;

        public ContainsEndPointsTests()
        {
            _fixture = new GitRepositoryFixture();
        }

        public void Dispose()
        {
            _fixture.Dispose();
        }

        [Fact]
        public void WithRuleAppThatHasEndPoints_ShouldReturnEndPointsInTree()
        {
            // Arrange
            var serializer = new InRuleGitSerializer();
            var ruleApp = new RuleApplicationDef("RuleAppWithEndPoints");
            ruleApp.EndPoints.Add(new RestServiceDef("RestService1"));

            // Act
            var tree = serializer.Serialize(ruleApp, _fixture.Repository.ObjectDatabase);

            // Assert
            var endPointsTreeEntry = tree["EndPoints"];
            Assert.NotNull(endPointsTreeEntry);

            var endPointsTree = (Tree)endPointsTreeEntry.Target;
            Assert.NotNull(endPointsTree);
            Assert.NotNull(endPointsTree["RestService1.xml"]);

           Assert.NotEmpty(ruleApp.EndPoints);
           Assert.All(ruleApp.EndPoints, (RuleRepositoryDefBase def) => Assert.Equal(def.Guid.ToString(), def.Name));
        }
    }

    public class ContainsVocabularyTests : IDisposable
    {
        private readonly GitRepositoryFixture _fixture;

        public ContainsVocabularyTests()
        {
            _fixture = new GitRepositoryFixture();
        }

        public void Dispose()
        {
            _fixture.Dispose();
        }

        [Fact]
        public void WithEntityThatHasVocabularyTemplates_ShouldReturnTemplatesInTree()
        {
            // Arrange
            var serializer = new InRuleGitSerializer();
            var ruleApp = new RuleApplicationDef("RuleApp1");
            var entityDef = ruleApp.Entities.Add(new EntityDef("EntityWithVocabulary"));
            entityDef.Vocabulary = new VocabularyDef();
            entityDef.Vocabulary.Templates.Add(new NotificationTemplateDef { Name = "Notification1" });

            // Act
            var tree = serializer.Serialize(ruleApp, _fixture.Repository.ObjectDatabase);
            tree = (Tree)tree["Entities/EntityWithVocabulary"].Target;

            // Assert
            var vocabularyTreeEntry = tree["Vocabulary"];
            Assert.NotNull(vocabularyTreeEntry);

            var vocabularyTree = (Tree)vocabularyTreeEntry.Target;
            Assert.NotNull(vocabularyTree);
            Assert.NotNull(vocabularyTree["Notification1.xml"]);

            Assert.NotEmpty(entityDef.Vocabulary.Templates);
            Assert.All(entityDef.Vocabulary.Templates, (RuleRepositoryDefBase def) => Assert.Equal(def.Guid.ToString(), def.Name));
        }
    }
}
