using InRule.Authoring.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sknet.InRuleGitStorage.AuthoringExtension
{
    public class GitPersistenceInfo : PersistenceInfo
    {
        public GitRepositoryOption RepositoryOption { get; set; }

        public GitPersistenceInfo(string ruleApplicationName, string path, GitRepositoryOption repositoryOption)  
            : base(ruleApplicationName, path)
        {
            RepositoryOption = repositoryOption;
        }

        public override string ToString()
        {
            return $"Name: {RuleApplicationName}\nGit: {Filename}";
        }
    }

    public static class PersistenceInfoExtensions
    {
        public static bool IsGitRepository(this PersistenceInfo persistenceInfo)
        {
            if (persistenceInfo == null)
            {
                return false;
            }

            return persistenceInfo is GitPersistenceInfo;
        }
    }
}
