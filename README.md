
# <img src="https://github.com/JosefNemec/LiteDbExplorer/blob/master/source/LiteDbExplorer/Images/icon.png" width="32">  LiteDb Explorer
Graphical editor for [LiteDB](https://github.com/mbdavid/LiteDB) databases. Writter in .NET and WPF.

### Features in current Alpha release:
* View and edit existing documents
* Add new items to database including files
* Preview files (images and text files)
* Export database documents (as JSON) and files
* Change password in protected databases
* Shrink database
* Open multiple databases at the same time
* Document tree
* Portable exe (require .NET 4.6.2)
* Material dark and white theme
<p>
<img align="center" src="https://raw.githubusercontent.com/julianpaulozzi/LiteDbExplorer/master/web/screen_01.png" width="880" >
</p>
<p>
<img align="center" src="https://raw.githubusercontent.com/julianpaulozzi/LiteDbExplorer/master/web/screen_02.png" width="880" >
</p>
<p>
<img align="center" src="https://raw.githubusercontent.com/julianpaulozzi/LiteDbExplorer/master/web/screen_03.png" width="880" >
</p>
<p>
<img align="center" src="https://raw.githubusercontent.com/julianpaulozzi/LiteDbExplorer/master/web/screen_04.png" width="880" >
</p>

Download
---------

Grab latest build from [releases](https://github.com/julianpaulozzi/LiteDbExplorer/releases) page.

Portable single exe version available from version 7.0, just unzip and run.

Requirements: Windows 7, 8 or 10 and [.Net 4.6.2](https://www.microsoft.com/en-us/download/details.aspx?id=53344)

Mac OS (WIP) [LiteDbExplorer.Mac](https://github.com/julianpaulozzi/LiteDbExplorer.Mac)
---------
Work in progress native mac version using Xamarin.Mac and netstandard2.0.


Building
---------

To build from cmdline run **build.ps1** in PowerShell. Script builds Release configuration by default into the same directory. Script accepts *Configuration*, *OutputPath*, *Portable* (creates zip package) and *SkipBuild* parameters.

Contributions
---------

All contributions are welcome!

Regarding code styling, there are only a few major rules:
* private fields and properties should use camelCase (with underscore)
* all methods (private and public) should use PascalCase
* use spaces instead of tabs with 4 spaces width
* always encapsulate with brackets:
```
if (true)
{
    DoSomething()
}
```
instead of 
```
if (true)
    DoSomething()
```
