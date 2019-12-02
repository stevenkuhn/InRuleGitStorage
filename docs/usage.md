# Usage

## Checkout

>Switches the current branch to the specified branch name.

Example:

```csharp
// Initialize a new repository
using (var repo = InRuleRepository.Init("/path/to/repo"))
{
    // Create a new branch
    repo.CreateBranch("mybranch");

    // Switch from the current "master" branch to "mybranch";
    // future commits will be made this this new branch
    repo.Checkout("mybranch");
}
```

## Clone

>Clones a remote InRule git repository to a new local repository.

Example:

```csharp
// Clone without authentication
InRuleGitRepository.Clone("https://github.com/owner/repo.git", "/path/to/local/repo");

// Clone with username/password authentication
InRuleGitRepository.Clone("https://github.com/owner/repo.git", "/path/to/local/repo", 
    new Sknet.InRuleGitStorage.CloneOptions
    {
        CredentialsProvider = (url, usernameFromUrl, types) => new UsernamePasswordCredentials
        {
            Username = "github_username",
            Password = "github_accesstoken"
        }
    });
```

## Commit

>Stores the content of the specified Rule Application in the current branch as a new commit.

Example:

```csharp
// Initialize a new repository
using (var repo = InRuleRepository.Init("/path/to/repo"))
{
    // Create a new rule application
    var ruleApplication = new RuleApplicationDef("MyRuleApplication");

    // Commit the rule application to the repository
    repo.Commit(ruleApplication, "Added a new rule application");
}
```

## CreateBranch

>Creates a new branch from the current branch.

Example:

```csharp
// Initialize a new repository
using (var repo = InRuleRepository.Init("/path/to/repo"))
{
    // Create a new branch; however the current checked out branch
    // is still "master" unless a call to Checkout("mybranch") is made
    repo.CreateBranch("mybranch");
}
```

## Fetch

>Fetches all of the latest changes from a remote InRule git repository.

Example:

```csharp
```

## GetRuleApplication

>Gets a rule application from the current branch.

Example:

```csharp
```

## GetRuleApplications

>Gets a collection of references to available rule application from the current branch.

Example:

```csharp
```

## Init

>Initializes a new InRule git repository at the specified path.

Example:

```csharp
```

## IsValid

>Checks if the specified path is a valid InRule git repository.

Example:

```csharp
```

## Merge

>Performs a merge of the current branch and the specified branch, and create a commit if there are no conflicts.

Example:

```csharp
```

## Open

>Initializes a new instance of InRuleGitRepository for an existing repository at the specified path.

Example:

```csharp
```

## Pull

>Fetches all of the changes from a remote InRule git repository and merge into the current branch.

Example:

```csharp
```

## Push

>Pushes the current branch to a remote InRule git repository.

Example:

```csharp
```

## RemoveBranch

>Removes an existing branch.

Example:

```csharp
```