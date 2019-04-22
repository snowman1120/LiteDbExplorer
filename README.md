
# <img src="https://github.com/julianpaulozzi/LiteDbExplorer/blob/master/source/LiteDbExplorer/Images/icon.png" width="32">  LiteDb Explorer
Graphical editor for [LiteDB](https://github.com/mbdavid/LiteDB) databases. Written in .NET and WPF.

## Important Notes

**This is a pre-release.** We'll point out that this release is identified as non-production ready.

The main purpose of keeping this fork is to meet my LiteDB use cases, refresh WPF knowledge, and share some improvements with the community.

I have seen some LiteDB use cases that go far beyond any that would have with this database. If you use LiteDB to store a large volume of data, large texts and many files ... this project currently will not meet your needs and is outside my current contribution scope. Any contribution is welcome, so before opening a issue mainly related to this topic collaborate and submit a pull request or use [other alternatives](https://github.com/mbdavid/LiteDB#3rd-party-tools-for-litedb).

<p>
<img align="center" src="https://raw.githubusercontent.com/julianpaulozzi/LiteDbExplorer/master/web/screen_01.png" width="880" >
</p>

## Features in current pre-release:
* Material dark and white theme
* Open multiple databases at the same time
* View and edit multiple documents with tabbed interface
* Document details in tree or Json
* Configurable layout options
* Preview files (images and text files)
* Add new items to database including files
* Export database documents (as JSON) and files
* Change password in protected databases
* Shrink database
* Portable exe (require .NET 4.6.2)

# Download

Grab latest build from [releases](https://github.com/julianpaulozzi/LiteDbExplorer/releases) page.

Portable single exe version available from version 7.0, just unzip and run.

Requirements: Windows 7, 8 or 10 and [.Net 4.6.2](https://www.microsoft.com/en-us/download/details.aspx?id=53344)

# Issue Reporting Guidelines

Please make sure to read the Issue Reporting Checklist before opening an issue. Issues not conforming to the guidelines may be closed immediately.

**This is a:** (required)  
- Bug Report or Feature Request

**Version:** (required)  
- Which version?

**Steps to reproduce:** (required) 
- Steps to reproduce...

**What is expected:** (required)  
- What is expected?

**What is actually happening:** (required)  
- What is actually happening?

**Any additional comments:** (optional)  
- Any additional comments 

# Contributions

All contributions are welcome!

Regarding code styling, there are only a few major rules:
* private fields and properties should use camelCase (with underscore)
* all methods (private and public) should use PascalCase
* use spaces instead of tabs with 4 spaces width
* always encapsulate with brackets:
```cs
if (true)
{
    DoSomething()
}
```
instead of 
```cs
if (true)
    DoSomething()
```

# Building

To build from cmdline run **build.ps1** in PowerShell. Script builds Release configuration by default into the same directory. Script accepts *Configuration*, *OutputPath*, *Portable* (creates zip package) and *SkipBuild* parameters.

# License
[MIT License](http://opensource.org/licenses/MIT).