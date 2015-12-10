using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GitSccProvider;
using GitSccProvider.Utilities;
using Microsoft.VisualStudio;
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
            RefreshDelay = InitialRefreshDelay;

            //automatic switch the scc provider
            if (!Active && !GitSccOptions.Current.DisableAutoLoad)
            {
                OpenTracker();

                if (RepositoryManager.Instance.GetRepositories().Count > 0)
                {
                    IVsRegisterScciProvider rscp =
                        (IVsRegisterScciProvider)_sccProvider.GetService(typeof(IVsRegisterScciProvider));
                    rscp.RegisterSourceControlProvider(GuidList.guidSccProvider);
                }
            }

            MarkDirty(false);
            return VSConstants.S_OK;
        }


        public int OnAfterCloseSolution([In] Object pUnkReserved)
        {
            CloseTracker();
            _fileCahce = new SccProviderSolutionCache(_sccProvider);
            return VSConstants.S_OK;
        }

        public int OnAfterLoadProject([In] IVsHierarchy pStubHierarchy, [In] IVsHierarchy pRealHierarchy)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterOpenProject([In] IVsHierarchy pHierarchy, [In] int fAdded)
        {
            _fileCahce.AddProject((IVsSccProject2)pHierarchy);
            // If a solution folder is added to the solution after the solution is added to scc, we need to controll that folder
            if (pHierarchy.IsSolutionFolderProject() && (fAdded == 1))
            {
                IVsHierarchy solHier = (IVsHierarchy)_sccProvider.GetService(typeof(SVsSolution));
                //if (IsProjectControlled(solHier))
                //{
                    // Register this solution folder using the same location as the solution
                    IVsSccProject2 pSccProject = (IVsSccProject2)pHierarchy;
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

        public int OnBeforeCloseProject([In] IVsHierarchy pHierarchy, [In] int fRemoved)
        {
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
