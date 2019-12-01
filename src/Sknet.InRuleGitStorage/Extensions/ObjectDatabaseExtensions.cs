using InRule.Repository;
using InRule.Repository.Classifications;
using InRule.Repository.Templates;
using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sknet.InRuleGitStorage.Extensions
{
    internal static class ObjectDatabaseExtensions
    {
        internal static Blob CreateBlob(this ObjectDatabase objectDatabase, RuleRepositoryDefBase def)
        {
            if (objectDatabase == null)
            {
                throw new ArgumentNullException(nameof(objectDatabase));
            }

            if (def == null)
            {
                throw new ArgumentNullException(nameof(def));
            }

            using (var stream = def.GetXmlStream())
            {
                return objectDatabase.CreateBlob(stream);
            }
        }

        internal static Blob CreateBlob(this ObjectDatabase objectDatabase, RuleRepositoryDefCollection defCollection)
        {
            if (objectDatabase == null)
            {
                throw new ArgumentNullException(nameof(objectDatabase));
            }

            if (defCollection == null)
            {
                throw new ArgumentNullException(nameof(defCollection));
            }

            using (var stream = defCollection.GetXmlStream())
            {
                return objectDatabase.CreateBlob(stream);
            }
        }

        internal static Commit CreateCommit(this ObjectDatabase objectDatabase,
            Signature author,
            Signature committer,
            string message,
            RuleApplicationDef ruleApplicationDef,
            IEnumerable<Commit> parents,
            bool prettifyMessage)
        {
            return objectDatabase.CreateCommit(author, committer, message, ruleApplicationDef, parents, prettifyMessage, null);
        }

        internal static Commit CreateCommit(this ObjectDatabase objectDatabase,
            Signature author,
            Signature committer,
            string message,
            RuleApplicationDef ruleApplicationDef,
            IEnumerable<Commit> parents,
            bool prettifyMessage,
            char? commentChar)
        {
            var parentsList = new List<Commit>(parents);
            if (parentsList.Count > 1) throw new NotImplementedException();

            var allRuleAppsTreeDefinition = new TreeDefinition();
            if (parentsList.Count == 1)
            {
                var parentTree = parentsList[0].Tree;

                foreach (var treeEntry in parentTree)
                {
                    if (string.Equals(treeEntry.Name, ruleApplicationDef.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    allRuleAppsTreeDefinition.Add(treeEntry.Name, treeEntry);
                }
            }

            ruleApplicationDef = (RuleApplicationDef)ruleApplicationDef.CopyWithSameGuids();
            
            IInRuleGitSerializer serializer = new InRuleGitSerializer();
            var ruleAppTree = serializer.Serialize(ruleApplicationDef, objectDatabase);

            allRuleAppsTreeDefinition.Add(ruleApplicationDef.Name, ruleAppTree);
            var allRuleAppsTree = objectDatabase.CreateTree(allRuleAppsTreeDefinition);

            return objectDatabase.CreateCommit(author, committer, message, allRuleAppsTree, parentsList, prettifyMessage, commentChar);
        }
    }
}
