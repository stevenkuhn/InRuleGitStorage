# Git Storage for InRule

This project adds support for storing and managing InRule rule applications in a custom Git repository.

## Getting Started

### Installing

```powershell
Install-Package InRuleContrib.Repository.Storage.Git -IncludePrerelease
```

## Usage

### Create a new repository
```csharp
InRuleGitRepository.Init(@"C:\path\to\your\repo");
```

### Create your first commit
```csharp
using (var repo = InRuleGitRepository.Open(@"C:\path\to\your\repo"))
{
    var ruleAppDef = new RuleApplicationDef("MyRuleApp");

    var identity = new Identity("Alex Doe", "alex.doe@example.org");
    var signature = new Signature(identity, DateTimeOffset.Now);
    repo.Commit(ruleAppDef, "My first commit", signature, signature);
}
```

### Get a rule application from the current branch
```csharp
using (var repo = InRuleGitRepository.Open(@"C:\path\to\your\repo"))
{
    var ruleAppDef = repo.GetRuleApplication("MyRuleApp");
}
```

## Examples

### Create multiple commits across different branches

```csharp
var repoPath = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
    "MyInRuleRepository");

var identity = new Identity("John Doe", "john.doe@example.org");

// Create a repository in the current user's _My Documents_ folder
InRuleGitRepository.Init(repoPath);
using (var repo = InRuleGitRepository.Open(repoPath))
{
    // Update the rule application and create commits in `master`.
    var ruleAppDef = new RuleApplicationDef("MyRuleApp");
    var entityDef = ruleAppDef.Entities.Add(new EntityDef("MyEntity"));

    repo.Commit(ruleAppDef, "My first commit",
        new Signature(identity, DateTimeOffset.Now),
        new Signature(identity, DateTimeOffset.Now));

    entityDef.Fields.Add(new FieldDef("MyField1", DataType.String));

    repo.Commit(ruleAppDef, "My second commit",
        new Signature(identity, DateTimeOffset.Now),
        new Signature(identity, DateTimeOffset.Now));

    // Create a new branch called `my-new-branch` and set it as the current branch.
    repo.CreateBranch("my-new-branch");
    repo.Checkout("my-new-branch");
    entityDef.Fields.Add(new FieldDef("MyField2", DataType.String));
    entityDef.Fields.Add(new FieldDef("MyField3", DataType.Integer));

    // Update the rule application and commit the change to `my-new-branch`.
    repo.Commit(ruleAppDef, "My third commit",
        new Signature(identity, DateTimeOffset.Now),
        new Signature(identity, DateTimeOffset.Now));
}
```