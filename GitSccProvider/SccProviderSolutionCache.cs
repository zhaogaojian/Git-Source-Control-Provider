using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
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
        private DateTime _lastNewFileScan;
        private static int _projectScanDelaySeconds = 5;
        private object _registerLock = new object();

        public SccProviderSolutionCache(BasicSccProvider provider)
        {
            _sccProvider = provider;
            _projects = new List<IVsSccProject2>();
            _fileProjectLookup = new ConcurrentDictionary<string, List<IVsSccProject2>>();
            _fileStatus = new ConcurrentDictionary<string, GitFileStatus>();
            _lastNewFileScan = DateTime.MinValue;
        }

        private void AddFileToList(string filename, IVsSccProject2 project)
        {
            List<IVsSccProject2> projects;

            if (!_fileProjectLookup.TryGetValue(filename, out projects))
            {
                _fileProjectLookup.TryAdd(filename, new List<IVsSccProject2> { project });
            }

            else
            {
                if (!projects.Contains(project))
                {
                    projects.Add(project);
                }
            }
        }
        #region Public Methods

        public bool FileTracked(string filename)
        {
            if (string.IsNullOrWhiteSpace(filename))
            {
                return false;
            }
            return _fileProjectLookup.ContainsKey(filename);
        }

        public void AddFile(string filename, IVsSccProject2 project)
        {
            if (_projects.Contains(project))
            {
                AddFileToList(filename.ToLower(), project);
            }
        }
        
        public void InValidateCache()
        {
            _projects = new List<IVsSccProject2>();
            _fileProjectLookup = new ConcurrentDictionary<string, List<IVsSccProject2>>();
            _lastNewFileScan = DateTime.MinValue;
        }

        public List<IVsSccProject2> GetProjectsSelectionForFile(string filename)
        {
           return GetProjectsSelectionForFileInternal(filename);
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

        public async Task AddProject(EnvDTE.Project envProject)
        {
            IVsHierarchy projectHierarchy = null;
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            IVsSolution sol = (IVsSolution)_sccProvider.GetService(typeof(SVsSolution));

            if (sol.GetProjectOfUniqueName(envProject.UniqueName, out projectHierarchy) == 0)
            {
                var project = projectHierarchy as IVsSccProject2;
                if (project != null)
                {
                    await AddProject(project);

                    if (!_projects.Contains(project) && project != null)
                    {
                        _projects.Add(project);
                    }

                    var files = SolutionExtensions.GetProjectFiles(envProject).ToList();

                    foreach (var file in files)
                    {
                        AddFileToList(file.ToLower(), project);
                    }
                }
            }
        }


        public async Task RegisterSolution()
        {
            await ScanSolution();
        }

        #endregion

        private List<IVsSccProject2> GetProjectsSelectionForFileInternal(string filename)
        {
            List<IVsSccProject2> projects;
            var filePath = filename;
            if (_fileProjectLookup == null || !_fileProjectLookup.TryGetValue(filePath, out projects))
            {
                return new List<IVsSccProject2>();
            }
            return projects;
        }

        private async Task ScanSolution()
        {
            var projects = await SolutionExtensions.GetLoadedControllableProjects();

            //TODO MAke sure I want to do this
            _fileProjectLookup = new ConcurrentDictionary<string, List<IVsSccProject2>>();
            foreach (var project in projects)
            {
                await AddProject(project);
            }

        }

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
