using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Serialization;

namespace InRuleContrib.Authoring.Extensions.Git
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

        public string Username { get; set; }

        [XmlIgnore]
        public string Password { get; set; }

        public string EncryptedPassword
        {
            get
            {
                return EncryptText(Password);
            }
            set
            {
                Password = DecryptText(value);
            }
        }

        public GitRepositoryOption()
        {
            Name = "";
            SourceUrl = "";
            Username = "";
            Password = null;
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

        private static string EncryptText(string clearText)
        {
            if (string.IsNullOrEmpty(clearText))
            {
                return "";
            }
            var clearBytes = Encoding.UTF8.GetBytes(clearText);
            var encryptedBytes = ProtectedData.Protect(clearBytes, null, DataProtectionScope.CurrentUser);
            var encryptedText = Convert.ToBase64String(encryptedBytes);
            return encryptedText;
        }

        private static string DecryptText(string encryptedText)
        {
            if (string.IsNullOrEmpty(encryptedText))
            {
                return "";
            }
            var encryptedBytes = Convert.FromBase64String(encryptedText);
            var clearBytes = ProtectedData.Unprotect(encryptedBytes, null, DataProtectionScope.CurrentUser);
            var clearText = Encoding.UTF8.GetString(clearBytes);
            return clearText;
        }
    }
}
