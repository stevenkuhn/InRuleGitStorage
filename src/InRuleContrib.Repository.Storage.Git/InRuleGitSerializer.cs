using InRule.Repository;
using InRule.Repository.EndPoints;
using InRule.Repository.RuleElements;
using InRule.Repository.Vocabulary;
using InRuleContrib.Repository.Storage.Git.Extensions;
using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace InRuleContrib.Repository.Storage.Git
{
    /// <summary>
    ///
    /// </summary>
    public interface IInRuleGitSerializer
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="ruleApplicationDef"></param>
        /// <param name="objectDatabase"></param>
        /// <returns></returns>
        Tree Serialize(RuleApplicationDef ruleApplicationDef, ObjectDatabase objectDatabase);

        /// <summary>
        ///
        /// </summary>
        /// <param name="treeEntry"></param>
        /// <returns></returns>
        RuleApplicationDef Deserialize(TreeEntry treeEntry);
    }

    internal class InRuleGitSerializer : IInRuleGitSerializer
    {
        public InRuleGitSerializer()
        {
        }

        public Tree Serialize(RuleApplicationDef ruleApplicationDef, ObjectDatabase objectDatabase)
        {
            return SerializeDefToTree(ruleApplicationDef, objectDatabase);
        }

        public RuleApplicationDef Deserialize(TreeEntry treeEntry)
        {
            return DeserializeDefFromTree<RuleApplicationDef>(treeEntry);
        }

        private T DeserializeDefFromTree<T>(TreeEntry treeEntry) where T : RuleRepositoryDefBase
        {
            var tree = (Tree)treeEntry.Target;
            var blob = (Blob)tree[$"{treeEntry.Name}.xml"].Target;

            var xml = blob.GetContentText();
            var type = GetDefTypeFromXml(xml);
            var def = (T)RuleRepositoryDefBase.LoadFromXml(xml, type);

            if (def is IContainsDataElements containsDataElementsDef)
            {
                var dataElementsTree = tree["DataElements"]?.Target as Tree;
                if (dataElementsTree != null)
                {
                    foreach (var dataElementTreeEntry in dataElementsTree)
                    {
                        var dataElementBlob = (Blob)dataElementTreeEntry.Target;
                        var dataElementXml = dataElementBlob.GetContentText();
                        var dataElementType = GetDefTypeFromXml(dataElementXml);
                        var dataElementDef = (DataElementDef)RuleRepositoryDefBase.LoadFromXml(dataElementXml, dataElementType);

                        containsDataElementsDef.DataElements[dataElementDef.Guid] = dataElementDef;
                    }
                }
            }

            if (def is IContainsEndPoints containsEndPointsDef)
            {
                var endPointsTree = tree["EndPoints"]?.Target as Tree;
                if (endPointsTree != null)
                {
                    foreach (var endPointTreeEntry in endPointsTree)
                    {
                        var endPointBlob = (Blob)endPointTreeEntry.Target;
                        var endPointXml = endPointBlob.GetContentText();
                        var endPointType = GetDefTypeFromXml(endPointXml);
                        var endPointDef = (EndPointDef)RuleRepositoryDefBase.LoadFromXml(endPointXml, endPointType);

                        containsEndPointsDef.EndPoints[endPointDef.Guid] = endPointDef;
                    }
                }
            }

            if (def is IContainsEntities containsEntitiesDef)
            {
                var entitiesTree = tree["Entities"]?.Target as Tree;
                if (entitiesTree != null)
                {
                    foreach (var entityTreeEntry in entitiesTree)
                    {
                        var entityDef = DeserializeDefFromTree<EntityDef>(entityTreeEntry);
                        containsEntitiesDef.Entities[entityDef.Guid] = entityDef;
                    }
                }
            }

            if (def is IContainsFields containsFielsDef)
            {
                var fieldsTree = tree["Fields"]?.Target as Tree;
                if (fieldsTree != null)
                {
                    foreach (var fieldTreeEntry in fieldsTree)
                    {
                        var fieldDef = DeserializeDefFromTree<FieldDef>(fieldTreeEntry);
                        containsFielsDef.Fields[fieldDef.Guid] = fieldDef;
                    }
                }
            }

            if (def is IContainsRuleElements containsRuleElementsDef && !(def is IContainsRuleSets))
            {
                var ruleElementsTree = tree["RuleElements"]?.Target as Tree;
                if (ruleElementsTree != null)
                {
                    foreach (var ruleElementTreeEntry in ruleElementsTree)
                    {
                        var ruleElementDef = DeserializeDefFromTree<RuleElementDef>(ruleElementTreeEntry);
                        containsRuleElementsDef.RuleElements[ruleElementDef.Guid] = ruleElementDef;
                    }
                }
            }

            if (def is IContainsRuleSets containsRuleSetsDef)
            {
                var ruleSetsTree = tree["RuleSets"]?.Target as Tree;
                if (ruleSetsTree != null)
                {
                    foreach (var ruleSetTreeEntry in ruleSetsTree)
                    {
                        var ruleSetDef = DeserializeDefFromTree<RuleSetDef>(ruleSetTreeEntry);
                        containsRuleSetsDef.RuleSets[ruleSetDef.Guid] = ruleSetDef;
                    }
                }
            }

            if (def is IContainsRuleSetParameters containsRuleSetParametersDef)
            {
                var ruleSetParametersTree = tree["Parameters"]?.Target as Tree;
                if (ruleSetParametersTree != null)
                {
                    foreach (var ruleSetParameterTreeEntry in ruleSetParametersTree)
                    {
                        var ruleSetParameterBlob = (Blob)ruleSetParameterTreeEntry.Target;
                        var ruleSetParameterXml = ruleSetParameterBlob.GetContentText();
                        var ruleSetParameterType = GetDefTypeFromXml(ruleSetParameterXml);
                        var ruleSetParameterDef = (RuleSetParameterDef)RuleRepositoryDefBase.LoadFromXml(ruleSetParameterXml, ruleSetParameterType);

                        containsRuleSetParametersDef.Parameters[ruleSetParameterDef.Guid] = ruleSetParameterDef;
                    }
                }
            }

            if (def is IContainsVocabulary containsVocabularyDef)
            {
                var vocabularyTree = tree["Vocabulary"]?.Target as Tree;
                if (vocabularyTree != null)
                {
                    foreach (var vocabularyTreeEntry in vocabularyTree)
                    {
                        var vocabularyBlob = (Blob)vocabularyTreeEntry.Target;
                        var vocabularyXml = vocabularyBlob.GetContentText();
                        var vocabularyType = GetDefTypeFromXml(vocabularyXml);
                        var vocabularyDef = (TemplateDef)RuleRepositoryDefBase.LoadFromXml(vocabularyXml, vocabularyType);

                        containsVocabularyDef.Vocabulary.Templates[vocabularyDef.Guid] = vocabularyDef;
                    }
                }
            }

            return def;
        }

        private static readonly Dictionary<string, Type> DefTypeLookup =
            typeof(RuleRepositoryDefBase).Assembly
                .GetTypes()
                .Where(x => x.Name.EndsWith("Def") || (x.BaseType != null && x.BaseType.Name.EndsWith("Def")))
                .ToDictionary(x => x.Name, x => x);

        private static Type GetDefTypeFromXml(string xml)
        {
            string searchString = $"<?xml version=\"1.0\" encoding=\"utf-16\"?>{Environment.NewLine}<";
            var typeName = xml.Substring(searchString.Length, xml.IndexOf(" ", searchString.Length, StringComparison.Ordinal) - searchString.Length);

            return DefTypeLookup[typeName];
        }

        private Tree SerializeDefsThatHaveNoChildDefs(RuleRepositoryDefCollection defs, ObjectDatabase objectDatabase)
        {
            if (defs == null || defs.Count == 0) return null;

            var treeDefinition = new TreeDefinition();

            for (int i = 0; i < defs.Count; i++)
            {
                var def = defs[i];
                var blob = objectDatabase.CreateBlob(def);
                treeDefinition.Add($"{def.Name}.xml", blob, Mode.NonExecutableFile);

                var defPointer = (RuleRepositoryDefBase)Activator.CreateInstance(def.GetType());
                defPointer.Guid = def.Guid;
                defPointer.Name = def.Guid.ToString();
                defs[i] = defPointer;
            }

            return objectDatabase.CreateTree(treeDefinition);
        }

        private Tree SerializeDefsThatHaveChildDefs(RuleRepositoryDefCollection defs, ObjectDatabase objectDatabase)
        {
            if (defs == null || defs.Count == 0) return null;

            var treeDefinition = new TreeDefinition();

            for (int i = 0; i < defs.Count; i++)
            {
                var def = defs[i];
                var tree = SerializeDefToTree(def, objectDatabase);
                treeDefinition.Add($"{def.Name}", tree);

                var defPointer = (RuleRepositoryDefBase)Activator.CreateInstance(def.GetType());
                defPointer.Guid = def.Guid;
                defPointer.Name = def.Guid.ToString();
                defs[i] = defPointer;
            }

            return objectDatabase.CreateTree(treeDefinition);
        }

        private Tree SerializeDefToTree(RuleRepositoryDefBase def, ObjectDatabase objectDatabase)
        {
            var treeDefinition = new TreeDefinition();

            if (def is IContainsDataElements containsDataElementsDef)
            {
                var tree = SerializeDefsThatHaveNoChildDefs(containsDataElementsDef.DataElements, objectDatabase);
                if (tree != null)
                {
                    treeDefinition.Add("DataElements", tree);
                }
            }

            if (def is IContainsEndPoints containsEndPointsDef)
            {
                var tree = SerializeDefsThatHaveNoChildDefs(containsEndPointsDef.EndPoints, objectDatabase);
                if (tree != null)
                {
                    treeDefinition.Add("EndPoints", tree);
                }
            }

            if (def is IContainsEntities containsEntitiesDef)
            {
                var tree = SerializeDefsThatHaveChildDefs(containsEntitiesDef.Entities, objectDatabase);
                if (tree != null)
                {
                    treeDefinition.Add("Entities", tree);
                }
            }

            if (def is IContainsFields containsFieldsDef)
            {
                var tree = SerializeDefsThatHaveChildDefs(containsFieldsDef.Fields, objectDatabase);
                if (tree != null)
                {
                    treeDefinition.Add("Fields", tree);
                }

                tree = SerializeDefsThatHaveChildDefs(containsFieldsDef.Classifications, objectDatabase);
                if (tree != null)
                {
                    treeDefinition.Add("Classifications", tree);
                }
            }

            if (def is IContainsRuleElements containsRuleElementsDef && !(def is IContainsRuleSets))
            {
                var tree = SerializeDefsThatHaveChildDefs(containsRuleElementsDef.RuleElements, objectDatabase);
                if (tree != null)
                {
                    treeDefinition.Add("RuleElements", tree);
                }
            }

            if (def is IContainsRuleSets containsRuleSetsDef)
            {
                var tree = SerializeDefsThatHaveChildDefs(containsRuleSetsDef.RuleSets, objectDatabase);
                if (tree != null)
                {
                    treeDefinition.Add("RuleSets", tree);
                }
            }

            if (def is IContainsRuleSetParameters containsRuleSetParametersDef)
            {
                var tree = SerializeDefsThatHaveNoChildDefs(containsRuleSetParametersDef.Parameters, objectDatabase);
                if (tree != null)
                {
                    treeDefinition.Add("Parameters", tree);
                }
            }

            if (def is IContainsVocabulary containsVocabularyDef)
            {
                var tree = SerializeDefsThatHaveNoChildDefs(containsVocabularyDef.Vocabulary?.Templates, objectDatabase);
                if (tree != null)
                {
                    treeDefinition.Add("Vocabulary", tree);
                }
            }

            /*if (def is IHaveHintTable haveHintTableDef)
            {

            }*/

            var blob =objectDatabase.CreateBlob(def);
            treeDefinition.Add($"{def.Name}.xml", blob, Mode.NonExecutableFile);

            return objectDatabase.CreateTree(treeDefinition);
        }
    }
}
