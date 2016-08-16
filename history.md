## Change Logs -- Visual Studio 2015

**V1.6.5**

- [x] Changed the icon for added files.
- [x] Added right click option to projects, to add them to the git repository, if not already added. 
- [x] Added option to auto-add files to repository when you add them to a project.
- [x] Publishing the solution from the status bar now adds all the projects and files to the repository.
- [x] Updated to Reactive Extensions 3.0. Should fix bug #54 
- [x] Options are now stored in the registry
- [x] Options Page should look better on high dpi screens. 


**V1.6.4**

* Reduced Install Package Size by 65% - Same Taste - Less Filling!
* Fix bug #45 - Changed files pane is not properly highlighting the selected item.

**V1.6.3**

*   Vastly Improved performance explicitly for larger projects
*   Added Tooltips on the status bar.
*   Clicking in the diffview now opens the line and column. 
*   Switching active git repistories from the status bar now updates the pending changes view.

**V1.6.2**

*   Fix to make work with Visual Studio Update 1


**V1.6.1**

*   Bug Fixes

**V1.6.0**

*   Added Repository controls on the status bar!
*   Clicking on line in diff window now opens the file to that line.
*   Improved performance with larger projects.
*   Can now initialize new projects from the status bar. 

**V1.5.1**
*   Fixed Issues #33,#34,#36,#38,#39-#42

**V1.5**

#####---New Features---
*   Added ability to update your .gitingore file to latest version from github
*   Added Track Active Item In Pending Changes - This will help those who have multiple repositories in one solution. It switch the pending changes to the repo for the active file.  
*   Added ability Filter Pending Changes To Only Show Solution Files


#####---Fixes---
*   Fixed issue where plugin could periodically crash Visual Studio.
*   Theme now switches with the need to resart.
*   Double clicking on files in Pending Changes now opens/switches to the file. 
*   No longer slows down when used with Resharper
*   Fixed issue #15, #21 where sometimes the plugin caused Visual Studio to crash. 
*   Fixed issue #16, Solution Explorer glyphs not updating. 

**V1.4.3**

*   Fixed Issues #14 -- Theme for Diff window now switches properly 


**V1.4.2**

*   Fixed Issues #1,#7,#8,#9,#10,#11, #12
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
*   Removed Git Diff Margin(https://visualstudiogallery.msdn.microsoft.com/cf49cf30-2ca6-4ea0-b7cc-6a8e0dadc1a8 )
*   Fixed file missing exceptions.


**V1.3.1**

*   Converted Solution to VS 2015
*   Created new project for plug-in, update all references to Visual Studio 14.0

## Change Logs -- Visual Studio 2013 and below

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
