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
    public class SccProviderSolutionCache
    {
        private BasicSccProvider _sccProvider;
        private List<IVsSccProject2> _projects;
        private ConcurrentDictionary<string, List<IVsSccProject2>> _fileProjectLookup;

        public SccProviderSolutionCache(BasicSccProvider provider)
        {
            _sccProvider = provider;
            _projects = new List<IVsSccProject2>();
            _fileProjectLookup = new ConcurrentDictionary<string, List<IVsSccProject2>>();
        }

        private void AddFileToList(string filename, IVsSccProject2 project)
        {
            List<IVsSccProject2> projects;

            if (!_fileProjectLookup.TryGetValue(filename, out projects))
            {
                _fileProjectLookup.TryAdd(filename, new List<IVsSccProject2> {project});
            }

            else if(!projects.Contains(project))
            {
                projects.Add(project);
            }
        }

        public List<IVsSccProject2> GetProjectsForFile(string filename)
        {
            return GetProjectsForFile(filename,true);
        }

        private List<IVsSccProject2> GetProjectsForFile(string filename, bool search)
        {
            List<IVsSccProject2> projects;
            if (!_fileProjectLookup.TryGetValue(filename, out projects))
            {
                if (!search)
                {
                    projects = new List<IVsSccProject2>();
                }
                else
                {
                    ScanSolution();
                    projects = GetProjectsForFile(filename, false);
                }
            }
            return projects;
        }

        public void AddProject(IVsSccProject2 project)
        {
            if (!_projects.Contains(project))
            {
                _projects.Add(project);
            }
            var files = SolutionExtensions.GetProjectFiles(project);
            foreach (var file in files)
            {
                AddFileToList(file, project);
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


    }
}
