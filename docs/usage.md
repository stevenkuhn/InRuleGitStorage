# Usage

## Clone<small style="color: #aaa;">&nbsp;&nbsp;static method</small>

>Clones a remote InRule git repository to a new local repository.

Example:

```csharp
// Clone without authentication
InRuleGitRepository.Clone("https://github.com/owner/repo.git", "/path/to/local/repo");

// Clone with username/password authentication
InRuleGitRepository.Clone("https://github.com/owner/repo.git", "/path/to/local/repo", 
    new CloneOptions
    {
        CredentialsProvider = (url, usernameFromUrl, types) => new UsernamePasswordCredentials
        {
            Username = "github_username",
            Password = "github_accesstoken"
        }
    });
```

## Init<small style="color: #aaa;">&nbsp;&nbsp;static method</small>

>Initializes a new InRule git repository at the specified path.

Example:

```csharp
// Initialize a new repository to the specified path
InRuleGitRepository.Init("/path/to/local/repo");
```

## IsValid<small style="color: #aaa;">&nbsp;&nbsp;static method</small>

>Checks if the specified path is a valid InRule Git repository.

Example:

```csharp
// Returns true if the path is valid Git repository; false otherwise
var isValidRepo = InRuleGitRepository.IsValid("/path/to/local/repo");
```

## Open<small style="color: #aaa;">&nbsp;&nbsp;static method</small>

>Initializes a new instance of InRuleGitRepository for an existing repository at the specified path.

Example:

```csharp
// Get a new instance of the local InRule Git repository
using (var repo = InRuleGitRepository.Open("/path/to/local/repo"))
{
    // Perform any repo operations here
}
```

## Checkout

>Switches the current branch to the specified branch name.

Example:

```csharp
// Initialize a new repository
using (var repo = InRuleGitRepository.Init("/path/to/repo"))
{
    // Create a new branch
    repo.CreateBranch("mybranch");

    // Switch from the current "master" branch to "mybranch";
    // future commits will be made this this new branch
    repo.Checkout("mybranch");
}
```

## Commit

>Stores the content of the specified Rule Application in the current branch as a new commit.

Example:

```csharp
// Initialize a new repository
using (var repo = InRuleGitRepository.Init("/path/to/repo"))
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
using (var repo = InRuleGitRepository.Init("/path/to/repo"))
{
    // Create a new branch; however the current checked out branch
    // is still "master" unless a call to Checkout("mybranch") is made
    repo.CreateBranch("mybranch");
}
```

>Creates a new tracked branch from the remote branch of the same name.

Example:

```csharp
// Clone the remote repository to a local directory
InRuleGitRepository.Clone("https://github.com/owner/repo.git", "/path/to/local/repo");

// Get a new instance of the local InRule Git repository
using (var repo = InRuleGitRepository.Open("/path/to/local/repo"))
{
    // Create a new tracked branch based on the remote branch; however the current
    // checked out branch remains the same unless a call to Checkout("mybranch")
    // is made
    repo.CreateBranch("mybranch", "origin");
}
```

## Fetch

>Fetches all of the latest changes from a remote InRule git repository.

Example:

```csharp
// Clone the remote repository to a local directory
InRuleGitRepository.Clone("https://github.com/owner/repo.git", "/path/to/local/repo");

// Get a new instance of the local InRule Git repository
using (var repo = InRuleGitRepository.Open("/path/to/local/repo"))
{
    // Fetch all the latest changes from the "origin" remote
    repo.Fetch();

    // Add another remote and fetch all the latest changes from that remote
    repo.Remotes.Add("another-remote", "https://github.com/owner/another-repo.git");
    repo.Fetch("another-remote");

    // Fetch all the latest changes from "origin" using authentication
    repo.Fetch(new FetchOptions
    {
        CredentialsProvider = (url, usernameFromUrl, types) => new UsernamePasswordCredentials
        {
            Username = "github_username",
            Password = "github_accesstoken"
        }
    })
}
```

## GetRuleApplication

>Gets a rule application from the current branch.

Example:

```csharp
// Get a new instance of the local InRule Git repository
using (var repo = InRuleGitRepository.Open("/path/to/repo"))
{
    // Get a RuleApplicationDef instance from the current branch in the repository
    var ruleApplication = repo.GetRuleApplication("MyRuleApplication");
}
```

## GetRuleApplications

>Gets a collection of references to available rule application from the current branch.

Example:

```csharp
// Get a new instance of the local InRule Git repository
using (var repo = InRuleGitRepository.Open("/path/to/repo"))
{
    // Get collection of rule application information and latest commit info for each
    // rule application for the current branch in the repository
    var ruleApplications = repo.GetRuleApplications();
}
```

## Merge

>Performs a merge of the current branch and the specified branch, and create a commit if there are no conflicts.

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
// Get a new instance of the local InRule Git repository
using (var repo = InRuleGitRepository.Open("/path/to/repo"))
{
    // Remove the branch; it must not be the current checked out branch
    repo.RemoveBranch("mybranch");
}
```

## RemoveRuleApplication

>Creates a commit that removes a rule application from the current branch

Example:
```csharp
// Get a new instance of the local InRule Git repository
using (var repo = InRuleGitRepository.Open("/path/to/repo"))
{
    // Create a commit that removes MyRuleApplication from the current branch
    repo.RemoveRuleApplication("MyRuleApplication", "Remove remove application");
}
```