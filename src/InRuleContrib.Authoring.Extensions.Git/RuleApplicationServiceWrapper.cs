using InRule.Authoring.Services;
using InRule.Repository;
using InRule.Repository.Client;
using InRule.Repository.RuleElements;
using InRule.Security;
using System;
using System.Collections.Generic;

namespace InRuleContrib.Authoring.Extensions.Git
{
    public abstract class RuleApplicationServiceWrapper : IRuleApplicationServiceImplementation
    {
        private readonly IRuleApplicationServiceImplementation _innerRuleApplicationServiceImpl;

        public RuleApplicationServiceWrapper(IRuleApplicationServiceImplementation innerRuleApplicationServiceImpl)
        {
            _innerRuleApplicationServiceImpl = innerRuleApplicationServiceImpl ?? throw new ArgumentNullException(nameof(_innerRuleApplicationServiceImpl));
        }

        public virtual bool BindToRuleApplication(RuleRepositoryDefBase def)
        {
            return _innerRuleApplicationServiceImpl.BindToRuleApplication(def);
        }

        public virtual bool CheckForPermission(RuleUserRolePermissions permission, RuleRepositoryDefBase def)
        {
            return _innerRuleApplicationServiceImpl.CheckForPermission(permission, def);
        }

        public virtual bool CheckForPermission(RuleUserRolePermissions permission, RuleRepositoryDefBase def, bool showWarningMsgBox)
        {
            return _innerRuleApplicationServiceImpl.CheckForPermission(permission, def, showWarningMsgBox);
        }

        public virtual bool CheckIn()
        {
            return _innerRuleApplicationServiceImpl.CheckIn();
        }

        public virtual bool CheckOut(RuleRepositoryDefBase def)
        {
            return _innerRuleApplicationServiceImpl.CheckOut(def);
        }

        public virtual bool CheckOut(ICollection<RuleRepositoryDefBase> defs)
        {
            return _innerRuleApplicationServiceImpl.CheckOut(defs);
        }

        public virtual bool CheckOut(RuleAppCheckOutMode checkOutMode)
        {
            return _innerRuleApplicationServiceImpl.CheckOut(checkOutMode);
        }

        public virtual bool CloseRequest()
        {
            return _innerRuleApplicationServiceImpl.CloseRequest();
        }

        public virtual void GenerateMissingHintTables(List<RuleSetDef> ruleSets, Dictionary<Guid, List<LanguageRuleDef>> languageRules)
        {
            _innerRuleApplicationServiceImpl.GenerateMissingHintTables(ruleSets, languageRules);
        }

        public virtual RuleCatalogConnection GetCatalogConnection()
        {
            return _innerRuleApplicationServiceImpl.GetCatalogConnection();
        }

        public virtual bool GetLatestRevision()
        {
            return _innerRuleApplicationServiceImpl.GetLatestRevision();
        }

        public virtual RuleCatalogConnection GetSpecificCatalogConnection(Uri uri)
        {
            return _innerRuleApplicationServiceImpl.GetSpecificCatalogConnection(uri);
        }

        public virtual RuleCatalogConnection GetSpecificCatalogConnection(Uri uri, bool isSingleSignOn)
        {
            return _innerRuleApplicationServiceImpl.GetSpecificCatalogConnection(uri, isSingleSignOn);
        }

        public virtual RuleCatalogConnection GetSpecificCatalogConnection(Uri uri, bool isSingleSignOn, string username, string password)
        {
            return _innerRuleApplicationServiceImpl.GetSpecificCatalogConnection(uri, isSingleSignOn, username, password);
        }

        public virtual Guid InsertSharedElement(RuleRepositoryDefBase def)
        {
            return _innerRuleApplicationServiceImpl.InsertSharedElement(def);
        }

        public virtual Guid InsertSharedRuleFlow(RuleRepositoryDefBase def)
        {
            return _innerRuleApplicationServiceImpl.InsertSharedRuleFlow(def);
        }

        public virtual bool OpenFromCatalog(Uri uri, string ruleAppName)
        {
            return _innerRuleApplicationServiceImpl.OpenFromCatalog(uri, ruleAppName);
        }

        public virtual bool OpenFromCatalog(Uri uri, string ruleAppName, bool isSingleSignOn)
        {
            return _innerRuleApplicationServiceImpl.OpenFromCatalog(uri, ruleAppName, isSingleSignOn);
        }

        public virtual bool OpenFromCatalog(Uri uri, string ruleAppName, bool isSingleSignOn, string username, string password)
        {
            return _innerRuleApplicationServiceImpl.OpenFromCatalog(uri, ruleAppName, isSingleSignOn, username, password);
        }

        public virtual bool OpenFromCatalogDialog()
        {
            return _innerRuleApplicationServiceImpl.OpenFromCatalogDialog();
        }

        public virtual bool OpenFromFile()
        {
            return _innerRuleApplicationServiceImpl.OpenFromFile();
        }

        public virtual bool OpenFromFilename(string filename)
        {
            return _innerRuleApplicationServiceImpl.OpenFromFilename(filename);
        }

        public virtual void OpenTraceFile()
        {
            _innerRuleApplicationServiceImpl.OpenTraceFile();
        }

        public virtual void OpenTraceFile(string filename)
        {
            _innerRuleApplicationServiceImpl.OpenTraceFile(filename);
        }

        public virtual bool Save()
        {
            return _innerRuleApplicationServiceImpl.Save();
        }

        public virtual bool SaveCopyToFile()
        {
            return _innerRuleApplicationServiceImpl.SaveCopyToFile();
        }

        public virtual bool SavePendingChanges()
        {
            return _innerRuleApplicationServiceImpl.SavePendingChanges();
        }

        public virtual bool SaveToCatalog(bool isSaveAs)
        {
            return _innerRuleApplicationServiceImpl.SaveToCatalog(isSaveAs);
        }

        public virtual bool SaveToFile()
        {
            return _innerRuleApplicationServiceImpl.SaveToFile();
        }

        public virtual void SetPermissions(RuleRepositoryDefBase def)
        {
            _innerRuleApplicationServiceImpl.SetPermissions(def);
        }

        public virtual void SetSchemaPermissions()
        {
            _innerRuleApplicationServiceImpl.SetSchemaPermissions();
        }

        public virtual bool SetSchemaShareable()
        {
            return _innerRuleApplicationServiceImpl.SetSchemaShareable();
        }

        public virtual bool SetShareable(RuleRepositoryDefBase def)
        {
            return _innerRuleApplicationServiceImpl.SetShareable(def);
        }

        public virtual bool UnbindFromRuleApplication(RuleRepositoryDefBase def)
        {
            return _innerRuleApplicationServiceImpl.UnbindFromRuleApplication(def);
        }

        public virtual bool UndoCheckout(ICollection<RuleRepositoryDefBase> defs)
        {
            return _innerRuleApplicationServiceImpl.UndoCheckout(defs);
        }

        public virtual bool UndoCheckout(RuleRepositoryDefBase selectedDef)
        {
            return _innerRuleApplicationServiceImpl.UndoCheckout(selectedDef);
        }

        public virtual bool UndoCheckout(RuleRepositoryDefBase selectedDef, bool quiet)
        {
            return _innerRuleApplicationServiceImpl.UndoCheckout(selectedDef, quiet);
        }

        public virtual bool Unshare(RuleRepositoryDefBase def)
        {
            return _innerRuleApplicationServiceImpl.Unshare(def);
        }
    }
}
