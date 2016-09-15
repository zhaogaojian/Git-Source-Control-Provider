using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Schedulers;
using System.Windows.Forms;
using System.Windows.Threading;
using EnvDTE;
using GitScc.StatusBar;
using GitSccProvider;
using GitSccProvider.Utilities;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Threading;
using CancellationToken = System.Threading.CancellationToken;
using CommandID = System.ComponentModel.Design.CommandID;
using IgnoreFileManager = GitSccProvider.Utilities.IgnoreFileManager;
using Interlocked = System.Threading.Interlocked;
using Task = System.Threading.Tasks.Task;
using TaskContinuationOptions = System.Threading.Tasks.TaskContinuationOptions;
using TaskCreationOptions = System.Threading.Tasks.TaskCreationOptions;
using TaskScheduler = System.Threading.Tasks.TaskScheduler;
using Thread = System.Threading.Thread;
using ThreadPriority = System.Threading.ThreadPriority;
using GitScc.Utilities;

namespace GitScc
{
    [Guid("C4128D99-1000-41D1-A6C3-704E6C1A3DE2")]
    public partial class SccProviderService : 
        IVsSccProvider,
        IVsSccManagerTooltip,
        IVsSolutionEvents,
        IVsSolutionEvents2,
        IVsSccGlyphs,
        IDisposable,
        IVsUpdateSolutionEvents2,
        IVsTrackProjectDocumentsEvents2

