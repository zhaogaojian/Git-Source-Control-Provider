using System;
using System.Collections.Generic;
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
using GitSccProvider;
using GitSccProvider.Utilities;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Threading;
using CancellationToken = System.Threading.CancellationToken;
using CommandID = System.ComponentModel.Design.CommandID;
using Interlocked = System.Threading.Interlocked;
using Task = System.Threading.Tasks.Task;
using TaskContinuationOptions = System.Threading.Tasks.TaskContinuationOptions;
using TaskCreationOptions = System.Threading.Tasks.TaskCreationOptions;
using TaskScheduler = System.Threading.Tasks.TaskScheduler;
using Thread = System.Threading.Thread;
using ThreadPriority = System.Threading.ThreadPriority;

namespace GitScc
{
    [Guid("C4128D99-1000-41D1-A6C3-704E6C1A3DE2")]
    public partial class SccProviderService : IVsSccProvider,
        IVsSccManager3,
        IVsSccManagerTooltip,
        IVsSolutionEvents,
        IVsSolutionEvents2,
        IVsSccGlyphs,
        IDisposable,
        IVsUpdateSolutionEvents2,
        IVsTrackProjectDocumentsEvents2
        
    {
        //private static readonly QueuedTaskScheduler _queuedTaskScheduler =
        //    new QueuedTaskScheduler(1, threadName: "Git SCC Tasks", threadPriority: ThreadPriority.BelowNormal);
        //private static readonly TaskScheduler _taskScheduler = _queuedTaskScheduler.ActivateNewQueue();

        private static readonly TimeSpan InitialRefreshDelay = TimeSpan.FromMilliseconds(500);
        private static TimeSpan RefreshDelay = InitialRefreshDelay;

        private bool _active = false;
        private BasicSccProvider _sccProvider = null;
        private SccProviderSolutionCache _fileCache;
        private object _glyphsLock = new object();
        //private List<GitFileStatusTracker> trackers;


        #region SccProvider Service initialization/unitialization
        public SccProviderService(BasicSccProvider sccProvider)
        {
            this._sccProvider = sccProvider;
            _fileCache = new SccProviderSolutionCache(_sccProvider);

            RepositoryManager.Instance.FileChanged += RepositoryManager_FileChanged;
            RepositoryManager.Instance.FilesChanged += RepositoryManager_FilesChanged;
            RepositoryManager.Instance.SolutionTrackerBranchChanged += RepositoryManager_SolutionTrackerBranchChanged;
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

        //public static TaskScheduler TaskScheduler
        //{
        //    get
        //    {
        //        return _taskScheduler;
        //    }
        //}

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

            //MarkDirty(false);
            return VSConstants.S_OK;
        }

        // Called by the scc manager when the provider is deactivated. 
        // Hides and disable scc related menu commands
        public int SetInactive()
        {
            Trace.WriteLine(String.Format(CultureInfo.CurrentUICulture, "Git Source Control Provider set inactive"));
            _active = false;

            GlobalCommandHook hook = GlobalCommandHook.GetInstance(_sccProvider);
            hook.UnhookCommand(new CommandID(VSConstants.VSStd2K, (int)VSConstants.VSStd2KCmdID.SLNREFRESH), HandleSolutionRefresh);

            CloseTracker();
            MarkDirty(false);
            return VSConstants.S_OK;
        }

        public int AnyItemsUnderSourceControl(out int pfResult)
        {
            pfResult = 0;
            return VSConstants.S_OK;
        }
        #endregion

        private void HandleSolutionRefresh(object sender, EventArgs e)
        {
            Refresh();
        }

        #region IVsSccManager2 interface functions

        public int BrowseForProject(out string pbstrDirectory, out int pfOK)
        {
            // Obsolete method
            pbstrDirectory = null;
            pfOK = 0;
            return VSConstants.E_NOTIMPL;
        }

        public int CancelAfterBrowseForProject()
        {
            // Obsolete method
            return VSConstants.E_NOTIMPL;
        }

        /// <summary>
        /// Returns whether the source control provider is fully installed
        /// </summary>
        public int IsInstalled(out int pbInstalled)
        {
            // All source control packages should always return S_OK and set pbInstalled to nonzero
            pbInstalled = 1;
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Provide source control icons for the specified files and returns scc status of files
        /// </summary>
        /// <returns>The method returns S_OK if at least one of the files is controlled, S_FALSE if none of them are</returns>
        public int GetSccGlyph([InAttribute] int cFiles, [InAttribute] string[] rgpszFullPaths, [OutAttribute] VsStateIcon[] rgsiGlyphs, [OutAttribute] uint[] rgdwSccStatus)
        {
            for (int i = 0; i < cFiles; i++)
            {
                GitFileStatus status = _active ? GetFileStatus(rgpszFullPaths[i]) : GitFileStatus.NotControlled;
                __SccStatus sccStatus;

                switch (status)
                {
                    case GitFileStatus.Tracked:
                        rgsiGlyphs[i] = SccGlyphsHelper.Tracked;
                        sccStatus = __SccStatus.SCC_STATUS_CONTROLLED;
                        break;

                    case GitFileStatus.Modified:
                        rgsiGlyphs[i] = SccGlyphsHelper.Modified;
                        sccStatus = __SccStatus.SCC_STATUS_CONTROLLED | __SccStatus.SCC_STATUS_CHECKEDOUT | __SccStatus.SCC_STATUS_OUTBYUSER;
                        break;

                    case GitFileStatus.New:
                        rgsiGlyphs[i] = SccGlyphsHelper.New;
                        sccStatus = __SccStatus.SCC_STATUS_CONTROLLED | __SccStatus.SCC_STATUS_CHECKEDOUT | __SccStatus.SCC_STATUS_OUTBYUSER;
                        break;

                    case GitFileStatus.Added:
                    case GitFileStatus.Staged:
                        rgsiGlyphs[i] = status == GitFileStatus.Added ? SccGlyphsHelper.Added : SccGlyphsHelper.Staged;
                        sccStatus = __SccStatus.SCC_STATUS_CONTROLLED | __SccStatus.SCC_STATUS_CHECKEDOUT | __SccStatus.SCC_STATUS_OUTBYUSER;
                        break;

                    case GitFileStatus.NotControlled:
                        rgsiGlyphs[i] = SccGlyphsHelper.NotControlled;
                        sccStatus = __SccStatus.SCC_STATUS_NOTCONTROLLED;
                        break;

                    case GitFileStatus.Ignored:
                        rgsiGlyphs[i] = SccGlyphsHelper.Ignored;
                        sccStatus = __SccStatus.SCC_STATUS_NOTCONTROLLED;
                        break;

                    case GitFileStatus.Conflict:
                        rgsiGlyphs[i] = SccGlyphsHelper.Conflict;
                        sccStatus = __SccStatus.SCC_STATUS_CONTROLLED | __SccStatus.SCC_STATUS_CHECKEDOUT | __SccStatus.SCC_STATUS_OUTBYUSER | __SccStatus.SCC_STATUS_MERGED;
                        break;

                    case GitFileStatus.Merged:
                        rgsiGlyphs[i] = SccGlyphsHelper.Merged;
                        sccStatus = __SccStatus.SCC_STATUS_CONTROLLED | __SccStatus.SCC_STATUS_CHECKEDOUT | __SccStatus.SCC_STATUS_OUTBYUSER;
                        break;

                    default:
                        sccStatus = __SccStatus.SCC_STATUS_INVALID;
                        break;
                }

                if (rgdwSccStatus != null)
                    rgdwSccStatus[i] = (uint)sccStatus;
            }
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Determines the corresponding scc status glyph to display, given a combination of scc status flags
        /// </summary>
        public int GetSccGlyphFromStatus([InAttribute] uint dwSccStatus, [OutAttribute] VsStateIcon[] psiGlyph)
        {
            // This method is called when some user (e.g. like classview) wants to combine icons
            // (Unfortunately classview uses a hardcoded mapping)
            psiGlyph[0] = VsStateIcon.STATEICON_BLANK;
            return VSConstants.S_OK;
        }

        /// <summary>
        /// One of the most important methods in a source control provider, is called by projects that are under source control when they are first opened to register project settings
        /// </summary>
        public int RegisterSccProject([InAttribute] IVsSccProject2 pscp2Project, [InAttribute] string pszSccProjectName, [InAttribute] string pszSccAuxPath, [InAttribute] string pszSccLocalPath, [InAttribute] string pszProvider)
        {
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Called by projects registered with the source control portion of the environment before they are closed. 
        /// </summary>
        public int UnregisterSccProject([InAttribute] IVsSccProject2 pscp2Project)
        {
            return VSConstants.S_OK;
        }

        #endregion

        #region IVsSccManager3 Members

        public bool IsBSLSupported()
        {
            return true;
        }

        #endregion

        #region IVsSccManagerTooltip interface functions

        /// <summary>
        /// Called by solution explorer to provide tooltips for items. Returns a text describing the source control status of the item.
        /// </summary>
        public int GetGlyphTipText([InAttribute] IVsHierarchy phierHierarchy, [InAttribute] uint itemidNode, out string pbstrTooltipText)
        {
            pbstrTooltipText = "";
            GitFileStatus status = ThreadHelper.JoinableTaskFactory.Run(async delegate
            {
                GitFileStatus fileStatus = await GetFileStatus(phierHierarchy, itemidNode);
                return fileStatus;
            });
            pbstrTooltipText = status.ToString(); //TODO: use resources
            return VSConstants.S_OK;
        }
        #endregion

     

        #region IVsSccGlyphs Members

        public int GetCustomGlyphList(uint BaseIndex, out uint pdwImageListHandle)
        {
            pdwImageListHandle = SccGlyphsHelper.GetCustomGlyphList(BaseIndex);
            return VSConstants.S_OK;
        }

        #endregion

        #region File Names

        private async Task SetSolutionExplorerTitle(string message)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            var dte = (DTE)_sccProvider.GetService(typeof(DTE));
            dte.Windows.Item(EnvDTE.Constants.vsWindowKindSolutionExplorer).Caption = message;
        }

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

        [DllImport("kernel32.dll")]
        static extern uint GetShortPathName(string longpath, StringBuilder sb, int buffer);

        [DllImport("kernel32.dll")]
        static extern uint GetLongPathName(string shortpath, StringBuilder sb, int buffer);


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
        FileSystemWatcher _watcher;
        string lastMonitorFolder = string.Empty;
        string monitorFolder;

        internal async Task OpenTracker()
        {
            Debug.WriteLine("==== Open Tracker");
            RepositoryManager.Instance.Clear();
            _fileCache = new SccProviderSolutionCache(_sccProvider);
            

            var solutionFileName = await GetSolutionFileName();
            RepositoryManager.Instance.SetSolutionTracker(solutionFileName);
            SetSolutionExplorerTitle();

            if (!string.IsNullOrEmpty(solutionFileName))
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                var projects = await GetLoadedControllableProjects();
                foreach (var vsSccProject2 in projects)
                {
                    await AddProject(vsSccProject2 as IVsHierarchy);
                }
            }
        }

        private async void RepositoryManager_FileChanged(object sender, GitFileUpdateEventArgs e)
        {
            //TODO update files change here

            try
            {
                await ProcessSingleFileSystemChange((GitRepository)sender, e);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error in RepositoryManager_FileChanged: " + ex.Message);
            }
            //Action action = () => ProcessSingleFileSystemChange((GitRepository)sender, e);
            //Task.Factory.StartNew(action, CancellationToken.None, TaskCreationOptions.None, SccProviderService.TaskScheduler)
            //    .HandleNonCriticalExceptions();
        }

        private async void RepositoryManager_FilesChanged(object sender, GitFilesUpdateEventArgs e)
        {
            try
            {
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
                await SetSolutionExplorerTitle();
            }
            catch (Exception)
            {
            }
        }

        private async Task SetSolutionExplorerTitle()
        {
            var caption = "Solution Explorer";
            string branch = RepositoryManager.Instance?.SolutionTracker?.CurrentBranchDisplayName;
            if (!string.IsNullOrEmpty(branch))
            {
                caption += " (" + branch + ")";
                await SetSolutionExplorerTitle(caption);
            }
        }

        //private void ProcessGitFileSystemChange(GitFileUpdateEventArgs e)
        //{
        //    if (GitSccOptions.Current.DisableAutoRefresh)
        //        return;

        //    if (string.Equals(Path.GetExtension(e.Name), ".lock", StringComparison.OrdinalIgnoreCase))
        //    {
        //        if (e.FullPath.Contains(Constants.DOT_GIT + Path.DirectorySeparatorChar))
        //            return;
        //    }

        //    MarkDirty(false);
        //}

        private async Task ProcessSingleFileSystemChange(GitRepository repo, GitFileUpdateEventArgs e)
        {
            //lock (_glyphsLock)
            //{
            //await Task.Run(async delegate
            //{
            //    await Task.Run(() => 
            //await TaskScheduler.Default;
            repo.Refresh();
            if (_fileCache.StatusChanged(e.FullPath, repo.GetFileStatus(e.FullPath)))
            {
                IList<IVsSccProject2> nodes = await _fileCache.GetProjectsSelectionForFile(e.FullPath);
                if (nodes != null && nodes.Count > 0)
                {
                    foreach (var vsSccProject2 in nodes)
                    {
                        //await QuickRefreshNodesGlyphs(vsSccProject2, new List<string>() {e.FullPath});
                        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                        vsSccProject2.SccGlyphChanged(0, null, null, null);
                    }
                    //RefreshNodesGlyphs(nodes);
                }
            }
            //  }
            //});

           
           // }
        }

        private async Task ProcessMultiFileChange(GitRepository repo, GitFilesUpdateEventArgs e)
        {
            //lock (_glyphsLock)
            //{
                HashSet<IVsSccProject2> nodes = new HashSet<IVsSccProject2>();
                foreach (var file in e.Files)
                {
                    var items = await _fileCache.GetProjectsSelectionForFile(file);
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
                string branch = CurrentBranchName;
                if (!string.IsNullOrEmpty(branch))
                {
                    caption += " (" + branch + ")";
                    SetSolutionExplorerTitle(caption);
                }
           // }
        }


        //private void ProcessFileSystemChange(FileSystemEventArgs e)
        //{
        //    if (GitSccOptions.Current.DisableAutoRefresh)
        //        return;

        //    if (e.ChangeType == WatcherChangeTypes.Changed && Directory.Exists(e.FullPath))
        //        return;

        //    if (string.Equals(Path.GetExtension(e.Name), ".lock", StringComparison.OrdinalIgnoreCase))
        //    {
        //        if (e.FullPath.Contains(Constants.DOT_GIT + Path.DirectorySeparatorChar))
        //            return;
        //    }

        //    MarkDirty(false);
        //}

        private void CloseTracker()
        {
            Debug.WriteLine("==== Close Tracker");
            RepositoryManager.Instance.Clear();
            RemoveFolderMonitor();
            MarkDirty(false);
        }

        private void RemoveFolderMonitor()
        {
            if (_watcher != null)
            {
                _watcher.Dispose();
                Debug.WriteLine("==== Stop Monitoring");
                _watcher = null;
                lastMonitorFolder = "";
            }
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
                    MarkDirty(true);
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
                string filename = ThreadHelper.JoinableTaskFactory.Run(async delegate
                {
                    return await GetSelectFileName();
                });
                return RepositoryManager.Instance.GetTrackerForPath(filename);
            }
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

        private bool IsParentFolder(string folder, string fileName)
        {
            if (string.IsNullOrEmpty(folder) || string.IsNullOrEmpty(fileName) ||
               !Directory.Exists(folder)) return false;

            bool b = false;
            var dir = new DirectoryInfo(Path.GetDirectoryName(Path.GetFullPath(fileName)));
            while (!b && dir != null)
            {
                b = string.Compare(dir.FullName, folder, true) == 0;
                dir = dir.Parent;
            }
            return b;
        }

        //TODO.. think about changing this to no go to repo
        private GitFileStatus GetFileStatus(string fileName)
        {
            var tracker = GetTracker(fileName);
            return tracker == null ? GitFileStatus.NotControlled :
                tracker.GetFileStatus(fileName);
        }

        private async Task<GitFileStatus> GetFileStatus(IVsHierarchy phierHierarchy, uint itemidNode)
        {
            var fileName = await GetFileName(phierHierarchy, itemidNode);
            return GetFileStatus(fileName);
        }

        #endregion

        #region new Refresh methods

        internal DateTime nextTimeRefresh = DateTime.Now;

        private int _nodesGlyphsDirty;
        private int _explicitRefreshRequested;
        private int _disableRefresh;

        private bool NoRefresh
        {
            get
            {
                return Thread.VolatileRead(ref _disableRefresh) != 0;
            }
        }

        internal void MarkDirty(bool defer)
        {
            if (defer)
                nextTimeRefresh = DateTime.Now;

            // this doesn't need to be a volatile write since it's fine if the write is delayed
            _nodesGlyphsDirty = 1;
        }

        internal IDisposable DisableRefresh()
        {
            return new DisableRefreshHandle(this);
        }

        private sealed class DisableRefreshHandle : IDisposable
        {
            private readonly SccProviderService _service;
            private bool _disposed;

            public DisableRefreshHandle(SccProviderService service)
            {
                _service = service;
                Interlocked.Increment(ref _service._disableRefresh);
            }

            public void Dispose()
            {
                if (_disposed)
                    return;

                _disposed = true;
                Interlocked.Decrement(ref _service._disableRefresh);
            }
        }

        internal void Refresh()
        {
            // this doesn't need to be a volatile write since it's fine if the write is delayed
            _explicitRefreshRequested = 1;
        }

        //public void UpdateNodesGlyphs()
        //{
        //    if (NoRefresh)
        //        return;

        //    bool refresh = Interlocked.Exchange(ref _explicitRefreshRequested, 0) != 0;
        //    if (!refresh && Thread.VolatileRead(ref _nodesGlyphsDirty) != 0)
        //    {
        //        refresh = DateTime.Now - nextTimeRefresh >= RefreshDelay;
        //    }

        //    if (refresh)
        //    {
        //        IDisposable disableRefresh = DisableRefresh();

        //        Debug.WriteLine("==== UpdateNodesGlyphs");

        //        // this comes before the actual refresh, since a change on the file system during
        //        // the refresh may or may not appear in the refresh results
        //        Thread.VolatileWrite(ref _nodesGlyphsDirty, 0);

        //        // used for progressive backoff of the update interval for large projects
        //        Stopwatch timer = new Stopwatch();

        //        Dispatcher dispatcher = Dispatcher.CurrentDispatcher;
        //        Action openTrackerAction = () =>
        //        {
        //            timer.Start();

        //            OpenTracker();
        //            foreach (GitFileStatusTracker tracker in RepositoryManager.Instance.GetRepositories())
        //                tracker.Refresh();

        //            timer.Stop();
        //        };

        //        Action<Task> continuationAction = (task) =>
        //        {
        //            if (task.Exception != null)
        //            {
        //                disableRefresh.Dispose();
        //                return;
        //            }

        //            Action applyUpdatesAction = () =>
        //            {
        //                try
        //                {
        //                    using (disableRefresh)
        //                    {
        //                        timer.Start();

        //                        //TODO right here... Split this 
        //                        RefreshNodesGlyphs();
        //                        RefreshToolWindows();
        //                        // make sure to defer next refresh
        //                        nextTimeRefresh = DateTime.Now;
        //                        timer.Stop();

        //                        TimeSpan totalTime = timer.Elapsed;
        //                        TimeSpan minimumRefreshInterval = new TimeSpan(totalTime.Ticks * 2);
        //                        if (minimumRefreshInterval > RefreshDelay)
        //                            RefreshDelay = minimumRefreshInterval;
        //                    }
        //                }
        //                catch (Exception ex)
        //                {
        //                    if (ErrorHandler.IsCriticalException(ex))
        //                        throw;
        //                }
        //            };

        //            dispatcher.BeginInvoke(applyUpdatesAction);
        //        };

        //        Task.Factory.StartNew(openTrackerAction, CancellationToken.None, TaskCreationOptions.LongRunning, SccProviderService.TaskScheduler)
        //            .HandleNonCriticalExceptions()
        //            .ContinueWith(continuationAction, TaskContinuationOptions.ExecuteSynchronously)
        //            .HandleNonCriticalExceptions();
        //    }
        //}


        //TODO MOVE 
        //public void RefreshNodesGlyphs()
        //{
        //  //var solHier = (IVsHierarchy)_sccProvider.GetService(typeof(SVsSolution));
        //    var projectList = GetLoadedControllableProjects();

        //    // We'll also need to refresh the solution folders glyphs
        //    // to reflect the controlled state
        //    IList<VSITEMSELECTION> nodes = new List<VSITEMSELECTION>();

        //    // add project node items
        //    foreach (IVsHierarchy hr in projectList)
        //    {
        //        VSITEMSELECTION vsItem;
        //        vsItem.itemid = VSConstants.VSITEMID_ROOT;
        //        vsItem.pHier = hr;
        //        nodes.Add(vsItem);
        //    }

        //    RefreshNodesGlyphs(nodes);

        //    //var caption = "Solution Explorer";
        //    //string branch = CurrentBranchName;
        //    //if (!string.IsNullOrEmpty(branch))
        //    //{
        //    //    caption += " (" + branch + ")";
        //    //    SetSolutionExplorerTitle(caption);
        //    //}
        //}

        /// <summary>
        /// Returns a list of controllable projects in the solution
        /// </summary>
        public async Task<List<IVsSccProject2>> GetLoadedControllableProjects()
        {
            var list = new List<IVsSccProject2>();
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            IVsSolution sol = (IVsSolution) _sccProvider.GetService(typeof(SVsSolution));
            list.Add(sol as IVsSccProject2);

            Guid rguidEnumOnlyThisType = new Guid();
            IEnumHierarchies ppenum = null;
            ErrorHandler.ThrowOnFailure(sol.GetProjectEnum((uint)__VSENUMPROJFLAGS.EPF_LOADEDINSOLUTION, ref rguidEnumOnlyThisType, out ppenum));

            IVsHierarchy[] rgelt = new IVsHierarchy[1];
            uint pceltFetched = 0;
            while (ppenum.Next(1, rgelt, out pceltFetched) == VSConstants.S_OK &&
                   pceltFetched == 1)
            {
                IVsSccProject2 sccProject2 = rgelt[0] as IVsSccProject2;
                if (sccProject2 != null)
                {
                    list.Add(sccProject2);
                }
            }

            return list;
        }


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
            catch (Exception ex )
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
                        IVsHierarchy solHier = (IVsHierarchy) _sccProvider.GetService(typeof(SVsSolution));
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
        #endregion

        #region git
        public bool IsSolutionGitControlled
        {
            get { return RepositoryManager.Instance.GetRepositories().Count > 0; }
        }

        internal async Task InitRepo()
        {
            var solutionPath = Path.GetDirectoryName(await GetSolutionFileName());
            GitFileStatusTracker.Init(solutionPath);
            File.WriteAllText(Path.Combine(solutionPath, ".gitignore"),
@"Thumbs.db
*.obj
*.exe
*.pdb
*.user
*.aps
*.pch
*.vspscc
*_i.c
*_p.c
*.ncb
*.suo
*.sln.docstates
*.tlb
*.tlh
*.bak
*.cache
*.ilk
*.log
[Bb]in
[Dd]ebug*/
*.lib
*.sbr
obj/
[Rr]elease*/
_ReSharper*/
[Tt]est[Rr]esult*
*.vssscc
$tf*/"
            );
            File.WriteAllText(Path.Combine(solutionPath, ".tfignore"), @"\.git");
        } 
        #endregion

        private void RefreshToolWindows()
        {
            var window = this._sccProvider.FindToolWindow(typeof(PendingChangesToolWindow), 0, false) 
                as PendingChangesToolWindow;
            if (window != null) window.Refresh(this.CurrentTracker);
        }
    }
}