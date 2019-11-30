using InRule.Authoring.Services;
using InRule.Repository;
using InRule.Repository.Client;
using InRule.Repository.RuleElements;
using InRule.Security;
using System;
using System.Collections.Generic;

namespace Sknet.InRuleGitStorage.AuthoringExtension
{
    public abstract class RuleApplicationServiceWrapper : IRuleApplicationServiceImplementation
    {
        public IRuleApplicationServiceImplementation InnerImplementation { get; }

        public RuleApplicationServiceWrapper(IRuleApplicationServiceImplementation innerRuleApplicationServiceImpl)
        {
            InnerImplementation = innerRuleApplicationServiceImpl ?? throw new ArgumentNullException(nameof(innerRuleApplicationServiceImpl));
        }

        public virtual bool BindToRuleApplication(RuleRepositoryDefBase def)
        {
            return InnerImplementation.BindToRuleApplication(def);
        }

        public virtual bool CheckForPermission(RuleUserRolePermissions permission, RuleRepositoryDefBase def)
        {
            return InnerImplementation.CheckForPermission(permission, def);
        }

        public virtual bool CheckForPermission(RuleUserRolePermissions permission, RuleRepositoryDefBase def, bool showWarningMsgBox)
        {
            return InnerImplementation.CheckForPermission(permission, def, showWarningMsgBox);
        }

        public virtual bool CheckIn()
        {
            return InnerImplementation.CheckIn();
        }

        public virtual bool CheckOut(RuleRepositoryDefBase def)
        {
            return InnerImplementation.CheckOut(def);
        }

        public virtual bool CheckOut(ICollection<RuleRepositoryDefBase> defs)
        {
            return InnerImplementation.CheckOut(defs);
        }

        public virtual bool CheckOut(RuleAppCheckOutMode checkOutMode)
        {
            return InnerImplementation.CheckOut(checkOutMode);
        }

        public virtual bool CloseRequest()
        {
            return InnerImplementation.CloseRequest();
        }

        public virtual void GenerateMissingHintTables(List<RuleSetDef> ruleSets, Dictionary<Guid, List<LanguageRuleDef>> languageRules)
        {
            InnerImplementation.GenerateMissingHintTables(ruleSets, languageRules);
        }

        public virtual RuleCatalogConnection GetCatalogConnection()
        {
            return InnerImplementation.GetCatalogConnection();
        }

        public virtual bool GetLatestRevision()
        {
            return InnerImplementation.GetLatestRevision();
        }

        public virtual RuleCatalogConnection GetSpecificCatalogConnection(Uri uri)
        {
            return InnerImplementation.GetSpecificCatalogConnection(uri);
        }

        public virtual RuleCatalogConnection GetSpecificCatalogConnection(Uri uri, bool isSingleSignOn)
        {
            return InnerImplementation.GetSpecificCatalogConnection(uri, isSingleSignOn);
        }

        public virtual RuleCatalogConnection GetSpecificCatalogConnection(Uri uri, bool isSingleSignOn, string username, string password)
        {
            return InnerImplementation.GetSpecificCatalogConnection(uri, isSingleSignOn, username, password);
        }

        public virtual Guid InsertSharedElement(RuleRepositoryDefBase def)
        {
            return InnerImplementation.InsertSharedElement(def);
        }

        public virtual Guid InsertSharedRuleFlow(RuleRepositoryDefBase def)
        {
            return InnerImplementation.InsertSharedRuleFlow(def);
        }

        public virtual bool OpenFromCatalog(Uri uri, string ruleAppName)
        {
            return InnerImplementation.OpenFromCatalog(uri, ruleAppName);
        }

        public virtual bool OpenFromCatalog(Uri uri, string ruleAppName, bool isSingleSignOn)
        {
            return InnerImplementation.OpenFromCatalog(uri, ruleAppName, isSingleSignOn);
        }

        public virtual bool OpenFromCatalog(Uri uri, string ruleAppName, bool isSingleSignOn, string username, string password)
        {
            return InnerImplementation.OpenFromCatalog(uri, ruleAppName, isSingleSignOn, username, password);
        }

        public virtual bool OpenFromCatalogDialog()
        {
            return InnerImplementation.OpenFromCatalogDialog();
        }

        public virtual bool OpenFromFile()
        {
            return InnerImplementation.OpenFromFile();
        }

        public virtual bool OpenFromFilename(string filename)
        {
            return InnerImplementation.OpenFromFilename(filename);
        }

        public virtual void OpenTraceFile()
        {
            InnerImplementation.OpenTraceFile();
        }

        public virtual void OpenTraceFile(string filename)
        {
            InnerImplementation.OpenTraceFile(filename);
        }

        public virtual bool Save()
        {
            return InnerImplementation.Save();
        }

        public virtual bool SaveCopyToFile()
        {
            return InnerImplementation.SaveCopyToFile();
        }

        public virtual bool SavePendingChanges()
        {
            return InnerImplementation.SavePendingChanges();
        }

        public virtual bool SaveToCatalog(bool isSaveAs)
        {
            return InnerImplementation.SaveToCatalog(isSaveAs);
        }

        public virtual bool SaveToFile()
        {
            return InnerImplementation.SaveToFile();
        }

        public virtual void SetPermissions(RuleRepositoryDefBase def)
        {
            InnerImplementation.SetPermissions(def);
        }

        public virtual void SetSchemaPermissions()
        {
            InnerImplementation.SetSchemaPermissions();
        }

        public virtual bool SetSchemaShareable()
        {
            return InnerImplementation.SetSchemaShareable();
        }

        public virtual bool SetShareable(RuleRepositoryDefBase def)
        {
            return InnerImplementation.SetShareable(def);
        }

        public virtual bool UnbindFromRuleApplication(RuleRepositoryDefBase def)
        {
            return InnerImplementation.UnbindFromRuleApplication(def);
        }

        public virtual bool UndoCheckout(ICollection<RuleRepositoryDefBase> defs)
        {
            return InnerImplementation.UndoCheckout(defs);
        }

        public virtual bool UndoCheckout(RuleRepositoryDefBase selectedDef)
        {
            return InnerImplementation.UndoCheckout(selectedDef);
        }

        public virtual bool UndoCheckout(RuleRepositoryDefBase selectedDef, bool quiet)
        {
            return InnerImplementation.UndoCheckout(selectedDef, quiet);
        }

        public virtual bool Unshare(RuleRepositoryDefBase def)
        {
            return InnerImplementation.Unshare(def);
        }
    }
}