    {
        private static readonly TimeSpan InitialRefreshDelay = TimeSpan.FromMilliseconds(500);
        

        private bool _active = false;
        private BasicSccProvider _sccProvider = null;
        private bool _solutionUpdating = false;
        private bool _updateQueued = false;
        private SccProviderSolutionCache _fileCache;
        private ConcurrentDictionary<GitRepository, GitChangesetManager> _fileChangesetManager;
        private GitApiStatusBarManager _statusBarManager;
        //private List<GitFileStatusTracker> trackers;


        #region SccProvider Service initialization/unitialization
        public SccProviderService(BasicSccProvider sccProvider)
        {
            this._sccProvider = sccProvider;
            _fileCache = new SccProviderSolutionCache(_sccProvider);
            _fileChangesetManager = new ConcurrentDictionary<GitRepository, GitChangesetManager>();

            RepositoryManager.Instance.FilesChanged += RepositoryManager_FilesChanged;
            RepositoryManager.Instance.FileStatusUpdate += RepositoryManager_FileStatusUpdate;
            RepositoryManager.Instance.SolutionTrackerBranchChanged += RepositoryManager_SolutionTrackerBranchChanged;
            RepositoryManager.Instance.CommitChanged += RepositoryManager_CommitChanged;

            //var mcs = sccProvider.GetService(typeof(IMenuCommandService)) as Microsoft.VisualStudio.Shell.OleMenuCommandService;
            _statusBarManager = new StandardGitStatusBarManager(
                GuidList.guidSccProviderCmdSet,
                PackageIds.cmdidBranchmenuStart,
                PackageIds.cmdidBranchMenuCommandStart,
                PackageIds.cmdidRepositorymenuStart,
                sccProvider,
                this);
            //this.trackers = trackers;
            SetupSolutionEvents();
            SetupDocumentEvents();
        }


        public void Dispose()
        {
            UnRegisterSolutionEvents();
            UnRegisterDocumentEvents();
        }
        #endregion


        #region IVsSccProvider interface functions
        /// <summary>
        /// Returns whether this source control provider is the active scc provider.
        /// </summary>
        public bool Active
        {
            get { return _active; }
        }

        // Called by the scc manager when the provider is activated. 
        // Make visible and enable if necessary scc related menu commands
        public int SetActive()
        {
            Trace.WriteLine(String.Format(CultureInfo.CurrentUICulture, "Git Source Control Provider set active"));
            _active = true;
            GlobalCommandHook hook = GlobalCommandHook.GetInstance(_sccProvider);
            hook.HookCommand(new CommandID(VSConstants.VSStd2K, (int)VSConstants.VSStd2KCmdID.SLNREFRESH), HandleSolutionRefresh);

            ThreadHelper.JoinableTaskFactory.Run(async delegate
            {
                await OpenTracker();
                await RefreshSolution();
            });
            //MarkDirty(false);
            return VSConstants.S_OK;
        }

        // Called by the scc manager when the provider is deactivated. 
        // Hides and disable scc related menu commands
        public int SetInactive()
        {
            Trace.WriteLine(String.Format(CultureInfo.CurrentUICulture, "Git Source Control Provider set inactive"));
            _active = false;
            DisableSccForSolution();
            GlobalCommandHook hook = GlobalCommandHook.GetInstance(_sccProvider);
            hook.UnhookCommand(new CommandID(VSConstants.VSStd2K, (int)VSConstants.VSStd2KCmdID.SLNREFRESH), HandleSolutionRefresh);
            return VSConstants.S_OK;
        }

        public int AnyItemsUnderSourceControl(out int pfResult)
        {
            if (!_active || RepositoryManager.Instance.SolutionTracker == null)
            {
                pfResult = 0;
            }
            else
            {
                    // Although the parameter is an int, it's in reality a BOOL value, so let's return 0/1 values
                pfResult = 1; //RepositoryManager.Instance.Active ? 1 : 0;
            }
            //pfResult = 0;
            return VSConstants.S_OK;
        }
        #endregion

        private async Task RefreshSolution()
        {
            await EnableSccForSolution();
            await ReloadAllGlyphs();
        }

        //TODO : FIX!!!!!
        private async void HandleSolutionRefresh(object sender, EventArgs e)
        {
           await RefreshSolution();
        }

        private async Task ReloadAllGlyphs()
        {
            var projects = await SolutionExtensions.GetLoadedControllableProjects();
            await Task.Run(async delegate
            {
                await RefreshSolutionGlyphs();
                await RefreshProjectGlyphs(projects);
            });
        }
        private async Task EnableSccForSolution()
        {
            await RegisterEntireSolution();
            await SetSolutionExplorerTitle();
            await _statusBarManager.SetActiveRepository(RepositoryManager.Instance.SolutionTracker);
            //await _statusBarManager.
        }

        private void DisableSccForSolution()
        {
            CloseTracker();
            _fileCache = null;
            _fileChangesetManager = null;
        }


        #region File Names

        private async Task SetSolutionExplorerTitle(string message)
        {

            await ThreadHelper.JoinableTaskFactory.RunAsync(
                VsTaskRunContext.UIThreadBackgroundPriority,
                async delegate
                {
                    // On caller's thread. Switch to main thread (if we're not already there).
                    await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                    // Now on UI thread via background priority.
                    await Task.Yield();
                    // Resumed on UI thread, also via background priority.
                    var dte = (DTE)_sccProvider.GetService(typeof(DTE));
                    dte.Windows.Item(EnvDTE.Constants.vsWindowKindSolutionExplorer).Caption = message;
                });
        }

        #region Remove 


        /// <summary>
        /// Returns the filename of the solution
        /// </summary>
        public async Task<string> GetSolutionFileName()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            IVsSolution sol = (IVsSolution)_sccProvider.GetService(typeof(SVsSolution));
            string solutionDirectory, solutionFile, solutionUserOptions;
            if (sol.GetSolutionInfo(out solutionDirectory, out solutionFile, out solutionUserOptions) == VSConstants.S_OK)
            {
                return solutionFile;
            }
            else
            {
                return null;
            }
        }

        private async Task<string> GetProjectFileName(IVsHierarchy hierHierarchy)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            if (!(hierHierarchy is IVsSccProject2)) return await GetSolutionFileName();

            var files = await GetNodeFiles(hierHierarchy as IVsSccProject2, VSConstants.VSITEMID_ROOT);
            string fileName = files.Count <= 0 ? null : files[0];

