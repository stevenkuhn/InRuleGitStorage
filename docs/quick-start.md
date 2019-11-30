# Quick Start

## Installation

InRuleGitStorage is a NuGet package using a NuGet package installer of your choice or by referencing the package directly in your project file.

<!-- tabs:start -->

#### ** Package Manager **

```powershell
PM> Install-Package Sknet.InRuleGitStorage -Version 0.1.0
```

#### ** .NET CLI **

```batch
dotnet add package Sknet.InRuleGitStorage --version 0.1.0
```

#### ** Package Reference **

```xml
<PackageReference Include="Sknet.InRuleGitStorage" Version="0.1.0" />
```

<!-- tabs:end -->

## Usage

### Create a new Git repository

```csharp
InRuleGitRepository.Init("C:/path/to/your/repo");
```

### Or, clone an existing Git repository

```csharp
InRuleGitRepository.Clone("https://github.com/owner/repo.git", "C:/path/to/your/repo");
```

### Create your first commit

```csharp
using (var repo = InRuleGitRepository.Open("C:/path/to/your/repo"))
{
    var ruleApp = new RuleApplicationDef("MyRuleApp");
    repo.Commit(ruleApp, "My first commit");
}
```

### Get your rule application

```csharp
using (var repo = InRuleGitRepository.Open("C:/path/to/your/repo"))
{
    var ruleApp = repo.GetRuleApplication("MyRuleApp");
}
```

### Create and checkout a branch

```csharp
using (var repo = InRuleGitRepository.Open("C:/path/to/your/repo"))
{
    var ruleApp = repo.CreateBranch("cool_feature");
    repo.Checkout("cool_feature");
}
```

### Merge a branch into your current branch

```csharp
using (var repo = InRuleGitRepository.Open("C:/path/to/your/repo"))
{
    repo.Checkout("master");
    repo.Merge("cool_feature");
}
```