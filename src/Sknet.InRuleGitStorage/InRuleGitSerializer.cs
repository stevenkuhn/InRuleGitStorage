using InRule.Common.Utilities;
using InRule.Repository;
using InRule.Repository.EndPoints;
using InRule.Repository.RuleElements;
using InRule.Repository.Vocabulary;
using LibGit2Sharp;
using Sknet.InRuleGitStorage.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Sknet.InRuleGitStorage
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

            var xml = blob.GetContentStream();
            var type = GetDefTypeFromXml(xml);

            var def = (T)XmlSerializationUtility.GetObjectFromStream(xml, type);

            if (def is IContainsDataElements containsDataElementsDef)
            {
                var dataElementsTree = tree["DataElements"]?.Target as Tree;
                if (dataElementsTree != null)
                {
                    var collectionBlob = (Blob)dataElementsTree["DataElements.xml"].Target;
                    var collectionStream = collectionBlob.GetContentStream();
                    var defCollection = (DataElementDefCollection)XmlSerializationUtility.GetObjectFromStream(collectionStream, typeof(DataElementDefCollection));

                    containsDataElementsDef.DataElements = defCollection;

                    foreach (var dataElementTreeEntry in dataElementsTree)
                    {
                        if (dataElementTreeEntry.Name == "DataElements.xml") continue;

                        var dataElementBlob = (Blob)dataElementTreeEntry.Target;
                        var dataElementXml = dataElementBlob.GetContentStream();
                        var dataElementType = GetDefTypeFromXml(dataElementXml);
                        var dataElementDef = (DataElementDef)XmlSerializationUtility.GetObjectFromStream(dataElementXml, dataElementType);

                        containsDataElementsDef.DataElements[dataElementDef.Guid] = dataElementDef;
                    }
                }
            }

            if (def is IContainsEndPoints containsEndPointsDef)
            {
                var endPointsTree = tree["EndPoints"]?.Target as Tree;
                if (endPointsTree != null)
                {
                    var collectionBlob = (Blob)endPointsTree["EndPoints.xml"].Target;
                    var collectionStream = collectionBlob.GetContentStream();
                    var defCollection = (EndPointDefCollection)XmlSerializationUtility.GetObjectFromStream(collectionStream, typeof(EndPointDefCollection));

                    containsEndPointsDef.EndPoints = defCollection;

                    foreach (var endPointTreeEntry in endPointsTree)
                    {
                        if (endPointTreeEntry.Name == "EndPoints.xml") continue;

                        var endPointBlob = (Blob)endPointTreeEntry.Target;
                        var endPointXml = endPointBlob.GetContentStream();
                        var endPointType = GetDefTypeFromXml(endPointXml);
                        var endPointDef = (EndPointDef)XmlSerializationUtility.GetObjectFromStream(endPointXml, endPointType);

                        containsEndPointsDef.EndPoints[endPointDef.Guid] = endPointDef;
                    }
                }
            }

            if (def is IContainsEntities containsEntitiesDef)
            {
                var entitiesTree = tree["Entities"]?.Target as Tree;
                if (entitiesTree != null)
                {
                    var collectionBlob = (Blob)entitiesTree["Entities.xml"].Target;
                    var collectionStream = collectionBlob.GetContentStream();
                    var defCollection = (EntityDefCollection)XmlSerializationUtility.GetObjectFromStream(collectionStream, typeof(EntityDefCollection));

                    containsEntitiesDef.Entities = defCollection;

                    foreach (var entityTreeEntry in entitiesTree)
                    {
                        if (entityTreeEntry.Name == "Entities.xml") continue;

                        var entityDef = DeserializeDefFromTree<EntityDef>(entityTreeEntry);
                        containsEntitiesDef.Entities[entityDef.Guid] = entityDef;
                    }
                }
            }

            if (def is IContainsFields containsFieldsDef)
            {
                var fieldsTree = tree["Fields"]?.Target as Tree;
                if (fieldsTree != null)
                {
                    var collectionBlob = (Blob)fieldsTree["Fields.xml"].Target;
                    var collectionStream = collectionBlob.GetContentStream();
                    var defCollection = (FieldDefCollection)XmlSerializationUtility.GetObjectFromStream(collectionStream, typeof(FieldDefCollection));

                    containsFieldsDef.Fields = defCollection;

                    foreach (var fieldTreeEntry in fieldsTree)
                    {
                        if (fieldTreeEntry.Name == "Fields.xml") continue;

                        var fieldDef = DeserializeDefFromTree<FieldDef>(fieldTreeEntry);
                        containsFieldsDef.Fields[fieldDef.Guid] = fieldDef;
                    }
                }
            }

            if (def is IContainsRuleElements containsRuleElementsDef && !(def is IContainsRuleSets))
            {
                var ruleElementsTree = tree["RuleElements"]?.Target as Tree;
                if (ruleElementsTree != null)
                {
                    var collectionBlob = (Blob)ruleElementsTree["RuleElements.xml"].Target;
                    var collectionStream = collectionBlob.GetContentStream();
                    var defCollection = (RuleElementDefCollection)XmlSerializationUtility.GetObjectFromStream(collectionStream, typeof(RuleElementDefCollection));

                    containsRuleElementsDef.RuleElements = defCollection;

                    foreach (var ruleElementTreeEntry in ruleElementsTree)
                    {
                        if (ruleElementTreeEntry.Name == "RuleElements.xml") continue;

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
                    var collectionBlob = (Blob)ruleSetsTree["RuleSets.xml"].Target;
                    var collectionStream = collectionBlob.GetContentStream();
                    var defCollection = (RuleSetDefBaseCollection)XmlSerializationUtility.GetObjectFromStream(collectionStream, typeof(RuleSetDefBaseCollection));

                    containsRuleSetsDef.RuleSets = defCollection;

                    foreach (var ruleSetTreeEntry in ruleSetsTree)
                    {
                        if (ruleSetTreeEntry.Name == "RuleSets.xml") continue;

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
                    var collectionBlob = (Blob)ruleSetParametersTree["Parameters.xml"].Target;
                    var collectionStream = collectionBlob.GetContentStream();
                    var defCollection = (RuleSetParameterDefCollection)XmlSerializationUtility.GetObjectFromStream(collectionStream, typeof(RuleSetParameterDefCollection));

                    containsRuleSetParametersDef.Parameters = defCollection;

                    foreach (var ruleSetParameterTreeEntry in ruleSetParametersTree)
                    {
                        if (ruleSetParameterTreeEntry.Name == "Parameters.xml") continue;

                        var ruleSetParameterBlob = (Blob)ruleSetParameterTreeEntry.Target;
                        var ruleSetParameterXml = ruleSetParameterBlob.GetContentStream();
                        var ruleSetParameterType = GetDefTypeFromXml(ruleSetParameterXml);
                        var ruleSetParameterDef = (RuleSetParameterDef)XmlSerializationUtility.GetObjectFromStream(ruleSetParameterXml, ruleSetParameterType);

                        containsRuleSetParametersDef.Parameters[ruleSetParameterDef.Guid] = ruleSetParameterDef;
                    }
                }
            }

            if (def is IContainsVocabulary containsVocabularyDef)
            {
                var vocabularyTree = tree["Vocabulary"]?.Target as Tree;
                if (vocabularyTree != null)
                {
                    var collectionBlob = (Blob)vocabularyTree["Templates.xml"].Target;
                    var collectionStream = collectionBlob.GetContentStream();
                    var defCollection = (TemplateDefCollection)XmlSerializationUtility.GetObjectFromStream(collectionStream, typeof(TemplateDefCollection));

                    var vocabularyBlob = (Blob)tree["Vocabulary.xml"].Target;
                    var vocabularyStream = vocabularyBlob.GetContentStream();
                    var vocabularyDef = (VocabularyDef)XmlSerializationUtility.GetObjectFromStream(vocabularyStream, typeof(VocabularyDef));
                    containsVocabularyDef.Vocabulary = vocabularyDef;

                    containsVocabularyDef.Vocabulary.Templates = defCollection;

                    foreach (var templateTreeEntry in vocabularyTree)
                    {
                        if (templateTreeEntry.Name == "Templates.xml") continue;

                        var templateBlob = (Blob)templateTreeEntry.Target;
                        var templateXml = templateBlob.GetContentStream();
                        var templateType = GetDefTypeFromXml(templateXml);
                        var templateDef = (TemplateDef)XmlSerializationUtility.GetObjectFromStream(templateXml, templateType);

                        containsVocabularyDef.Vocabulary.Templates[templateDef.Guid] = templateDef;
                    }
                }
            }

            return def;
        }

        private static readonly Dictionary<string, Type> DefTypeLookup = 
            typeof(RuleRepositoryDefBase).Assembly
                .GetTypes()
                .Where(type => type.IsSubclassOf(typeof(RuleRepositoryDefBase)))
                .ToDictionary(type => type.GetXmlTypeName(), type => type);

        private static Type GetDefTypeFromXml(Stream xml)
        {
            var settings = new XmlReaderSettings { ConformanceLevel = ConformanceLevel.Fragment, CloseInput = false };
            using var reader = XmlReader.Create(xml, settings);

            while (reader.Read() && !reader.IsStartElement()) { };
            var typeName = reader.Name;

            xml.Position = 0;
            return DefTypeLookup[typeName];
        }

        private Tree SerializeDefsThatHaveNoChildDefs(RuleRepositoryDefCollection defs, string propertyName, ObjectDatabase objectDatabase)
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

            // serialize def collection and then remove items from collection
            var defCollectionBlob = objectDatabase.CreateBlob(defs);
            treeDefinition.Add($"{propertyName}.xml", defCollectionBlob, Mode.NonExecutableFile);

            defs.Clear();

            return objectDatabase.CreateTree(treeDefinition);
        }

        private Tree SerializeDefsThatHaveChildDefs(RuleRepositoryDefCollection defs, string propertyName, ObjectDatabase objectDatabase)
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

            // serialize def collection and then remove items from collection
            var defCollectionBlob = objectDatabase.CreateBlob(defs);
            treeDefinition.Add($"{propertyName}.xml", defCollectionBlob, Mode.NonExecutableFile);

            defs.Clear();

            return objectDatabase.CreateTree(treeDefinition);
        }

        private Tree SerializeDefToTree(RuleRepositoryDefBase def, ObjectDatabase objectDatabase)
        {
            var treeDefinition = new TreeDefinition();

            if (def is IContainsDataElements containsDataElementsDef)
            {
                var tree = SerializeDefsThatHaveNoChildDefs(containsDataElementsDef.DataElements, "DataElements", objectDatabase);
                if (tree != null)
                {
                    treeDefinition.Add("DataElements", tree);
                }
            }

            if (def is IContainsEndPoints containsEndPointsDef)
            {
                var tree = SerializeDefsThatHaveNoChildDefs(containsEndPointsDef.EndPoints, "EndPoints", objectDatabase);
                if (tree != null)
                {
                    treeDefinition.Add("EndPoints", tree);
                }
            }

            if (def is IContainsEntities containsEntitiesDef)
            {
                var tree = SerializeDefsThatHaveChildDefs(containsEntitiesDef.Entities, "Entities", objectDatabase);
                if (tree != null)
                {
                    treeDefinition.Add("Entities", tree);
                }
            }

            if (def is IContainsFields containsFieldsDef)
            {
                var tree = SerializeDefsThatHaveChildDefs(containsFieldsDef.Fields, "Fields", objectDatabase);
                if (tree != null)
                {
                    treeDefinition.Add("Fields", tree);
                }

                tree = SerializeDefsThatHaveChildDefs(containsFieldsDef.Classifications, "Classifications", objectDatabase);
                if (tree != null)
                {
                    treeDefinition.Add("Classifications", tree);
                }
            }

            if (def is IContainsRuleElements containsRuleElementsDef && !(def is IContainsRuleSets))
            {
                var tree = SerializeDefsThatHaveChildDefs(containsRuleElementsDef.RuleElements, "RuleElements", objectDatabase);
                if (tree != null)
                {
                    treeDefinition.Add("RuleElements", tree);
                }
            }

            if (def is IContainsRuleSets containsRuleSetsDef)
            {
                var tree = SerializeDefsThatHaveChildDefs(containsRuleSetsDef.RuleSets, "RuleSets", objectDatabase);
                if (tree != null)
                {
                    treeDefinition.Add("RuleSets", tree);
                }
            }

            if (def is IContainsRuleSetParameters containsRuleSetParametersDef)
            {
                var tree = SerializeDefsThatHaveNoChildDefs(containsRuleSetParametersDef.Parameters, "Parameters", objectDatabase);
                if (tree != null)
                {
                    treeDefinition.Add("Parameters", tree);
                }
            }

            if (def is IContainsVocabulary containsVocabularyDef)
            {
                var tree = SerializeDefsThatHaveNoChildDefs(containsVocabularyDef.Vocabulary?.Templates, "Templates", objectDatabase);
                if (tree != null)
                {
                    treeDefinition.Add("Vocabulary", tree);

                    var vocabularyBlob = objectDatabase.CreateBlob(containsVocabularyDef.Vocabulary);
                    treeDefinition.Add("Vocabulary.xml", vocabularyBlob, Mode.NonExecutableFile);
                }
            }

            /*if (def is IHaveHintTable haveHintTableDef)
            {

            }*/

            var blob = objectDatabase.CreateBlob(def);
            treeDefinition.Add($"{def.Name}.xml", blob, Mode.NonExecutableFile);

            return objectDatabase.CreateTree(treeDefinition);
        }
    }
}
