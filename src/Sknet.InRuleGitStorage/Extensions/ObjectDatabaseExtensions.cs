namespace Sknet.InRuleGitStorage.Extensions;

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
}
