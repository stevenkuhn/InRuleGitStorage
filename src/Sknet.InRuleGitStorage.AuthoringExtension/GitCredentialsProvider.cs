using System.Diagnostics;

namespace Sknet.InRuleGitStorage.AuthoringExtension
{
    public static class GitCredentialsProvider
    {
        public static LibGit2Sharp.Credentials CredentialsHandler(string url, string usernameFromUrl, LibGit2Sharp.SupportedCredentialTypes types)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "git.exe",
                Arguments = "credential fill",
                UseShellExecute = false,
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true
            };

            string username = null;
            string password = null;

            using (var process = new Process { StartInfo = startInfo })
            {
                process.Start();

                process.StandardInput.NewLine = "\n";
                process.StandardInput.WriteLine($"url={url}");
                process.StandardInput.WriteLine();

                string line;
                string error;
                while ((line = process.StandardOutput.ReadLine()) != null || (error = process.StandardError.ReadLine()) != null)
                {
                    var details = line.Split('=');
                    if (details[0] == "username")
                    {
                        username = details[1];
                    }
                    else if (details[0] == "password")
                    {
                        password = details[1];
                    }
                }

            }

            return new LibGit2Sharp.UsernamePasswordCredentials
            {
                Username = username,
                Password = password
            };
        }
    }
}
