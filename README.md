InRuleGitStorage
====

[![Nuget](https://img.shields.io/nuget/vpre/Sknet.InRuleGitStorage)](https://www.nuget.org/packages/Sknet.InRuleGitStorage)

This project allows you to store and manage your [InRule](https://www.inrule.com/)® business rules in a Git repository as an alternative to the built-in support of the file system and irCatalog.

# Features

- Initialize a new InRule git repository
- Open an existing InRule git repository
- Clone, pull from, and push to a remote InRule git repository
- Create, remove, and checkout a branch
- Commit (serialize a `RuleApplicationDef` into a git commit)
- Merge branches (_merge conflict support is not yet supported_)
- Get rule application (deserialize the current branch into a `RuleApplicationDef`)
- Get a list of rule applications

# Quickstart

## Installing

```powershell
Install-Package Sknet.InRuleGitStorage -IncludePrerelease
```

```batch
dotnet add package Sknet.InRuleGitStorage --version 0.3.0
```

## Basic example

```csharp
// Create a new repository in a local directory
InRuleGitRepository.Init("/path/to/local/repo");

// Get a new instance of your local InRule Git repository
using (var repo = InRuleGitRepository.Open("/path/to/local/repo"))
{
    // Create a new rule application and commit it to the "master" branch
    var ruleApp = new RuleApplicationDef("QuickstartSample");
    repo.Commit(ruleApp, "Add quickstart sample rule application");
    
    // Get the rule application from the Git repository
    ruleApp = repo.GetRuleApplication("QuickstartSample");
}
```

## Remote repository example

```csharp
// Clone the public samples repository to a local directory
InRuleGitRepository.Clone(
    sourceUrl: "https://github.com/stevenkuhn/InRuleGitStorage-Samples.git",
    destinationPath: "/path/to/local/repo");

// Get a new instance of your local InRule Git repository
using (var repo = InRuleGitRepository.Open("/path/to/local/repo"))
{
    // Create a local branch that is tracked to the remote "v0.3.0" branch
    repo.CreateTrackedBranch("v0.3.0", "origin");
    
    // Switch the current branch to the newly created tracked branch
    repo.Checkout("v0.3.0");

    // Create a local branch from the "v0.3.0" branch
    repo.CreateBranch("invoice-date-field");
    
    // Switch the current branch to the newly created local branch
    repo.Checkout("invoice-date-field");

    // Get the InvoiceSample rule application from the repository, add an invoice date
    // field, and commit that change to the current branch
    var ruleApp = repo.GetRuleApplication("InvoiceSample");
    ruleApp.Entities["Invoice"].Fields.Add(new FieldDef("Date", DataType.DateTime));
    repo.Commit(ruleApp, "Add invoice date field");

    // Switch back to the previous branch that does not have the field change
    repo.Checkout("v0.3.0");
    
    // Merge the invoice date field change into the current branch
    repo.Merge("invoice-date-field");
    
    // Delete the original branch containing the invoice date field change since the
    // change now exists in the "v0.3.0" branch
    repo.RemoveBranch("invoice-date-field");
}
```

# Documentation

[Additional documentation](https://inrulegitstorage.stevenkuhn.net/) is available for the InRuleGitStorage SDK.

# Building

To build this project locally, you will need the [.NET 5.0 SDK](https://dotnet.microsoft.com/download/dotnet/5.0) and, if you are building in Windows, the [.NET Framework 4.7.2](https://dotnet.microsoft.com/download/dotnet-framework/net472) and [.NET Framework 4.6.1](https://dotnet.microsoft.com/download/dotnet-framework/net461) developer packs.

## Windows

By default the build process in Windows will build both the SDK and _experimental_ authoring extension, run tests for the SDK, publish both projects artifacts to the `.\artifacts` folder, and deploy the authoring extension to your local irAuthor®'s extension folder (if irAuthor is installed).

_You may provide the `--help` command-line parameter below for details on available targets and other command-line parameters._

### Powershell

```
PS> .\build.ps1
```

### Command Prompt

```
> build.cmd
```

## Linux

By default the build process in Linux will only build the SDK, run tests for the SDK, and publish its artifacts to the `.\artifacts` folder.

_You may provide the `--help` command-line parameter below for details on available targets and other command-line parameters._

```
$ ./build.sh
```

# License

InRuleGitStorage is licensed under the MIT license. See [LICENSE](LICENSE) for full license information.