            //try hierHierarchy.GetCanonicalName to get project name for web site
            if (fileName == null)
            {
                if (hierHierarchy.GetCanonicalName(VSConstants.VSITEMID_ROOT, out fileName) != VSConstants.S_OK) return null;
                return GetCaseSensitiveFileName(fileName);
            }
            return fileName;
        }

        /// <summary>
        /// Returns a list of source controllable files associated with the specified node
        /// </summary>
        private async Task<IList<string>> GetNodeFiles(IVsSccProject2 pscp2, uint itemid)
        {
            // NOTE: the function returns only a list of files, containing both regular files and special files
            // If you want to hide the special files (similar with solution explorer), you may need to return 
            // the special files in a hastable (key=master_file, values=special_file_list)

            // Initialize output parameters
            IList<string> sccFiles = new List<string>();
            if (pscp2 != null)
            {
                CALPOLESTR[] pathStr = new CALPOLESTR[1];
                CADWORD[] flags = new CADWORD[1];
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                if (pscp2.GetSccFiles(itemid, pathStr, flags) == 0)
                {
                    for (int elemIndex = 0; elemIndex < pathStr[0].cElems; elemIndex++)
                    {
                        IntPtr pathIntPtr = Marshal.ReadIntPtr(pathStr[0].pElems, elemIndex);


                        String path = Marshal.PtrToStringAuto(pathIntPtr);
                        sccFiles.Add(path);

                        // See if there are special files
                        if (flags.Length > 0 && flags[0].cElems > 0)
                        {
                            int flag = Marshal.ReadInt32(flags[0].pElems, elemIndex);

                            if (flag != 0)
                            {
                                // We have special files
                                CALPOLESTR[] specialFiles = new CALPOLESTR[1];
                                CADWORD[] specialFlags = new CADWORD[1];

                                pscp2.GetSccSpecialFiles(itemid, path, specialFiles, specialFlags);
                                for (int i = 0; i < specialFiles[0].cElems; i++)
                                {
                                    IntPtr specialPathIntPtr = Marshal.ReadIntPtr(specialFiles[0].pElems, i * IntPtr.Size);
                                    String specialPath = Marshal.PtrToStringAuto(specialPathIntPtr);

                                    sccFiles.Add(specialPath);
                                    Marshal.FreeCoTaskMem(specialPathIntPtr);
                                }

                                if (specialFiles[0].cElems > 0)
                                {
                                    Marshal.FreeCoTaskMem(specialFiles[0].pElems);
                                }
                            }
                        }

                        Marshal.FreeCoTaskMem(pathIntPtr);

                    }
                    if (pathStr[0].cElems > 0)
                    {
                        Marshal.FreeCoTaskMem(pathStr[0].pElems);
                    }
                }
            }
            else if (itemid == VSConstants.VSITEMID_ROOT)
            {
                sccFiles.Add(await GetSolutionFileName());
            }

            return sccFiles;
        }

        private static string GetCaseSensitiveFileName(string fileName)
        {
            if (fileName == null) return fileName;

            if (Directory.Exists(fileName) || File.Exists(fileName))
            {
                try
                {
                    StringBuilder sb = new StringBuilder(1024);
                    GetShortPathName(fileName.ToUpper(), sb, 1024);
                    GetLongPathName(sb.ToString(), sb, 1024);
                    var fn = sb.ToString();
                    return string.IsNullOrWhiteSpace(fn) ? fileName : fn;
                }
                catch { }
            }

            return fileName;
        }

        #endregion

        private async Task<string> GetFileName(IVsHierarchy hierHierarchy, uint itemidNode)
        {
            if (itemidNode == VSConstants.VSITEMID_ROOT)
            {
                if (hierHierarchy == null)
                    return await GetSolutionFileName();
                else
                    return await GetProjectFileName(hierHierarchy);
            }
            else
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                string fileName = null;
                if (hierHierarchy.GetCanonicalName(itemidNode, out fileName) != VSConstants.S_OK) return null;
                return GetCaseSensitiveFileName(fileName);
            }
        }



        [DllImport("kernel32.dll")]
        static extern uint GetShortPathName(string longpath, StringBuilder sb, int buffer);

        [DllImport("kernel32.dll")]
        static extern uint GetLongPathName(string shortpath, StringBuilder sb, int buffer);


       


        /// <summary>
        /// Gets the list of directly selected VSITEMSELECTION objects
        /// </summary>
        /// <returns>A list of VSITEMSELECTION objects</returns>
        private async Task<IList<VSITEMSELECTION>> GetSelectedNodes()
        {
            // Retrieve shell interface in order to get current selection
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            IVsMonitorSelection monitorSelection = _sccProvider.GetService(typeof(IVsMonitorSelection)) as IVsMonitorSelection;

            Debug.Assert(monitorSelection != null, "Could not get the IVsMonitorSelection object from the services exposed by this project");

            if (monitorSelection == null)
            {
                throw new InvalidOperationException();
            }

            List<VSITEMSELECTION> selectedNodes = new List<VSITEMSELECTION>();
            IntPtr hierarchyPtr = IntPtr.Zero;
            IntPtr selectionContainer = IntPtr.Zero;
            try
            {
                // Get the current project hierarchy, project item, and selection container for the current selection
                // If the selection spans multiple hierachies, hierarchyPtr is Zero
                uint itemid;
                IVsMultiItemSelect multiItemSelect = null;
                ErrorHandler.ThrowOnFailure(monitorSelection.GetCurrentSelection(out hierarchyPtr, out itemid, out multiItemSelect, out selectionContainer));

                if (itemid != VSConstants.VSITEMID_SELECTION)
                {
                    // We only care if there are nodes selected in the tree
                    if (itemid != VSConstants.VSITEMID_NIL)
                    {
                        if (hierarchyPtr == IntPtr.Zero)
                        {
                            // Solution is selected
                            VSITEMSELECTION vsItemSelection;
                            vsItemSelection.pHier = null;
                            vsItemSelection.itemid = itemid;
                            selectedNodes.Add(vsItemSelection);
                        }
                        else
                        {
                            IVsHierarchy hierarchy = (IVsHierarchy)Marshal.GetObjectForIUnknown(hierarchyPtr);
                            // Single item selection
                            VSITEMSELECTION vsItemSelection;
                            vsItemSelection.pHier = hierarchy;
                            vsItemSelection.itemid = itemid;
                            selectedNodes.Add(vsItemSelection);
                        }
                    }
                }
                else
                {
                    if (multiItemSelect != null)
                    {
                        // This is a multiple item selection.

                        //Get number of items selected and also determine if the items are located in more than one hierarchy
                        uint numberOfSelectedItems;
                        int isSingleHierarchyInt;
                        ErrorHandler.ThrowOnFailure(multiItemSelect.GetSelectionInfo(out numberOfSelectedItems, out isSingleHierarchyInt));
                        bool isSingleHierarchy = (isSingleHierarchyInt != 0);

                        // Now loop all selected items and add them to the list 
                        Debug.Assert(numberOfSelectedItems > 0, "Bad number of selected itemd");
                        if (numberOfSelectedItems > 0)
                        {
                            VSITEMSELECTION[] vsItemSelections = new VSITEMSELECTION[numberOfSelectedItems];
                            ErrorHandler.ThrowOnFailure(multiItemSelect.GetSelectedItems(0, numberOfSelectedItems, vsItemSelections));
                            foreach (VSITEMSELECTION vsItemSelection in vsItemSelections)
                            {
                                selectedNodes.Add(vsItemSelection);
                            }
                        }
                    }
                }
            }
            finally
            {
                if (hierarchyPtr != IntPtr.Zero)
                {
                    Marshal.Release(hierarchyPtr);
                }
                if (selectionContainer != IntPtr.Zero)
                {
                    Marshal.Release(selectionContainer);
                }
            }

            return selectedNodes;
        }

        #endregion

        #region open and close tracker

        internal async Task OpenTracker()
        {
            Debug.WriteLine("==== Open Tracker");
            RepositoryManager.Instance.Clear();
            _fileCache = new SccProviderSolutionCache(_sccProvider);
            _fileChangesetManager = new ConcurrentDictionary<GitRepository, GitChangesetManager>();

            var solutionFileName = await GetSolutionFileName();
            await RepositoryManager.Instance.SetSolutionTracker(solutionFileName);
            await _statusBarManager.SetActiveRepository(RepositoryManager.Instance.SolutionTracker);
            await SetSolutionExplorerTitle();
            RepositoryName = RepositoryManager.Instance?.SolutionTracker?.Name;

            if (!string.IsNullOrEmpty(solutionFileName))
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                var projects = await SolutionExtensions.GetLoadedControllableProjects();
                foreach (var vsSccProject2 in projects)
                {
                    await AddProject(vsSccProject2 as IVsHierarchy);
                }
            }
        }

        private async void RepositoryManager_FileStatusUpdate(object sender, GitFilesStatusUpdateEventArgs e)
        {

            try
            {
                ThreadHelper.JoinableTaskFactory.Run(async delegate {
                    await ProcessFileStatusUpdate((GitRepository)sender, e);
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error in RepositoryManager_FileChanged: " + ex.Message);
            }
        }

        private async void RepositoryManager_FilesChanged(object sender, GitFilesUpdateEventArgs e)
        {
            try
            {
                await TaskScheduler.Default;
                await ProcessMultiFileChange((GitRepository) sender, e);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error in RepositoryManager_FilesChanged: " + ex.Message);
            }
        }

        private async void RepositoryManager_SolutionTrackerBranchChanged(object sender, string e)
        {
            try
            {
                //TODO LOG
                await SetSolutionExplorerTitle();
            }
            catch (Exception)
            {
            }
        }


        private async void RepositoryManager_CommitChanged(object sender, GitRepositoryEvent e)
        {
            try
            {
                //TODO LOG
                await RefreshSolution();
            }
            catch (Exception)
            {

                throw;
            }
        }

        private async Task SetSolutionExplorerTitle()
        {
            var caption = "Solution Explorer";
            BranchName = RepositoryManager.Instance?.SolutionTracker?.CurrentBranchDisplayName;
            if (!string.IsNullOrEmpty(BranchName))
            {
                caption += " (" + BranchName + ")";
                await SetSolutionExplorerTitle(caption);
            }
        }

        private async Task RegisterEntireSolution()
        {
            var registerSolution = _fileCache?.RegisterSolution();
            if (registerSolution != null) await registerSolution;
        }


        private async Task ProcessFileStatusUpdate(GitRepository repo, GitFilesStatusUpdateEventArgs e)
        {
            //await UpdateSolutionFiles(repo, e.Files);
            await UpdateSolutionFileStatus(repo, e.Files);
        }

        private async Task UpdateSolutionFiles(GitRepository repo, List<GitFile> files, bool force = false)
        {
            if (!_solutionUpdating || force)
            {
                _solutionUpdating = true;
                await UpdateSolutionFileStatus(repo, files);
                if (_updateQueued)
                {
                    _updateQueued = false;
                    await UpdateSolutionFiles(repo, repo.ChangedFiles.ToList(),true);
                }
                _solutionUpdating = false;
            }
            else
            {
                _updateQueued = true;
            }
        }

        private async Task UpdateSolutionFileStatus(GitRepository repo, List<GitFile> files)
        {
            HashSet<IVsSccProject2> nodes = new HashSet<IVsSccProject2>();
            var changeSet = GetChangesetManager(repo).LoadChangeSet(files);
            foreach (var file in changeSet)
            {
                ////if (_fileCache.StatusChanged(file.Key.t, file.Status))
                ////{

                var items = _fileCache.GetProjectsSelectionForFile(file.Key);
                if (items != null)
                {
                    foreach (var vsitemselection in items)
                    {
                        nodes.Add(vsitemselection);
                    }
                }
            }
            await Task.Run(async delegate
            {
                await RefreshProjectGlyphs(nodes.ToList());
            });
        }

        private async Task ProcessMultiFileChange(GitRepository repo, GitFilesUpdateEventArgs e)
        {
            //lock (_glyphsLock)
            //{
                HashSet<IVsSccProject2> nodes = new HashSet<IVsSccProject2>();
                foreach (var file in e.Files)
                {
                    var items = _fileCache.GetProjectsSelectionForFile(file);
                    if (items != null)
                    {
                        foreach (var vsitemselection in items)
                        {
                           
                            nodes.Add(vsitemselection);
                        }
                    }
                }
                if (nodes.Count > 0)
                {
                Debug.WriteLine("Updating Multiple Files");
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                foreach (var project in nodes)
                {
                   
                    project.SccGlyphChanged(0, null, null, null);
                }
            }

                //todo maybe move this
                var caption = "Solution Explorer";
                string branch = await GetCurrentBranchNameAsync();
                if (!string.IsNullOrEmpty(branch))
                {
                    caption += " (" + branch + ")";
                    SetSolutionExplorerTitle(caption);
                }
           // }
        }

        private void CloseTracker()
        {
            Debug.WriteLine("==== Close Tracker");
            RepositoryManager.Instance.Clear();
        }

        #endregion

        #region Compare and undo

        internal bool CanCompareSelectedFile
        {
            get
            {
                string fileName = ThreadHelper.JoinableTaskFactory.Run(async delegate
                {
                    string value = await GetSelectFileName();
                    return value;
                });
                GitFileStatus status = GetFileStatus(fileName);
                return status == GitFileStatus.Modified || status == GitFileStatus.Staged;
            }
        }

        internal async Task<string> GetSelectFileName()
        {
            var selectedNodes = await GetSelectedNodes();
            if (selectedNodes.Count <= 0) return null;
            return await GetFileName(selectedNodes[0].pHier, selectedNodes[0].itemid);
        }

        internal async Task CompareSelectedFile()
        {
            var fileName = await GetSelectFileName();
            await CompareFile(fileName);
        }


        //TODO mAybe move this to a static git helper
        internal async Task CompareFile(string filename)
        {
            var repo = GetTracker(filename);
            await _sccProvider.RunDiffCommand(GitCommands.GenerateDiffFileInfo(repo,filename));
        }

        internal async Task UndoSelectedFile()
        {
            var fileName = await GetSelectFileName();
            UndoFileChanges(fileName);
        }

        //TODO FIX
        internal void UndoFileChanges(string fileName)
        {
            GitFileStatus status = GetFileStatus(fileName);
            if (status == GitFileStatus.Modified || status == GitFileStatus.Staged ||
                status == GitFileStatus.Deleted || status == GitFileStatus.Removed)
            {
                var deleteMsg = "";
                if (status == GitFileStatus.Deleted || status == GitFileStatus.Removed)
                {
                    deleteMsg = @"

Note: you will need to click 'Show All Files' in solution explorer to see the file.";
                }
                
                if (MessageBox.Show("Are you sure you want to undo changes for " + Path.GetFileName(fileName) +
                    " and restore a version from the last commit? " + deleteMsg,
                    "Undo Changes", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    CurrentTracker.UndoFileChanges(fileName);
                    
                    //SaveFileFromRepository(fileName, fileName);
                    if (status == GitFileStatus.Staged || status == GitFileStatus.Removed)
                    {
                        CurrentTracker.UnStageFile(fileName);
                    }
                    //CurrentTracker.CheckOutFile(fileName);
                }
            }
        }

        internal void EditIgnore()
        {
            if (this.CurrentTracker != null && this.CurrentTracker.IsGit)
            {
                var dte = BasicSccProvider.GetServiceEx<EnvDTE.DTE>();
                var fn = Path.Combine(this.CurrentTracker.WorkingDirectory, ".gitignore");
                if (!File.Exists(fn)) File.WriteAllText(fn, "# git ignore file");
                dte.ItemOperations.OpenFile(fn);
            }
        }

        #endregion

      

        #region project trackers
        private async Task AddProject(IVsHierarchy pHierarchy)
        {
            string projectName = await GetProjectFileName(pHierarchy);

            if (string.IsNullOrEmpty(projectName)) return;
            string projectDirecotry = Path.GetDirectoryName(projectName);
            RepositoryManager.Instance.GetTrackerForPath(projectDirecotry);
        }

        internal string CurrentBranchName
        {
            get
            {
                GitFileStatusTracker tracker = CurrentTracker;
                return tracker != null ? tracker.CurrentBranchDisplayName : null;
            }
        }

        public async Task<string> GetCurrentBranchNameAsync()
        {
            var tracker = await GetCurrentTrackerAsync();
            return tracker != null ? tracker.CurrentBranchDisplayName : null;
        }

        internal string CurrentWorkingDirectory
        {
            get
            {
                GitFileStatusTracker tracker = CurrentTracker;
                return tracker != null ? tracker.WorkingDirectory : null;
            }
        }

        internal GitFileStatusTracker CurrentTracker
        {
            get
            {
                var filename = ThreadHelper.JoinableTaskFactory.Run(async delegate
                {
                    return await GetSelectFileName();
                });
                return RepositoryManager.Instance.GetTrackerForPath(filename);
            }
        }

        internal async  Task<GitFileStatusTracker> GetCurrentTrackerAsync()
        {
            var filename = await GetSelectFileName();
            return await RepositoryManager.Instance.GetTrackerForPathAsync(filename);
        }

        internal async Task<GitFileStatusTracker> GetSolutionTracker()
        {
            return GetTracker(await GetSolutionFileName());
        }


        //TODO :  I don't like this.. 
        internal GitFileStatusTracker GetTracker(string fileName)
        {
            return RepositoryManager.Instance.GetTrackerForPath(fileName);
        }

        public static bool IsFileBelowDirectory(string fileInfo, string directoryInfo, string separator)
        {
            var directoryPath = string.Format("{0}{1}"
            , directoryInfo
            , directoryInfo.EndsWith(separator) ? "" : separator);

            return fileInfo.StartsWith(directoryPath, StringComparison.OrdinalIgnoreCase);
        }
        
        private GitFileStatus GetFileStatus(string fileName)
        {
            var tracker = GetTracker(fileName);
            var status = GitFileStatus.NotControlled;
            if (tracker != null)
            {
                status = tracker.GetFileStatus(fileName);
                var cm = GetChangesetManager(tracker);
                cm?.SetStatus(fileName,status);
                //cm.SetStatus(fileName, status);
                //status = cm?.GetFileStatus(fileName) ?? GitFileStatus.NotControlled;
            }
            return status;
        }

        private async Task<GitFileStatus> GetFileStatus(IVsHierarchy phierHierarchy, uint itemidNode)
        {
            var fileName = await GetFileName(phierHierarchy, itemidNode);
            return GetFileStatus(fileName);
        }

        private GitChangesetManager GetChangesetManager(GitRepository repo)
        {
            GitChangesetManager manager;
            if (!_fileChangesetManager.TryGetValue(repo, out manager))
            {
                manager = new GitChangesetManager();
                _fileChangesetManager.TryAdd(repo, manager);
            }
            return manager;
        }

        #endregion

        public async Task QuickRefreshNodesGlyphs(IVsSccProject2 project, List<string> files)
        {
            try
            {
                if (files.Count > 0)
                {
                    string[] rgpszFullPaths = new string[files.Count];
                    for (int i = 0; i < files.Count; i++)
                        rgpszFullPaths[i] = files[i];

                    VsStateIcon[] rgsiGlyphs = new VsStateIcon[files.Count];
                    uint[] rgdwSccStatus = new uint[files.Count];
                    GetSccGlyph(files.Count, rgpszFullPaths, rgsiGlyphs, rgdwSccStatus);

                    uint[] rguiAffectedNodes = new uint[files.Count];

                    //TODO We could/Should cache this mapping !!!! 
                    await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                    IList<uint> subnodes = await SolutionExtensions.GetProjectItems((IVsHierarchy)project, VSConstants.VSITEMID_ROOT);

                    var dict = new Dictionary<string, uint>();
                    var proj = project as IVsProject2;

                    foreach (var id in subnodes)
                    {
                        string docname;
                        var res = proj.GetMkDocument(id, out docname);

                        if (res == VSConstants.S_OK && !string.IsNullOrEmpty(docname))
                            dict[docname] = id;
                    }

                    for (int i = 0; i < files.Count; ++i)
                    {
                        uint id;
                        if (dict.TryGetValue(files[i], out id))
                        {
                            rguiAffectedNodes[i] = id;
                        }
                    }
                    await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                    project.SccGlyphChanged(files.Count, rguiAffectedNodes, rgsiGlyphs, rgdwSccStatus);
                }
            }
            catch (Exception ex)
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                IVsActivityLog log = _sccProvider.GetService(typeof(SVsActivityLog)) as IVsActivityLog;
                if (log == null) return;

                int hr = log.LogEntry((UInt32)__ACTIVITYLOG_ENTRYTYPE.ALE_ERROR,
                    ex.StackTrace,
                    string.Format(CultureInfo.CurrentCulture,
                    "Called for: {0}", this.ToString()));
            }

        }

        private async Task RefreshSolutionGlyphs()
        {
            await ThreadHelper.JoinableTaskFactory.RunAsync(VsTaskRunContext.UIThreadBackgroundPriority,
                    async delegate
                    {
                    // On caller's thread. Switch to main thread (if we're not already there).
                    await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

                    // Now on UI thread via background priority.
                    await Task.Yield();
                    string[] rgpszFullPaths = new string[1];
                    rgpszFullPaths[0] = await GetSolutionFileName();
                    VsStateIcon[] rgsiGlyphs = new VsStateIcon[1];
                    uint[] rgdwSccStatus = new uint[1];
                    GetSccGlyph(1, rgpszFullPaths, rgsiGlyphs, rgdwSccStatus);

                    // Set the solution's glyph directly in the hierarchy
                    IVsHierarchy solHier = (IVsHierarchy)_sccProvider.GetService(typeof(SVsSolution));
                    solHier.SetProperty(VSConstants.VSITEMID_ROOT, (int)__VSHPROPID.VSHPROPID_StateIconIndex, rgsiGlyphs[0]);
                });
         
        }

        public async Task RefreshProjectGlyphs(List<IVsSccProject2> projects)
        {

            await ThreadHelper.JoinableTaskFactory.RunAsync(VsTaskRunContext.UIThreadBackgroundPriority,
                async delegate
            {
              // On caller's thread. Switch to main thread (if we're not already there).
              await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

              // Now on UI thread via background priority.
              await Task.Yield();
              foreach (var project in projects)
              {
                  if (project != null)
                  {
                      project.SccGlyphChanged(0, null, null, null);
                  }
                  await Task.Yield();
              }
              // Resumed on UI thread, also via background priority.
                });
        }

        //TODO Move TO Provider
        /// <summary>
        /// Refreshes the glyphs of the specified hierarchy nodes
        /// </summary>
        public async Task RefreshNodesGlyphs(IList<VSITEMSELECTION> selectedNodes)
        {
            foreach (VSITEMSELECTION vsItemSel in selectedNodes)
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                IVsSccProject2 sccProject2 = vsItemSel.pHier as IVsSccProject2;
                if (vsItemSel.itemid == VSConstants.VSITEMID_ROOT)
                {
                    if (sccProject2 == null)
                    {
                        // Note: The solution's hierarchy does not implement IVsSccProject2, IVsSccProject interfaces
                        // It may be a pain to treat the solution as special case everywhere; a possible workaround is 
                        // to implement a solution-wrapper class, that will implement IVsSccProject2, IVsSccProject and
                        // IVsHierarhcy interfaces, and that could be used in provider's code wherever a solution is needed.
                        // This approach could unify the treatment of solution and projects in the provider's code.

                        // Until then, solution is treated as special case
                        string[] rgpszFullPaths = new string[1];
                        rgpszFullPaths[0] = await GetSolutionFileName();
                        VsStateIcon[] rgsiGlyphs = new VsStateIcon[1];
                        uint[] rgdwSccStatus = new uint[1];
                        GetSccGlyph(1, rgpszFullPaths, rgsiGlyphs, rgdwSccStatus);

                        // Set the solution's glyph directly in the hierarchy
                        IVsHierarchy solHier = (IVsHierarchy)_sccProvider.GetService(typeof(SVsSolution));
                        solHier.SetProperty(VSConstants.VSITEMID_ROOT, (int)__VSHPROPID.VSHPROPID_StateIconIndex, rgsiGlyphs[0]);
                    }
                    else
                    {
                        // Refresh all the glyphs in the project; the project will call back GetSccGlyph() 
                        // with the files for each node that will need new glyph
                        sccProject2.SccGlyphChanged(0, null, null, null);
                    }
                }
                else
                {
                    // It may be easier/faster to simply refresh all the nodes in the project, 
                    // and let the project call back on GetSccGlyph, but just for the sake of the demo, 
                    // let's refresh ourselves only one node at a time
                    IList<string> sccFiles = await GetNodeFiles(sccProject2, vsItemSel.itemid);

                    // We'll use for the node glyph just the Master file's status (ignoring special files of the node)
                    if (sccFiles.Count > 0)
                    {
                        string[] rgpszFullPaths = new string[1];
                        rgpszFullPaths[0] = sccFiles[0];
                        VsStateIcon[] rgsiGlyphs = new VsStateIcon[1];
                        uint[] rgdwSccStatus = new uint[1];
                        GetSccGlyph(1, rgpszFullPaths, rgsiGlyphs, rgdwSccStatus);

                        uint[] rguiAffectedNodes = new uint[1];
                        rguiAffectedNodes[0] = vsItemSel.itemid;
                        sccProject2.SccGlyphChanged(1, rguiAffectedNodes, rgsiGlyphs, rgdwSccStatus);
                    }
                }
            }
        }
//#endregion

        #region git
        public bool IsSolutionGitControlled
        {
            get { return RepositoryManager.Instance.GetRepositories().Count > 0; }
        }
        public bool FileTracked(string filename)
        {
            return _fileCache.FileTracked(filename.ToLower());
        }

        internal async Task InitRepo()
        {
            await GitCommandWrappers.InitRepo(await GetSolutionFileName());
            SolutionExtensions.WriteMessageToOutputPane("Enabling SCC Provider");

            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            var projects = await SolutionExtensions.GetLoadedControllableProjects();
            SolutionExtensions.WriteMessageToOutputPane("Adding Projects To git");
            foreach (var vsSccProject2 in projects)
            {
               await AddProjectToSourceControl(vsSccProject2);
            }
            await OpenTracker();
            await RefreshSolution();
            SolutionExtensions.WriteMessageToOutputPane("Done");
        }
        
        internal async Task AddProjectToSourceControl(IVsSccProject2 project)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            string projectName = await GetProjectFileName(project as IVsHierarchy);

            if (string.IsNullOrEmpty(projectName)) return;
            string projectDirecotry = Path.GetDirectoryName(projectName);
            var repo = RepositoryManager.Instance.GetTrackerForPath(projectDirecotry);
            repo.AddFile(projectName);
            var files = await SolutionExtensions.GetProjectFiles(project);
            foreach (var file in files)
            {
                repo.AddFile(file);
            }
        } 
        #endregion
    }
}