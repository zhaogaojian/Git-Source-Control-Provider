using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GitScc;
using GitSccProvider.Utilities;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell.Interop;
using Sharpen;

namespace GitSccProvider
{
    public class SccProviderSolutionCache : IDisposable
    {
        private BasicSccProvider _sccProvider;
        private List<IVsSccProject2> _projects;
        private ConcurrentDictionary<string, List<VSITEMSELECTION>> _fileProjectLookup;
        private ConcurrentDictionary<IVsSccProject2, VSITEMSELECTION> _projectSelectionLookup;
        private DateTime _lastNewFileScan;
        private static int _projectScanDelaySeconds = 5;

        public SccProviderSolutionCache(BasicSccProvider provider)
        {
            _sccProvider = provider;
            _projects = new List<IVsSccProject2>();
            _fileProjectLookup = new ConcurrentDictionary<string, List<VSITEMSELECTION>>();
            _projectSelectionLookup = new ConcurrentDictionary<IVsSccProject2, VSITEMSELECTION>();
            _lastNewFileScan = DateTime.MinValue;
        }

        private void AddFileToList(string filename, IVsSccProject2 project)
        {
            List<VSITEMSELECTION> projects;

            if (!_fileProjectLookup.TryGetValue(filename, out projects))
            {
                VSITEMSELECTION vsItem;
                if (!_projectSelectionLookup.TryGetValue(project, out vsItem))
                {
                    vsItem = CreateItem(filename, project);
                }
                _fileProjectLookup.TryAdd(filename, new List<VSITEMSELECTION> { vsItem });
            }

            else
            {
                VSITEMSELECTION vsItem;
                if (!_projectSelectionLookup.TryGetValue(project, out vsItem))
                {
                    projects.Add(CreateItem(filename, project));
                }
            }
        }

        private VSITEMSELECTION CreateItem(string filename,IVsSccProject2 project)
        {
            VSITEMSELECTION vsItem = new VSITEMSELECTION();


            IVsHierarchy solHier = (IVsHierarchy)_sccProvider.GetService(typeof(SVsSolution));
            IVsHierarchy pHier = project as IVsHierarchy;

            if (solHier == pHier)
            {
                // This is the solution
                if (filename.ToLower().CompareTo(GetSolutionFileName().ToLower()) == 0)
                {
                    vsItem.itemid = VSConstants.VSITEMID_ROOT;
                    vsItem.pHier = null;
                }
            }
            else
            {
                IVsProject2 pProject = pHier as IVsProject2;
                // See if the file is member of this project
                // Caveat: the IsDocumentInProject function is expensive for certain project types, 
                // you may want to limit its usage by creating your own maps of file2project or folder2project
                int fFound;
                uint itemid;
                VSDOCUMENTPRIORITY[] prio = new VSDOCUMENTPRIORITY[1];
                if (pProject != null && pProject.IsDocumentInProject(filename, out fFound, prio, out itemid) == VSConstants.S_OK && fFound != 0)
                {
                    vsItem.itemid = itemid;
                    vsItem.pHier = pHier;
                }
            }
            return  vsItem;
        }

        public List<VSITEMSELECTION> GetProjectsSelectionForFile(string filename)
        {
            return GetProjectsSelectionForFile(filename,true);
        }

        private List<VSITEMSELECTION> GetProjectsSelectionForFile(string filename, bool search)
        {
            List<VSITEMSELECTION> projects;
            var filePath = filename.ToLower();
            if (!_fileProjectLookup.TryGetValue(filePath, out projects))
            {
                if (!search)
                {
                    _fileProjectLookup.TryAdd(filePath, null);
                    return null;
                }
                else
                {
                    if (DateTime.UtcNow > _lastNewFileScan.AddSeconds(_projectScanDelaySeconds))
                    {
                        ScanSolution();
                        _lastNewFileScan = DateTime.UtcNow;
                    }
                    projects = GetProjectsSelectionForFile(filePath, false);
                }
            }
            return projects;
        }


        public void AddProject(IVsSccProject2 project)
        {
            if (!_projects.Contains(project) && project != null)
            {
                _projects.Add(project);
            }

            var files = SolutionExtensions.GetProjectFiles(project);

            foreach (var file in files)
            {
                AddFileToList(file.ToLower(), project);
            }
        }



        private void ScanSolution()
        {
            var projects = GetLoadedControllableProjects();
            foreach (var project in projects)
            {
                AddProject(project);
            }

        }

        //TODo Temp
        private List<IVsSccProject2> GetLoadedControllableProjects()
        {
            var list = new List<IVsSccProject2>();

            IVsSolution sol = (IVsSolution)_sccProvider.GetService(typeof(SVsSolution));
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

        public string GetSolutionFileName()
        {
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

        #region Implementation of IDisposable

        public void Dispose()
        {
            _projects = null;
            _fileProjectLookup = null;
            _projectSelectionLookup = null;
        }

        #endregion
    }
}
