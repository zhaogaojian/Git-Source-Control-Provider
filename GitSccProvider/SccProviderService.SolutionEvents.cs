using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GitSccProvider;
using GitSccProvider.Utilities;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace GitScc
{
    public partial class SccProviderService 
    {
        private uint _vsSolutionEventsCookie;
        private uint _vsIVsUpdateSolutionEventsCookie;


        private void SetupSolutionEvents()
        {
            // Subscribe to solution events
            ThreadHelper.ThrowIfNotOnUIThread();
            IVsSolution sol = (IVsSolution)_sccProvider.GetService(typeof(SVsSolution));
            sol.AdviseSolutionEvents(this, out _vsSolutionEventsCookie);

            var sbm = _sccProvider.GetService(typeof(SVsSolutionBuildManager)) as IVsSolutionBuildManager2;
            if (sbm != null)
            {
                sbm.AdviseUpdateSolutionEvents(this, out _vsIVsUpdateSolutionEventsCookie);
            }
        }

        private void UnRegisterSolutionEvents()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            // Unregister from receiving solution events
            if (VSConstants.VSCOOKIE_NIL != _vsSolutionEventsCookie)
            {
                IVsSolution sol = (IVsSolution)_sccProvider.GetService(typeof(SVsSolution));
                sol.UnadviseSolutionEvents(_vsSolutionEventsCookie);
                _vsSolutionEventsCookie = VSConstants.VSCOOKIE_NIL;
            }

            if (VSConstants.VSCOOKIE_NIL != _vsIVsUpdateSolutionEventsCookie)
            {
                var sbm = _sccProvider.GetService(typeof(SVsSolutionBuildManager)) as IVsSolutionBuildManager2;
                sbm.UnadviseUpdateSolutionEvents(_vsIVsUpdateSolutionEventsCookie);
            }
        }

        #region IVsSolutionEvents interface functions

        public int OnAfterOpenSolution([In] Object pUnkReserved, [In] int fNewSolution)
        {

            ThreadHelper.JoinableTaskFactory.Run(async delegate
            {
                await OpenTracker();
            });
           
            //RefreshDelay = InitialRefreshDelay;

            //automatic switch the scc provider
            if (!Active && !GitSccOptions.Current.DisableAutoLoad)
            {
                ThreadHelper.JoinableTaskFactory.Run(async delegate
                {
                    await OpenTracker();
                });

                if (RepositoryManager.Instance.GetRepositories().Count > 0)
                {
                    ThreadHelper.ThrowIfNotOnUIThread();
                    IVsRegisterScciProvider rscp =
                        (IVsRegisterScciProvider)_sccProvider.GetService(typeof(IVsRegisterScciProvider));
                    rscp.RegisterSourceControlProvider(GuidList.guidSccProvider);
                }
            }            
            return VSConstants.S_OK;
        }


        public int OnAfterCloseSolution([In] Object pUnkReserved)
        {
            CloseTracker();
            _fileCache = new SccProviderSolutionCache(_sccProvider);
            return VSConstants.S_OK;
        }

        public int OnAfterLoadProject([In] IVsHierarchy pStubHierarchy, [In] IVsHierarchy pRealHierarchy)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterOpenProject([In] IVsHierarchy pHierarchy, [In] int fAdded)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            bool isSolutionFolder = ThreadHelper.JoinableTaskFactory.Run(async delegate
            {
                return await pHierarchy.IsSolutionFolderProject();
            });
            //_fileCache.AddProject(pHierarchy);
            // If a solution folder is added to the solution after the solution is added to scc, we need to controll that folder
            if (isSolutionFolder && (fAdded == 1))
            {
                IVsHierarchy solHier = (IVsHierarchy)_sccProvider.GetService(typeof(SVsSolution));
                //if (IsProjectControlled(solHier))
                //{
                    // Register this solution folder using the same location as the solution
                IVsSccProject2 pSccProject = (IVsSccProject2)pHierarchy;
                RegisterProjectWithGit(pSccProject);
                    //RegisterSccProject(pSccProject, _solutionLocation, "", "", _sccProvider.ProviderName);

                // We'll also need to refresh the solution folders glyphs to reflect the controlled state
                IList<VSITEMSELECTION> nodes = new List<VSITEMSELECTION>();

                    VSITEMSELECTION vsItem;
                    vsItem.itemid = VSConstants.VSITEMID_ROOT;
                    vsItem.pHier = pHierarchy;
                    nodes.Add(vsItem);
                    RefreshNodesGlyphs(nodes);
                //}
            }

            return VSConstants.S_OK;
        }

        private void RegisterProjectWithGit(IVsSccProject2 pscp2Project)
        {
            if (pscp2Project == null)
            {
                // Manual registration with source control of the solution, from OnAfterOpenSolution
                Debug.WriteLine(string.Format(CultureInfo.CurrentUICulture, "Solution {0} is registering with source control - {1}", GetSolutionFileName()));

                //IVsHierarchy solHier = (IVsHierarchy)_sccProvider.GetService(typeof(SVsSolution));
                //string solutionFile = GetSolutionFileName();
            
            }
            else
            {
                // Debug.WriteLine(string.Format(CultureInfo.CurrentUICulture, "Project {0} is registering with source control - {1}", GetProjectFileName(pscp2Project)));

                // Add the project to the list of controlled projects
                ThreadHelper.JoinableTaskFactory.Run(async delegate
                {
                    await _fileCache.AddProject(pscp2Project);
                });

            }
        }



        public int OnBeforeCloseProject([In] IVsHierarchy pHierarchy, [In] int fRemoved)
        {
            _fileCache.InValidateCache();
            return VSConstants.S_OK;
        }

        public int OnBeforeCloseSolution([In] Object pUnkReserved)
        {
            return VSConstants.S_OK;
        }

        public int OnBeforeUnloadProject([In] IVsHierarchy pRealHierarchy, [In] IVsHierarchy pStubHierarchy)
        {
            return VSConstants.S_OK;
        }

        public int OnQueryCloseProject([In] IVsHierarchy pHierarchy, [In] int fRemoving, [In] ref int pfCancel)
        {
            return VSConstants.S_OK;
        }

        public int OnQueryCloseSolution([In] Object pUnkReserved, [In] ref int pfCancel)
        {
            _fileCache.InValidateCache();
            return VSConstants.S_OK;
        }

        public int OnQueryUnloadProject([In] IVsHierarchy pRealHierarchy, [In] ref int pfCancel)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterMergeSolution([In] Object pUnkReserved)
        {
            return VSConstants.S_OK;
        }

        #endregion

        #region IVsUpdateSolutionEvents2 Members

        public int OnActiveProjectCfgChange(IVsHierarchy pIVsHierarchy)
        {
            return VSConstants.S_OK;
        }

        public int UpdateProjectCfg_Begin(IVsHierarchy pHierProj, IVsCfg pCfgProj, IVsCfg pCfgSln, uint dwAction,
            ref int pfCancel)
        {
            return VSConstants.S_OK;
        }

        public int UpdateProjectCfg_Done(IVsHierarchy pHierProj, IVsCfg pCfgProj, IVsCfg pCfgSln, uint dwAction,
            int fSuccess, int fCancel)
        {
            return VSConstants.S_OK;
        }

        private IDisposable _updateSolutionDisableRefresh;

        public int UpdateSolution_Begin(ref int pfCancelUpdate)
        {
            Debug.WriteLine("Git Source Control Provider: suppress refresh before build...");
            //_fileCache = new SccProviderSolutionCache(_sccProvider);
            IDisposable disableRefresh = DisableRefresh();
            disableRefresh = Interlocked.Exchange(ref _updateSolutionDisableRefresh, disableRefresh);
            if (disableRefresh != null)
            {
                // this is unexpected, but if we did overwrite a handle make sure it gets disposed
                disableRefresh.Dispose();
            }

            return VSConstants.S_OK;
        }

        public int UpdateSolution_Cancel()
        {
            Debug.WriteLine("Git Source Control Provider: resume refresh after cancel...");
            IDisposable handle = Interlocked.Exchange(ref _updateSolutionDisableRefresh, null);
            if (handle != null)
                handle.Dispose();

            return VSConstants.S_OK;
        }

        public int UpdateSolution_Done(int fSucceeded, int fModified, int fCancelCommand)
        {
            Debug.WriteLine("Git Source Control Provider: resume refresh after build...");
            IDisposable handle = Interlocked.Exchange(ref _updateSolutionDisableRefresh, null);
            if (handle != null)
                handle.Dispose();

            return VSConstants.S_OK;
        }

        public int UpdateSolution_StartUpdate(ref int pfCancelUpdate)
        {
            return VSConstants.S_OK;
        }

        #endregion
    }
}
