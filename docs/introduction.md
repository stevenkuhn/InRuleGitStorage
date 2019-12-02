# Introduction

[![Nuget](https://img.shields.io/nuget/vpre/Sknet.InRuleGitStorage)](https://www.nuget.org/packages/Sknet.InRuleGitStorage)

This project allows you to store and manage your [InRule](https://www.inrule.com/)® business rules in a Git repository as an alternative to the built-in support of the file system and irCatalog.

## Features

- Initialize a new InRule git repository
- Open an existing InRule git repository
- Clone, pull from, and push to a remote InRule git repository
- Create, remove, and checkout a branch
- Commit (serialize a `RuleApplicationDef` into a git commit)
- Merge branches (_merge conflict support is a work in progress_)
- Get rule application (deserialize the current branch into a `RuleApplicationDef`)
- Get a list of rule applications

