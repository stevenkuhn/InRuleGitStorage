using System;
using System.Linq;
using System.Xml.Serialization;

namespace Sknet.InRuleGitStorage.Extensions
{
    internal static class TypeExtensions
    {
        public static T GetCustomAttribute<T>(this Type type, bool inherit)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));

            return (T)type.GetCustomAttributes(typeof(T), inherit).SingleOrDefault();
        }

        public static string GetXmlTypeName(this Type type)
        {
            var xmlType = type.GetCustomAttribute<XmlTypeAttribute>(false);

            return !string.IsNullOrWhiteSpace(xmlType?.TypeName) ? xmlType.TypeName : type.Name;
        }
    }
}
