using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnvDTE;
using EnvDTE80;
using GitSccProvider.Utilities;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Threading;
using ThreadHelper = Microsoft.VisualStudio.Shell.ThreadHelper;

namespace GitScc
{
    public partial class SccProviderService
    {
        // The cookie for project document events
        private uint _tpdTrackProjectDocumentsCookie;
       // private DTE2 _activeIde;
        private WindowEvents _windowEvents;
        private SolutionEvents _solutionEvents;

        private void SetupDocumentEvents()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            IVsTrackProjectDocuments2 tpdService = (IVsTrackProjectDocuments2)_sccProvider.GetService(typeof(SVsTrackProjectDocuments));
            tpdService.AdviseTrackProjectDocumentsEvents(this, out _tpdTrackProjectDocumentsCookie);
            Debug.Assert(VSConstants.VSCOOKIE_NIL != _tpdTrackProjectDocumentsCookie);

            var activeIde = SolutionExtensions.GetActiveIDE();
            //var activeIde = BasicSccProvider.GetServiceEx<EnvDTE80.DTE2>();
            //activeIde.Events.SolutionItemsEvents.
            _windowEvents = activeIde.Events.WindowEvents;
            _solutionEvents = activeIde.Events.SolutionEvents;
            _solutionEvents.Opened += _solutionEvents_Opened;
            _windowEvents.WindowActivated += _windowEvents_WindowActivated;

        }

        private void UnRegisterDocumentEvents()
        {
            if (VSConstants.VSCOOKIE_NIL != _tpdTrackProjectDocumentsCookie)
            {
                ThreadHelper.ThrowIfNotOnUIThread();
                IVsTrackProjectDocuments2 tpdService = (IVsTrackProjectDocuments2)_sccProvider.GetService(typeof(SVsTrackProjectDocuments));
                tpdService.UnadviseTrackProjectDocumentsEvents(_tpdTrackProjectDocumentsCookie);
                _tpdTrackProjectDocumentsCookie = VSConstants.VSCOOKIE_NIL;
            }
            _windowEvents.WindowActivated -= _windowEvents_WindowActivated;
            _solutionEvents.Opened -= _solutionEvents_Opened;
        }

        private async void _solutionEvents_Opened()
        {
            await EnableSccForSolution();
        }

        private async void _windowEvents_WindowActivated(Window GotFocus, Window LostFocus)
        {

            if (GitSccOptions.Current.TrackActiveGitRepo)
            {
                var filename = GotFocus?.Document?.FullName;
                if (!string.IsNullOrWhiteSpace(filename))
                {
                    //await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                    //await TaskScheduler.Default;
                    RepositoryManager.Instance.GetTrackerForPath(filename, true);
                }
            }
        }

        #region Implementation of IVsTrackProjectDocumentsEvents2


        public int OnQueryAddFiles(IVsProject pProject, int cFiles, string[] rgpszMkDocuments, VSQUERYADDFILEFLAGS[] rgFlags,
            VSQUERYADDFILERESULTS[] pSummaryResult, VSQUERYADDFILERESULTS[] rgResults)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            pSummaryResult[0] = VSQUERYADDFILERESULTS.VSQUERYADDFILERESULTS_AddOK;
            if (rgResults != null)
            {
                for (int iFile = 0; iFile < cFiles; iFile++)
                {
                    rgResults[iFile] = VSQUERYADDFILERESULTS.VSQUERYADDFILERESULTS_AddOK;
                }
            }

            try
            {
                var sccProject = pProject as IVsSccProject2;
                var pHier = pProject as IVsHierarchy;
                string projectName = null;
                if (sccProject == null)
                {
                    // This is the solution calling
                    pHier = (IVsHierarchy) _sccProvider.GetService(typeof (SVsSolution));
                }

                if (sccProject != null)
                {
                   for (int iFile = 0; iFile < cFiles; iFile++)
					{
                        _fileCache.AddFile(rgpszMkDocuments[iFile], sccProject);
                    } 
                }
             
            }
            catch (Exception)
            {
            }


