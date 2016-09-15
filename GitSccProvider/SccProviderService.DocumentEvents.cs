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
    public partial class SccProviderService : IVsSolutionLoadEvents
    {
        // The cookie for project document events
        private uint _tpdTrackProjectDocumentsCookie;
       // private DTE2 _activeIde;
        private WindowEvents _windowEvents;
        private SolutionEvents _solutionEvents;
        private bool _solutionLoaded = false;

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
            _solutionEvents.ProjectAdded += _solutionEvents_ProjectAdded;
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
            _solutionEvents.ProjectAdded -= _solutionEvents_ProjectAdded;
            _windowEvents.WindowActivated -= _windowEvents_WindowActivated;
        }

        private async void _solutionEvents_ProjectAdded(Project dteProject)
        {
            try
            {
               await AutoAddProject(dteProject);
            }
            catch (Exception)
            {
                //TODO 
            }
        }

        private async Task AutoAddProject(Project dteProject)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            var project = await SolutionExtensions.GetIVsHierarchy(dteProject) as IVsSccProject2;
            if (_solutionLoaded) 
            {
                if (GitSccOptions.Current.AutoAddProjects && project != null && !SolutionExtensions.IsProjectInGit(dteProject.FullName))
                {
                    await AddProjectToSourceControl(project);
                }
                //ok To be safe rebuild cache 
                await RefreshSolution();
                //if (!_fileCache.ProjectAddedToCache(project))
                //{
                //    await _fileCache.AddProject(project);
                //}
            }
           
        }



        private async void _solutionEvents_Opened()
        {
            await _statusBarManager.SetActiveRepository(RepositoryManager.Instance.SolutionTracker);
            await RefreshSolution();
        }

        private async void _windowEvents_WindowActivated(Window GotFocus, Window LostFocus)
        {

            try
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
            catch (Exception ex)
            {
                Debug.WriteLine("Error In Window Activated Event: " + ex.Message);
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

            if (Active)
            {
                // Start by iterating through all projects calling this function
                for (int iProject = 0; iProject < cProjects; iProject++)
                {
                    IVsSccProject2 sccProject = rgpProjects[iProject] as IVsSccProject2;
                    IVsHierarchy pHier = rgpProjects[iProject] as IVsHierarchy;

                    // If the project is not controllable, or is not controlled, skip it
                    if (sccProject == null)
                    {
                        continue;
                    }

                    // Files in this project are in rgszMkOldNames, rgszMkNewNames arrays starting with iProjectFilesStart index and ending at iNextProjecFilesStart-1
                    int iProjectFilesStart = rgFirstIndices[iProject];
                    int iNextProjecFilesStart = cFiles;
                    if (iProject < cProjects - 1)
                    {
                        iNextProjecFilesStart = rgFirstIndices[iProject + 1];
                    }

                    // Now that we know which files belong to this project, iterate the project files
                    for (int iFile = iProjectFilesStart; iFile < iNextProjecFilesStart; iFile++)
                    {
                        var fileName = rgpszMkDocuments[iFile];
                        _fileCache.AddFile(fileName,sccProject);
                        if (GitSccOptions.Current.AutoAddFiles)
                        {
                            var repo = RepositoryManager.Instance.GetTrackerForPath(fileName);
                            repo.AddFile(fileName);
                        }
                        // Refresh the solution explorer glyphs for all projects containing this file
                        //IList<VSITEMSELECTION> nodes = GetControlledProjectsContainingFile(rgpszMkDocuments[iFile]);
                        //_sccProvider.RefreshNodesGlyphs(nodes);
                    }
                }
            }

            return VSConstants.E_NOTIMPL; ;
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

        public int OnBeforeOpenSolution(string pszSolutionFilename)
        {
            _solutionLoaded = false;
            return VSConstants.S_OK;
        }

        public int OnBeforeBackgroundSolutionLoadBegins()
        {
            return VSConstants.S_OK;
        }

        public int OnQueryBackgroundLoadProjectBatch(out bool pfShouldDelayLoadToNextIdle)
        {
            pfShouldDelayLoadToNextIdle = false;
            return VSConstants.S_OK;
        }

        public int OnBeforeLoadProjectBatch(bool fIsBackgroundIdleBatch)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterLoadProjectBatch(bool fIsBackgroundIdleBatch)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterBackgroundSolutionLoadComplete()
        {
            _solutionLoaded = true;

            ThreadHelper.JoinableTaskFactory.Run(async delegate
            {
                await RefreshSolution();
            });

            return VSConstants.S_OK;
        }
    }
}
