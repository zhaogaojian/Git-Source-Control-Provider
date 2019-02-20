Easy Git Integration Tools (EZ-GIT)
================================

Introduction
------------
[![Join the chat at https://gitter.im/jzoss/Git-Source-Control-Provider](https://badges.gitter.im/jzoss/Git-Source-Control-Provider.svg)](https://gitter.im/jzoss/Git-Source-Control-Provider?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)

** This Visual Studio Extensions integrates Git with Visual Studio solution explorer and the status bar ** 

![solution explorer](http://gitscc.codeplex.com/Project/Download/FileDownload.aspx?DownloadId=123874)

![Status Bar](https://cloud.githubusercontent.com/assets/3586254/15159754/d5b40796-16bb-11e6-97bb-25ecdd6f42ef.png)


New Feature Highlights
-------------
* BETA : Support Visual Studio 2019
* BETA : Support For SSDT 2015 and SSDT 2017 [Read about Limitations](#ssdt)
* Added Visual character count for comments
* Now Autosaves on Commit! (Configurable) 
* Added option to auto-add projects and to the git repository when you add them to the project!
* Switch and create branches, switch git repositories, and open the pending changes window all from the status bar!



Features
--------
* Display file status in solution explorer and solution navigator
* Multiple repository support.  
* Display repository status e.g. in the middle of merging, patching, rebase and bisecting
* Enable/disable plug-in through visual studio's source control plug-in selection
* No source code control information stored in solution or project file
* Initialize new git repository and generate .gitignore 
* Integrates with [Git for Windows](https://gitforwindows.org/)
* Integrates with [Git Extensions](https://gitextensions.github.io/)
* Integrates with [TortoiseGit](https://tortoisegit.org/)
* [Git - Pending Changes Tool Window](http://gitscc.codeplex.com/wikipage?title=Commit%20Changes)
* [Git - View History Tool Window](http://gitscc.codeplex.com/wikipage?title=View%20History)
* Options page

How to use
----------
* Install [Git for Windows](https://gitforwindows.org/), or [Git Extensions](https://gitextensions.github.io/), or [TortoiseGit](https://tortoisegit.org/).
* Run Visual Studio. 
* Go to Tools | Extension Manager, search online gallery for Git Source Control Provider and install. Or Install From [Visual Studio Gallery](https://visualstudiogallery.msdn.microsoft.com/51e11ccc-6334-4873-912d-bf5025eb115d) 
* Go to Tools | Options, Select Source Control.
* Select Git Source Control Provider from the drop down list, and click OK.
* Open your solution controlled by Git to see the file's status.
* Right click within solution explorer and select "Git". If Git for Windows, Git Extensions or TortoiseGit are installed, their commands are listed in the menu.


Visual Studio Data Tools 2017/2015 Support            {#ssdt}
========

* Visual Visual Studio Data Tools 2015 needs to be at Update 3 or Later.
* Note: As of 5/23/18 - In SSDT 2017: Solution Explorer Status Icons and the Status bar may not work.  All other features work, including the right click menu in the solution explorer and the pending changes window. There does not appear to be anything can be done to fix this, because the built in SCC providers also don't work. 

Living on the Edge!
----------
Install the Latest C.I. Build either from [Open VSIX Gallery](http://vsixgallery.com/extension/GitSccProvider.Microsoft.88d658b3-e361-4e7f-8f4d-9e78f6e4515a/) or [Github](https://github.com/jzoss/Git-Source-Control-Provider/releases/tag/GSCP-CI). Don't worry you will still auto-update when the next release comes out. 

For Bonus Points Add this [feed](http://vsixgallery.com/feed/extension/GitSccProvider.Microsoft.88d658b3-e361-4e7f-8f4d-9e78f6e4515a) from VSIX Gallery to your extension manager to always auto-update to the latest C.I. build.  

How to contribute 
----------
If you like this plugin there is a few way you can help out.

* Review It! - I'm not asking for donations or anything, but if you do use this and like it.. Please head over to the [Visual Studio Gallery](https://visualstudiogallery.msdn.microsoft.com/51e11ccc-6334-4873-912d-bf5025eb115d) and review it.  Good reviews refill my nerd powers that power all those late night programming sessions. They also make me feel all warm and fuzzy! 
* Suggest Features / Point out bugs. 
* Test It - Check out the [Releases](https://github.com/jzoss/Git-Source-Control-Provider/releases). If there is a early release try it out and let me know if you find any bugs!
* Code - Check out the code, play with it. maybe fix a bug while you are there.. It fun and educational. It also makes you super cool, better looking and possibly give you superpowers.

#### Help Wanted - Features/Fixes 
* Options Page - Settings are stored in a user folder, this is not how it is supposed to work. The UI the UI is ugly does not look right on high dpi monitors. See [Example](https://github.com/Microsoft/VSSDK-Extensibility-Samples/tree/master/Options_Page)
* Localization - I would be nice to support a few other  languages.
* Button commands - This part is a bit messy right now. It would be sweet if this could be cleaned up.

#### Building

All you need to build the code is Visual Studio 2015 with Visual Studio Extensibility Tools installed. It's **THAT** easy!



## Current status

| Build | Status |
| --- | --- |
| **Release** | [![release][release-badge]][release] |
| **Dev** | [![dev][dev-badge]][dev] |


[release]: https://ci.appveyor.com/project/jzoss/git-source-control-provider
[release-badge]: https://ci.appveyor.com/api/projects/status/0178gk42noyr7mk3?svg=true
[dev]: https://ci.appveyor.com/project/jzoss/git-source-control-provider-bfftg
[dev-badge]: https://ci.appveyor.com/api/projects/status/qr4hm0uqyr4wnnm9?svg=true

## Change Logs -- Visual Studio 2015

**V-Next** 
*   Add more features to the status bar.
*   Work on integrating gitflow.

**V1.9.0**
* Visual Studio 2019 Support

**V1.8.2**
* Bug Fix: #102,#105 

**V1.8.0**
* Update: Updated to Async Pasckage (https://docs.microsoft.com/en-us/visualstudio/extensibility/how-to-use-asyncpackage-to-load-vspackages-in-the-background)
* Bug Fix: #89 - Big Thanks to ceztko for fixing this bug. 
* Enhancement: #59,#90,#91
* BETA : Limited Support For SSDT 2015 and SSDT 2017 [Read about Limitations](#ssdt)

**V1.7.3**
* Bug Fix: #80,#83

**V1.7.2**
* Bug Fix: #74,#77,#78, #79

**V1.7.0**
* Updated solution to Visual Studio 2017
* Plugin now supports Visual Studio 2015 and 2017


#####---New Features---
*   Added ability to update your .gitingore file to latest version from github
*   Added Track Active Item In Pending Changes - This will help those who have multiple repositories in one solution. It switch the pending changes to the repo for the active file.  
*   Added ability Filter Pending Changes To Only Show Solution Files


#####---Fixes---
*   Fixed issue where plugin could periodically crash Visual Studio.
*   Theme now switches with the need to restart.
*   Double clicking on files in Pending Changes now opens/switches to the file. 
*   No longer slows down when used with Resharper
*   Fixed issue #15, #21 where sometimes the plugin caused Visual Studio to crash. 
*   Fixed issue #16, Solution Explorer glyphs not updating. 




## Notice : Initial Visual Studio 2015 Upgrade
This is a fork of the super awesome [git SCC plugin](https://visualstudiogallery.msdn.microsoft.com/63a7e40d-4d71-4fbb-a23b-d262124b8f4c) by [Yiyi Sun](https://visualstudiogallery.msdn.microsoft.com/site/search?f[0].Type=User&f[0].Value=yysun)


[Full Changelog](history.md)

Documentation
-----------------
[Source + Documentation](https://github.com/jzoss/Git-Source-Control-Provider)


