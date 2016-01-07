Git Source Control Provider 2015
================================

Introduction
------------
This Visual Studio Extensions integrates Git with Visual Studio solution explorer.  


## Upgrade : 1.4

*   This upgrade might not look like much on the surface, but under the hood, it was pretty large. Please post any issues https://github.com/jzoss/Git-Source-Control-Provider or contact me directly if you find any bugs of have any questions. 
*   I'm not asking for donataions or anything, but if you want to fuel my nerd happy-ness please rate the plugin and tell me how you like it.  

### Future Version

*   Performance - I want it to be the faster git provider out there. 
*   Keep fixing bugs people find. 
*   Fix the hundreds of Warnings.
*   Add new test project to replace the test project that had to be removed.
*   Remove all the dead code.
*   Finish move to libgit2sharp.



## Notice : Initial Visual Studio 2015 Upgrade
This is a fork of the super awesome [git SCC plugin](https://visualstudiogallery.msdn.microsoft.com/63a7e40d-4d71-4fbb-a23b-d262124b8f4c) by [Yiyi Sun](https://visualstudiogallery.msdn.microsoft.com/site/search?f[0].Type=User&f[0].Value=yysun)

*   This Upgrade started as a quick and dirty modification so I could use the git plug-in I wanted in Visual Studio 2015\. A few others asked if they could use it, so then I went a little crazy and ended up adding a new extension project in 2015 and moving the files over from the old project. From my testing all works, but please tell me if you find anything, I will try and fix it.<br\>


![solution explorer](http://gitscc.codeplex.com/Project/Download/FileDownload.aspx?DownloadId=123874)

Features
--------
* Display file status in solution explorer and solution navigator
* Display repository status e.g. in the middle of merging, patching, rebase and bisecting
* Enable/disable plug-in through visual studio's source control plug-in selection
* No source code control information stored in solution or project file
* Initialize new git repository and generate .gitignore 
* Integrates with [Git for Windows](http://code.google.com/p/msysgit)
* Integrates with [Git Extensions](http://code.google.com/p/gitextensions)
* Integrates with [TortoiseGit](http://code.google.com/p/tortoisegit)
* [Git - Pending Changes Tool Window] (http://gitscc.codeplex.com/wikipage?title=Commit%20Changes)
* [Git - View History Tool Window] (http://gitscc.codeplex.com/wikipage?title=View%20History)
* Options page

How to use
----------
* Install [Git for Windows](http://code.google.com/p/msysgit), or [Git Extensions](http://code.google.com/p/gitextensions), or [TortoiseGit](http://code.google.com/p/tortoisegit).
* Run Visual Studio. 
* Go to Tools | Extension Manager, search online gallery for Git Source Control Provider and install. 
* Go to Tools | Options, Select Source Control.
* Select Git Source Control Provider from the drop down list, and click OK.
* Open your solution controlled by Git to see the file's status.
* Right click within solution explorer and select "Git". If Git for Windows, Git Extensions or TortoiseGit are installed, their commands are listed in the menu.
* Using the option page to disable the commands if you like.


## Change Logs -- Visual Studio 2015

**C.I Build - Pre-V1.5** [![Build status](https://ci.appveyor.com/api/projects/status/pxqohbd79ix57vw5?svg=true)](https://ci.appveyor.com/project/jzoss/git-source-control-provider)

    ---New Features---
*   Added ability to update your .gitingore file to latest version from github
*   Added Track Active Item In Pending Changes - This will help those who have multiple repositories in one solution. It switch the pending changes to the repo for the active file.  


    ---Fixes---
*   Theme now switches with the need to resart.
*   Double clicking on files in Pending Changes now opens/switches to the file. 
*   No longer slows down when used with Resharper
*   Fixed issue #15, #21 where sometimes the plugin caused Visual Studio to crash. 
*   Fixed issue #16, Solution Explorer glyphs not updating. 



**V1.4.2**

*   Fixed Issues #1, #7, #8, #9, #10, #11 and #12
*   Fixed Switch command
*   Added dark theme form diff window



**V1.4.1**

*   Fixed Settings Error
*   Fix bugs #5 + #6. -Thanks 
*   Thanks To  teebee76 (You Rock) for his help, mnadel, PureKrome + NightOwl888, you guys are also super cool. Keep finidng bugs and I will fix them as fast as I can.  



**V1.4**

*   Switch to using LibGit2Sharp, should see a large performace boost.
*   Fix Pending changes window.
*   Fixed reported bug 2 + 3
*   Refactored external diff tools.. Should be able to extend them in the future
*   Cleaned out lots of dead code. 
*   Fix lot's of bugs
*   Removed Git Diff Margin(use https://visualstudiogallery.msdn.microsoft.com/cf49cf30-2ca6-4ea0-b7cc-6a8e0dadc1a8 )
*   Fixed file missing exceptions.

[Full Changelog](history.md)

Documentation
-----------------
[Source + Documentation](https://github.com/jzoss/Git-Source-Control-Provider)


