using InRule.Authoring.Services;
using InRule.Authoring.Settings;
using System;
using System.Collections.Generic;

namespace InRuleContrib.Authoring.Extensions.Git
{
    public class GitRepositorySettings : ISettings
    {
        public static Guid Guid = new Guid("C339AC91-E844-4D18-B69E-F1050FCF4207");

        public static GitRepositorySettings Load(SettingsStorageService settingsStorageService)
        {
            return settingsStorageService.LoadSettings<GitRepositorySettings>(Guid);
        }

        public string Description { get; } = "Git repository settings";

        public Guid ID => Guid;

        public List<GitRepositoryOption> Options { get; private set; }

        public GitRepositorySettings()
        {
            Options = new List<GitRepositoryOption>();
        }

        public void Save(SettingsStorageService settingsStorageService)
        {
            settingsStorageService.SaveSettings(this);
        }
    }
}
