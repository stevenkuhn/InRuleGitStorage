using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Serialization;

namespace Sknet.InRuleGitStorage.AuthoringExtension
{
    public class GitRepositoryOption
    {
        public Guid Guid { get; set; }

        public string Name { get; set; }

        public string SourceUrl { get; set; }

        public string WorkingDirectory
        {
            get
            {
                if (string.IsNullOrWhiteSpace(SourceUrl))
                {
                    return null;
                }

                var hash = MD5Hash(SourceUrl);

                var appDataDirectory = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                return Path.Combine(appDataDirectory, "InRule", "irAuthor", "GitRepository", hash);
            }
        }

        public GitRepositoryOption()
        {
            Name = "";
            Guid = Guid.NewGuid();
        }

        private static string MD5Hash(string input)
        {
            StringBuilder hash = new StringBuilder();
            MD5CryptoServiceProvider md5provider = new MD5CryptoServiceProvider();
            byte[] bytes = md5provider.ComputeHash(new UTF8Encoding().GetBytes(input));

            for (int i = 0; i < bytes.Length; i++)
            {
                hash.Append(bytes[i].ToString("x2"));
            }
            return hash.ToString();
        }
    }
}