            return VSConstants.E_NOTIMPL;
        }

        public int OnAfterAddFilesEx(int cProjects, int cFiles, IVsProject[] rgpProjects, int[] rgFirstIndices,
            string[] rgpszMkDocuments, VSADDFILEFLAGS[] rgFlags)
        {
            return VSConstants.E_NOTIMPL;
        }

        public int OnAfterAddDirectoriesEx(int cProjects, int cDirectories, IVsProject[] rgpProjects, int[] rgFirstIndices,
            string[] rgpszMkDocuments, VSADDDIRECTORYFLAGS[] rgFlags)
        {
            return VSConstants.E_NOTIMPL;
        }

        public int OnAfterRemoveFiles(int cProjects, int cFiles, IVsProject[] rgpProjects, int[] rgFirstIndices,
            string[] rgpszMkDocuments, VSREMOVEFILEFLAGS[] rgFlags)
        {
            return VSConstants.E_NOTIMPL;
        }

        public int OnAfterRemoveDirectories(int cProjects, int cDirectories, IVsProject[] rgpProjects, int[] rgFirstIndices,
            string[] rgpszMkDocuments, VSREMOVEDIRECTORYFLAGS[] rgFlags)
        {
            return VSConstants.E_NOTIMPL;
        }

        public int OnQueryRenameFiles(IVsProject pProject, int cFiles, string[] rgszMkOldNames, string[] rgszMkNewNames,
            VSQUERYRENAMEFILEFLAGS[] rgFlags, VSQUERYRENAMEFILERESULTS[] pSummaryResult, VSQUERYRENAMEFILERESULTS[] rgResults)
        {
            return VSConstants.E_NOTIMPL;
        }

        public int OnAfterRenameFiles(int cProjects, int cFiles, IVsProject[] rgpProjects, int[] rgFirstIndices,
            string[] rgszMkOldNames, string[] rgszMkNewNames, VSRENAMEFILEFLAGS[] rgFlags)
        {
            return VSConstants.E_NOTIMPL;
        }

        public int OnQueryRenameDirectories(IVsProject pProject, int cDirs, string[] rgszMkOldNames, string[] rgszMkNewNames,
            VSQUERYRENAMEDIRECTORYFLAGS[] rgFlags, VSQUERYRENAMEDIRECTORYRESULTS[] pSummaryResult,
            VSQUERYRENAMEDIRECTORYRESULTS[] rgResults)
        {
            return VSConstants.E_NOTIMPL;
        }

        public int OnAfterRenameDirectories(int cProjects, int cDirs, IVsProject[] rgpProjects, int[] rgFirstIndices,
            string[] rgszMkOldNames, string[] rgszMkNewNames, VSRENAMEDIRECTORYFLAGS[] rgFlags)
        {
            return VSConstants.E_NOTIMPL;
        }

        public int OnQueryAddDirectories(IVsProject pProject, int cDirectories, string[] rgpszMkDocuments,
            VSQUERYADDDIRECTORYFLAGS[] rgFlags, VSQUERYADDDIRECTORYRESULTS[] pSummaryResult,
            VSQUERYADDDIRECTORYRESULTS[] rgResults)
        {
            return VSConstants.E_NOTIMPL;
        }

        public int OnQueryRemoveFiles(IVsProject pProject, int cFiles, string[] rgpszMkDocuments, VSQUERYREMOVEFILEFLAGS[] rgFlags,
            VSQUERYREMOVEFILERESULTS[] pSummaryResult, VSQUERYREMOVEFILERESULTS[] rgResults)
        {
            return VSConstants.E_NOTIMPL;
        }

        public int OnQueryRemoveDirectories(IVsProject pProject, int cDirectories, string[] rgpszMkDocuments,
            VSQUERYREMOVEDIRECTORYFLAGS[] rgFlags, VSQUERYREMOVEDIRECTORYRESULTS[] pSummaryResult,
            VSQUERYREMOVEDIRECTORYRESULTS[] rgResults)
        {
            return VSConstants.E_NOTIMPL;
        }

        public int OnAfterSccStatusChanged(int cProjects, int cFiles, IVsProject[] rgpProjects, int[] rgFirstIndices,
            string[] rgpszMkDocuments, uint[] rgdwSccStatus)
        {
            return VSConstants.E_NOTIMPL;
        }

        #endregion
    }
}
