using InRule.Repository;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace InRuleContrib.Repository.Storage.Git.Extensions
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
            var xml = RuleRepositoryDefBase.GetXml(def);

            var writer = new StreamWriter(stream);
            writer.Write(xml);
            writer.Flush();

            stream.Position = 0;

            return stream;
        }
    }
}
