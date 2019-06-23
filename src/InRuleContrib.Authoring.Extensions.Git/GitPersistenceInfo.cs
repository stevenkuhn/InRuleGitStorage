using InRule.Authoring.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InRuleContrib.Authoring.Extensions.Git
{
    public class GitPersistenceInfo : PersistenceInfo
    {
        public GitPersistenceInfo(string ruleApplicationName, string path)  
            : base(ruleApplicationName, path)
        {
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
                throw new ArgumentNullException(nameof(persistenceInfo));
            }

            return persistenceInfo is GitPersistenceInfo;
        }
    }
}
