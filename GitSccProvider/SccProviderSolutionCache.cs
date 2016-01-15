using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using GitScc;
using GitSccProvider.Utilities;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;


namespace GitSccProvider
{
    public class ProjectFileCacheItem
    {
        public VSITEMSELECTION SelectItem { get; set; }
        public IVsSccProject2 Project { get; set; }
    }

    public class SccProviderSolutionCache : IDisposable
    {
        private BasicSccProvider _sccProvider;
        private ConcurrentDictionary<string, GitFileStatus> _fileStatus;
        private List<IVsSccProject2> _projects;
        private ConcurrentDictionary<string, List<IVsSccProject2>> _fileProjectLookup;
        //private ConcurrentDictionary<IVsSccProject2, VSITEMSELECTION> _projectSelectionLookup;
        private DateTime _lastNewFileScan;
        private static int _projectScanDelaySeconds = 5;

        public SccProviderSolutionCache(BasicSccProvider provider)
        {
            _sccProvider = provider;
            _projects = new List<IVsSccProject2>();
            _fileProjectLookup = new ConcurrentDictionary<string, List<IVsSccProject2>>();
            _fileStatus = new ConcurrentDictionary<string, GitFileStatus>();
    //_projectSelectionLookup = new ConcurrentDictionary<IVsSccProject2, VSITEMSELECTION>();
            _lastNewFileScan = DateTime.MinValue;
        }

        public void InValidateCache()
        {
            _projects = new List<IVsSccProject2>();
            _fileProjectLookup = new ConcurrentDictionary<string, List<IVsSccProject2>>();
           // _projectSelectionLookup = new ConcurrentDictionary<IVsSccProject2, VSITEMSELECTION>();
            _lastNewFileScan = DateTime.MinValue;
        }

        private void AddFileToList(string filename, IVsSccProject2 project)
        {
            List<IVsSccProject2> projects;

            if (!_fileProjectLookup.TryGetValue(filename, out projects))
            {
                //VSITEMSELECTION vsItem;
                //if (!_projectSelectionLookup.TryGetValue(project, out vsItem))
                //{
                //    vsItem = CreateItem(filename, project);
                //    _projectSelectionLookup.TryAdd(project, vsItem);
                //}
                _fileProjectLookup.TryAdd(filename, new List<IVsSccProject2> { project });
            }

            else
            {
                if (!projects.Contains(project))
                {
                    projects.Add(project);
                }
                //VSITEMSELECTION vsItem;
                //if (!_projectSelectionLookup.TryGetValue(project, out vsItem))
                //{
                //    vsItem = CreateItem(filename, project);
                //    _projectSelectionLookup.TryAdd(project, vsItem);
                //}
                //if (!projects.Contains(vsItem))
                //{
                //    projects.Add(vsItem);
                //}
            }
        }

        //private VSITEMSELECTION CreateItem(string filename,IVsSccProject2 project)
        //{
        //    VSITEMSELECTION vsItem = new VSITEMSELECTION();


        //    IVsHierarchy solHier = (IVsHierarchy)_sccProvider.GetService(typeof(SVsSolution));
        //    IVsHierarchy pHier = project as IVsHierarchy;

        //    if (solHier == pHier)
        //    {
        //        // This is the solution
        //        if (filename.ToLower().CompareTo(GetSolutionFileName().ToLower()) == 0)
        //        {
        //            vsItem.itemid = VSConstants.VSITEMID_ROOT;
        //            vsItem.pHier = null;
        //        }
        //    }
        //    else
        //    {
        //        IVsProject2 pProject = pHier as IVsProject2;
        //        // See if the file is member of this project
        //        // Caveat: the IsDocumentInProject function is expensive for certain project types, 
        //        // you may want to limit its usage by creating your own maps of file2project or folder2project
        //        int fFound;
        //        uint itemid;
        //        VSDOCUMENTPRIORITY[] prio = new VSDOCUMENTPRIORITY[1];
        //        if (pProject != null && pProject.IsDocumentInProject(filename, out fFound, prio, out itemid) == VSConstants.S_OK && fFound != 0)
        //        {
        //            vsItem.itemid = itemid;
        //            vsItem.pHier = pHier;
        //        }
        //    }
        //    return  vsItem;
        //}

        public bool StatusChanged(string filename, GitFileStatus status)
        {

            var file = filename.ToLower();
            var fileStatus = GitFileStatus.NotControlled;


            if (_fileStatus.TryGetValue(file, out fileStatus))
            {
                if (fileStatus == status)
                {
                    return false;
                }
                _fileStatus[file] = status;
                return true;
            }
            _fileStatus.TryAdd(file, status);
            return true;
        }

        public async Task<List<IVsSccProject2>> GetProjectsSelectionForFile(string filename)
        {
           return await  GetProjectsSelectionForFile(filename,true);
        }

        private async Task<List<IVsSccProject2>> GetProjectsSelectionForFile(string filename, bool search)
        {
            List<IVsSccProject2> projects;
            var filePath = filename.ToLower();
            if (!_fileProjectLookup.TryGetValue(filePath, out projects))
            {
                if (!search)
                {
                    _fileProjectLookup.TryAdd(filePath, new List<IVsSccProject2>());
                    return null;
                }
                else
                {
                    if (DateTime.UtcNow > _lastNewFileScan.AddSeconds(_projectScanDelaySeconds))
                    {
                        await ScanSolution();
                        _lastNewFileScan = DateTime.UtcNow;
                    }
                    projects = await GetProjectsSelectionForFile(filePath, false);
                }
            }
            return projects;
        }


        public async Task AddProject(IVsSccProject2 project)
        {
            if (!_projects.Contains(project) && project != null)
            {
                _projects.Add(project);
            }

            var files = await SolutionExtensions.GetProjectFiles(project);

            foreach (var file in files)
            {
                AddFileToList(file.ToLower(), project);
            }
        }



        private async Task ScanSolution()
        {
            var projects = await GetLoadedControllableProjects();
            //TODO MAke sure I want to do this
            _fileProjectLookup = new ConcurrentDictionary<string, List<IVsSccProject2>>();
            foreach (var project in projects)
            {
                await AddProject(project);
            }

        }

        //TODo Temp
        private async Task<List<IVsSccProject2>> GetLoadedControllableProjects()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
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

        //public string GetSolutionFileName()
        //{
        //    IVsSolution sol = (IVsSolution)_sccProvider.GetService(typeof(SVsSolution));
        //    string solutionDirectory, solutionFile, solutionUserOptions;
        //    if (sol.GetSolutionInfo(out solutionDirectory, out solutionFile, out solutionUserOptions) == VSConstants.S_OK)
        //    {
        //        return solutionFile;
        //    }
        //    else
        //    {
        //        return null;
        //    }
        //}

        //public bool IsProjectControlled(IVsHierarchy pHier)
        //{
        //    if (pHier == null)
        //    {
        //        // this is solution, get the solution hierarchy
        //        pHier = (IVsHierarchy)_sccProvider.GetService(typeof(SVsSolution));
        //    }

        //    return project_to_storage_map.ContainsKey(pHier);
        //}

        //public bool IsProjectControlled(IVsSccProject2 project)
        //{
        //    if (pHier == null)
        //    {
        //        // this is solution, get the solution hierarchy
        //        pHier = (IVsHierarchy)_sccProvider.GetService(typeof(SVsSolution));
        //    }

        //    return project_to_storage_map.ContainsKey(pHier);
        //}

        #region Implementation of IDisposable

        public void Dispose()
        {
            _projects = null;
            _fileProjectLookup = null;
           // _projectSelectionLookup = null;
        }

        #endregion
    }
}
