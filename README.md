Git Source Control Provider 2015
================================

Introduction
------------
This Visual Studio Extensions integrates Git with Visual Studio solution explorer. 
This is a fork of the super awesome [git SCC plugin](https://visualstudiogallery.msdn.microsoft.com/63a7e40d-4d71-4fbb-a23b-d262124b8f4c) by [Yiyi Sun](https://visualstudiogallery.msdn.microsoft.com/site/search?f[0].Type=User&f[0].Value=yysun)

## Notice : Initial Visual Studio 2015 Upgrade

*   This Upgrade started as a quick and dirty modification so I could use the git plug-in I wanted in Visual Studio 2015\. A few others asked if they could use it, so then I went a little crazy and ended up adding a new extension project in 2015 and moving the files over from the old project. From my testing all works, but please tell me if you find anything, I will try and fix it.<br\>

### Future Version

*   Fix the hundreds of Warnings.
*   Add new test project to replace the test project that had to be removed.
*   Replace external submodule git dependencies with updated versions.
*   Find all unhanded file missing exceptions.


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

## Change Logs

**V1.3.1**

*   Converted Solution to VS 2015
*   Created new project for plug-in, update all references to Visual Studio 14.0

**V1.3**

*   Added Settings button to the Pending change tool window
*   Bug fixes: Pull request #110, #111, #112

**V1.2**

*   Support Visual Studio 2013
*   Bug fixes: Pull request #52, #94, #96, #97, #99, #103, #105, #106, #108, #109

**V1.1**

*   Add the [Laurent Kempe's GitDiffMargin](https://github.com/laurentkempe/GitDiffMargin) by [Sam Harwell](http://tunnelvisionlabs.com/research)  

    ![](https://a248.e.akamai.net/camo.github.com/efde11e7acc8e77f3da08bb8e30d14567c29e3c6/687474703a2f2f6661726d392e737461746963666c69636b722e636f6d2f383332392f383131363839353032355f656339353139623562625f6f2e706e67)
*   Performance Improvement (by Sam Harwell)
*   UI de-clutter and cleanup (by Sam Harwell)
*   Various bugs fixes (by Sam Harwell)
*   Visual Studio 2012 dark theme support (by Sam Harwell)
*   Refresh Git status using the solution refresh button (by Sam Harwell)
*   Use Visual Studio code editor to view the diff in pending changes window (by Sam Harwell)

**V1.0.1-V1.0.2**

*   Add Git Pending Change menu to View | Other Windows
*   Remove user name and email verification

**V1.0.0**

*   Add support to use Visual Studio's diff window (by Duncan Smart)
*   Prompt user to save files before commit
*   Prompt user to set name and email for git, if not already
*   Pending Changes commit not showing hooks errors/messages (by Javier Castro)
*   Support bulk file deletion (by Javier Castro)
*   Use new icons for git and git extensions
*   Merge pull requests, #30, #34, #36, #38 from github
*   [Allow commit of selected changes via Ctrl+Enter](http://gitscc.codeplex.com/workitem/17798)
*   [Automatically save items when refreshing](http://gitscc.codeplex.com/workitem/17795)
*   [commit-msg hook error not visible (by Javier Castro)](http://gitscc.codeplex.com/workitem/17793)
*   [Git Refresh not working in Pending Changes when auto refresh is disabled](http://gitscc.codeplex.com/workitem/17792)

**V 0.9.6.1**

*   Rebuild in release mode and write to error log file in Release mode

**V 0.9.6 (acb0278)**

*   Add support to local workspace of TFS 2012
*   Add UI to allow configure user name and password for Git
*   Bug fix: [Please allow amend with no changes](http://gitscc.codeplex.com/workitem/17789)
*   Bug fix: [Provider not detecting being in a repo](http://gitscc.codeplex.com/workitem/17788)
*   Bug fix: [Solution no longer identifies as Git controlled](http://gitscc.codeplex.com/workitem/17774)

**V 0.9.5 (3758789)**

*   Detect binary and large files in pending changes window
*   Add Sign-off option in pending changes window
*   Add context menu for ignoring files in pending changes window
*   Add commit list for file in commit details window
*   Add multiple lines of commit message display in commit details window
*   Add file blame function in commit details window
*   Bug fix: [Multiline Commit Message](http://gitscc.codeplex.com/workitem/17782)
*   Bug fix: [Enhance Pending Changes window](http://gitscc.codeplex.com/workitem/17781)
*   Bug fix: [Hang on click in items (.ico) in Pending changes Window](http://gitscc.codeplex.com/workitem/17776)

**V 0.9.4 (0cf9485)**

*   Detect Git for Windows Path in pending changes window
*   Add option to disable UTF-8 file names (for Git 1.7.10+)
*   Change pre-load to UI context instead of UIContextGuids.SolutionExists
*   Fix commit text box line height
*   Dragon Tool: Fix folder browse dialog for saving patches between two commits
*   Dragon Tool: Remove git console
*   Dragon Tool: Add custom Chrome

**V 0.9.3 (f609cdd)**

*   Bug Fix: [VS 11 Beta crash when reopening auto-hidden "GIT PENDING CHANGES" window](http://gitscc.codeplex.com/workitem/17538)
*   Bug Fix: [tags not displayed](http://gitscc.codeplex.com/workitem/17437)
*   Merge Pull Request: [Fix GitFileStatusTracker returning New instead of Ignored](https://github.com/yysun/Git-Source-Control-Provider/pull/26)
*   Merge Pull Request: [Handle files modified after being staged](https://github.com/yysun/Git-Source-Control-Provider/pull/24)
*   Dragon Tool: Display Author name instead of committer name

**V 0.9.2 (aa64b75)**

*   Dragon Tool: Add console with git command intellisense (with some limitations)
*   Dragon Tool: Add support to accept drag and drop file from file explorer
*   Dragon Tool: Add button to launch Git Bash
*   Bug Fix: [reserve staging checkboxes when refreshing](http://gitscc.codeplex.com/workitem/17379)
*   Bug Fix: [Missing Intellisense](http://gitscc.codeplex.com/workitem/17391)
*   Bug Fix: [Git history view scrolling and scrollbars](http://gitscc.codeplex.com/workitem/17276)
*   Bug Fix: [Simplified view mode Issue](http://gitscc.codeplex.com/workitem/17438)

**V0.9.1 (1def890)**

*   Change History Window to be a stand alone program - Dragon
    *   add/delete tag
    *   add/delete/checkout branch
    *   scroll to branch/tag
    *   refresh button
    *   search commits
    *   select and compare too commits
    *   Archive (export) commit
    *   create patch
*   Add Git Extensions menus to Pending Changed tool window
*   Add GitTortoise menus to Pending Changed tool window
*   Add Git - About ... menu, [Referencing Git hash when building](http://gitscc.codeplex.com/workitem/17051)
*   Add Option to disable auto load, [Automatically switch to the Git provider when loading a Git-controlled solution](http://gitscc.codeplex.com/workitem/16904)
*   Bug Fix: [autocrlf warning prevents commits from Git Pending Changes](http://gitscc.codeplex.com/workitem/17101)
*   Bug Fix: [Show Changes Window Disappear](http://gitscc.codeplex.com/workitem/17213)
*   Buf Fix: [Git status is not refreshed](http://gitscc.codeplex.com/workitem/17277)
*   Bug Fix: [config core.ignorecase true](http://gitscc.codeplex.com/workitem/17322)
*   Merge pull request #19 from GitHubki/Markdown)

Documentation
-----------------
[Source + Documentation](https://github.com/jzoss/Git-Source-Control-Provider)


