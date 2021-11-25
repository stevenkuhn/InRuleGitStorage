namespace Sknet.InRuleGitStorage.AuthoringExtension;

public class GitRepositoryOption
{
    private string? _workingDirectory;

    public Guid Guid { get; set; }
    public string Name { get; set; }
    public string SourceUrl { get; set; }

    public string WorkingDirectory
    {
        get
        {
            if (_workingDirectory != null && !string.IsNullOrWhiteSpace(_workingDirectory))
            {
                return _workingDirectory;
            }

            if (string.IsNullOrWhiteSpace(SourceUrl))
            {
                return "";
            }

            var hash = MD5Hash(SourceUrl);

            var appDataDirectory = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            return Path.Combine(appDataDirectory, "InRule", "irAuthor", "InRuleGitStorage", hash);
        }
        set
        {
            _workingDirectory = value;
        }
    }

    public GitRepositoryOption()
    {
        Name = "";
        Guid = Guid.NewGuid();
        SourceUrl = "";
    }

    private static string MD5Hash(string input)
    {
        StringBuilder hash = new();
        MD5CryptoServiceProvider md5provider = new();
        byte[] bytes = md5provider.ComputeHash(new UTF8Encoding().GetBytes(input));

        for (int i = 0; i < bytes.Length; i++)
        {
            hash.Append(bytes[i].ToString("x2"));
        }
        return hash.ToString();
    }
}

