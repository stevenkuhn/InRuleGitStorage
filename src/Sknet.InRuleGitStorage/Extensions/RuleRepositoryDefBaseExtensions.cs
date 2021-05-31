using InRule.Common.Utilities;
using InRule.Repository;
using System;
using System.IO;

namespace Sknet.InRuleGitStorage.Extensions
{
    internal static class RuleRepositoryDefBaseExtensions
    {
        internal static Stream GetXmlStream(this RuleRepositoryDefBase def)
        {
            if (def == null)
            {
                throw new ArgumentNullException(nameof(def));
            }

            var stream = new MemoryStream();

            XmlSerializationUtility.SaveObjectToStream(stream, def);

            stream.Position = 0;

            return stream;
        }

        internal static Stream GetXmlStream(this RuleRepositoryDefCollection defCollection)
        {
            if (defCollection == null)
            {
                throw new ArgumentNullException(nameof(defCollection));
            }

            var stream = new MemoryStream();

            XmlSerializationUtility.SaveObjectToStream(stream, defCollection);

            stream.Position = 0;

            return stream;
        }
    }
}
