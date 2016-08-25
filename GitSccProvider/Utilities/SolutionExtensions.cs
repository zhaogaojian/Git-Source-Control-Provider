using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using EnvDTE;
using EnvDTE80;
using GitScc;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

namespace GitSccProvider.Utilities
{
    public static class SolutionExtensions
    {
        public static readonly Guid GuidSolutionFolderProject = new Guid(0x2150e333, 0x8fdc, 0x42a3, 0x94, 0x74, 0x1a, 0x39, 0x56, 0xd4, 0x6d, 0xe8);


        #region File Lists


        public static async Task<List<IVsSccProject2>> GetLoadedControllableProjects()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            var list = new List<IVsSccProject2>();

            IVsSolution sol = await GetActiveSolution();
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
                if (sccProject2 != null && await IsProjectInGitRepoitory(sccProject2))
                {
                    list.Add(sccProject2);
                }
            }

            return list;
        }

        public static async Task<bool> IsProjectInGitRepoitory(IVsSccProject2 project)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            var filename = await GetProjectFileName(project as IVsHierarchy);
            if (!string.IsNullOrWhiteSpace(filename))
            {
                return RepositoryManager.Instance.IsProjectInGitRepoitory(filename);
            }
            return false;
        }

        public static bool CanAddSelectedProjectToGitRepoitory()
        {
            var project = GetSelectedProject();
            return !IsProjectInGit(project.FullName);
        }

        public static bool IsProjectInGit(string filename)
        {
            if (!string.IsNullOrWhiteSpace(filename))
            {
                var repo = RepositoryManager.Instance.GetTrackerForPath(filename);
                if (repo != null)
                {
                    var status = repo.GetFileStatus(filename, true);
                    if(status != GitFileStatus.New && status != GitFileStatus.NotControlled)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static  Project GetSelectedProject()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            IntPtr hierarchyPointer, selectionContainerPointer;
            Object selectedObject = null;
            IVsMultiItemSelect multiItemSelect;
            uint projectItemId;
            IVsMonitorSelection monitorSelection =
                    (IVsMonitorSelection)Package.GetGlobalService(
                    typeof(SVsShellMonitorSelection));

            monitorSelection.GetCurrentSelection(out hierarchyPointer,
                                                 out projectItemId,
                                                 out multiItemSelect,
                                                 out selectionContainerPointer);

            IVsHierarchy selectedHierarchy = null;
            try
            {
                selectedHierarchy = Marshal.GetTypedObjectForIUnknown(
                                                     hierarchyPointer,
                                                     typeof(IVsHierarchy)) as IVsHierarchy;
            }
            catch (Exception)
            {
                return null;
            }

            if (selectedHierarchy != null)
            {
                ErrorHandler.ThrowOnFailure(selectedHierarchy.GetProperty(
                                                  projectItemId,
                                                  (int)__VSHPROPID.VSHPROPID_ExtObject,
                                                  out selectedObject));
            }

            Project selectedProject = selectedObject as Project;

            return selectedProject;
        }

        public static IVsHierarchy GetSelectedProjectHierarchy()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            IntPtr hierarchyPointer, selectionContainerPointer;
            IVsMultiItemSelect multiItemSelect;
            uint projectItemId;
            IVsMonitorSelection monitorSelection =
                    (IVsMonitorSelection)Package.GetGlobalService(
                    typeof(SVsShellMonitorSelection));

            monitorSelection.GetCurrentSelection(out hierarchyPointer,
                                                 out projectItemId,
                                                 out multiItemSelect,
                                                 out selectionContainerPointer);
            try
            {
                return Marshal.GetTypedObjectForIUnknown(
                                                     hierarchyPointer,
                                                     typeof(IVsHierarchy)) as IVsHierarchy;
            }
            catch (Exception)
            {
                return null;
            }


        }

        public static async Task<IVsHierarchy> GetIVsHierarchy(Project project)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            var solutionService = await GetActiveSolution();
            IVsHierarchy projectHierarchy = null;

            if (solutionService.GetProjectOfUniqueName(project.UniqueName, out projectHierarchy) == VSConstants.S_OK)
            {
                return projectHierarchy;
            }
            return null;
        }

        /// <summary>
        /// Gets the list of source controllable files in the specified project
        /// </summary>
        public static async Task<IList<string>>  GetProjectFiles(IVsSccProject2 pscp2Project)
        {
            return await GetProjectFiles(pscp2Project, VSConstants.VSITEMID_ROOT);
        }

        /// <summary>
        /// Gets the list of source controllable files in the specified project
        /// </summary>
        public static async Task<IList<string>> GetProjectFiles(IVsSccProject2 pscp2Project, uint startItemId)
        {
            IList<string> projectFiles = new List<string>();
            if (pscp2Project == null)
            {
                return projectFiles;
            }
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            IVsHierarchy hierProject = (IVsHierarchy)pscp2Project;
            
            var itemid = VSConstants.VSITEMID_ROOT;
            object objProj;
            hierProject.GetProperty(itemid, (int)__VSHPROPID.VSHPROPID_ExtObject, out objProj);
            var project = objProj as EnvDTE.Project;
            if (project != null && project.Kind != ProjectKinds.vsProjectKindSolutionFolder)
            {
                try
                {
                    FindFilesInProject(project.ProjectItems, projectFiles);
                }
                catch (Exception)
                {
                    Debug.WriteLine("==== Error With : " + project.Name + " Type : " + project.Kind);
                }
            }
            return projectFiles;
        }

        public static IList<string> GetProjectFiles(EnvDTE.Project project)
        {
            IList<string> projectFiles = new List<string>();

            if (project != null && project.Kind != ProjectKinds.vsProjectKindSolutionFolder)
            {
                try
                {
                    FindFilesInProject(project.ProjectItems, projectFiles);
                }
                catch (Exception)
                    {
                    Debug.WriteLine("==== Error With : " + project.Name + " Type : " + project.Kind);
                }
            }
            return projectFiles;
        }

        private static void FindFilesInProject(ProjectItems items, IList<string> projectFiles)
        {
            foreach (ProjectItem projectItem in items)
            {
                if (projectItem.Kind != ProjectKinds.vsProjectKindSolutionFolder)
                {
                    projectFiles.Add(projectItem.FileNames[0]);
                }

                if (projectItem.ProjectItems != null || projectItem.ProjectItems.Count > 0)
                {
                    FindFilesInProject(projectItem.ProjectItems, projectFiles);
                }
            }
        }


        public static async Task<IList<uint>> GetProjectItems(IVsHierarchy pHier, uint startItemid)
        {
            List<uint> projectNodes = new List<uint>();

            // The method does a breadth-first traversal of the project's hierarchy tree
            Queue<uint> nodesToWalk = new Queue<uint>();
            nodesToWalk.Enqueue(startItemid);

            while (nodesToWalk.Count > 0)
            {
                uint node = nodesToWalk.Dequeue();
                projectNodes.Add(node);

                // DebugWalkingNode(pHier, node);
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                object property = null;
                if (pHier?.GetProperty(node, (int)__VSHPROPID.VSHPROPID_FirstChild, out property) == VSConstants.S_OK)
                {
                    uint childnode = (uint)(int)property;
                    if (childnode == VSConstants.VSITEMID_NIL)
                    {
                        continue;
                    }

                   // DebugWalkingNode(pHier, childnode);

                    if ((pHier.GetProperty(childnode, (int)__VSHPROPID.VSHPROPID_Expandable, out property) == VSConstants.S_OK && (int)property != 0) ||
                        (pHier.GetProperty(childnode, (int)__VSHPROPID2.VSHPROPID_Container, out property) == VSConstants.S_OK && (bool)property))
                    {
                        nodesToWalk.Enqueue(childnode);
                    }
                    else
                    {
                        projectNodes.Add(childnode);
                    }

                    while (pHier.GetProperty(childnode, (int)__VSHPROPID.VSHPROPID_NextSibling, out property) == VSConstants.S_OK)
                    {
                        childnode = (uint)(int)property;
                        if (childnode == VSConstants.VSITEMID_NIL)
                        {
                            break;
                        }

                        //DebugWalkingNode(pHier, childnode);
                        //TODO exception to fix here
                        var propertyCache = pHier.GetProperty(childnode, (int) __VSHPROPID.VSHPROPID_Expandable,
                            out property);
                        object property2 = null;
                        var propertyCache2 = pHier.GetProperty(childnode, (int)__VSHPROPID.VSHPROPID_Name,
                          out property2);
                        if ((pHier.GetProperty(childnode, (int)__VSHPROPID.VSHPROPID_Expandable, out property) == VSConstants.S_OK && (int)property != 0) ||
                            (pHier.GetProperty(childnode, (int)__VSHPROPID2.VSHPROPID_Container, out property) == VSConstants.S_OK && (bool)property))
                        {
                            nodesToWalk.Enqueue(childnode);
                        }
                        else
                        {
                            projectNodes.Add(childnode);
                        }
                    }
                }

            }

            return projectNodes;
        }

        #endregion


        /// <summary>
        /// Checks whether the specified project is a solution folder
        /// </summary>
        public static async Task<bool> IsSolutionFolderProject(this IVsHierarchy pHier)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            IPersistFileFormat pFileFormat = pHier as IPersistFileFormat;
            if (pFileFormat != null)
            {
                Guid guidClassID;
                if (pFileFormat.GetClassID(out guidClassID) == VSConstants.S_OK &&
                    guidClassID.CompareTo(GuidSolutionFolderProject) == 0)
                {
                    return true;
                }
            }

            return false;
        }


        //public static async Task<IList<Project>> GetProjectsAsync()
        //{
        //     return await Task.Run(() => Projects());
        //}


        public static DTE2 GetActiveIDE()
        {
            // Get an instance of currently running Visual Studio IDE.
            DTE2 dte2 = Package.GetGlobalService(typeof(DTE)) as DTE2;
            return dte2;
        }
        public static async Task<IVsSolution> GetActiveSolution()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            IVsSolution sol = Package.GetGlobalService(typeof(SVsSolution)) as IVsSolution;
            return sol;
        }

        public static async Task<string> GetSolutionFileName()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            IVsSolution sol = await GetActiveSolution();
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

        public static async Task<string> GetProjectFileName(IVsHierarchy hierHierarchy)
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
        private static async Task<IList<string>> GetNodeFiles(IVsSccProject2 pscp2, uint itemid)
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


        //TODO remove these2.. replace with https://github.com/alphaleonis/AlphaFS/
        [DllImport("kernel32.dll")]
        static extern uint GetShortPathName(string longpath, StringBuilder sb, int buffer);

        [DllImport("kernel32.dll")]
        static extern uint GetLongPathName(string shortpath, StringBuilder sb, int buffer);


      

        //public static async Task<List<Project>> GetProjects()
        //{
        //    await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
        //    Projects projects = GetActiveIDE().Solution.Projects;
        //    List<Project> list = new List<Project>();
        //    var item = projects.GetEnumerator();
        //    while (item.MoveNext())
        //    {
        //        var project = item.Current as Project;
        //        if (project == null)
        //        {
        //            continue;
        //        }

        //        if (project.Kind == ProjectKinds.vsProjectKindSolutionFolder)
        //        {
        //            list.AddRange(GetSolutionFolderProjects(project));
        //        }
        //        else
        //        {
        //            list.Add(project);
        //        }
        //    }

        //    return list;
        //}

        private static IEnumerable<Project> GetSolutionFolderProjects(Project solutionFolder)
        {
            List<Project> list = new List<Project>();
            for (var i = 1; i <= solutionFolder.ProjectItems.Count; i++)
            {
                var subProject = solutionFolder.ProjectItems.Item(i).SubProject;
                if (subProject == null)
                {
                    continue;
                }

                // If this is another solution folder, do a recursive call, otherwise add
                if (subProject.Kind == ProjectKinds.vsProjectKindSolutionFolder)
                {
                    list.AddRange(GetSolutionFolderProjects(subProject));
                }
                else
                {
                    list.Add(subProject);
                }
            }
            return list;
        }

        public static void WriteMessageToOutputPane(string message,string title = "GIT Source Control")
        {
            DTE2 dte = GetActiveIDE();
            OutputWindowPanes panes =
                dte.ToolWindows.OutputWindow.OutputWindowPanes;

            try
            {
                // If the pane exists already, write to it.
                panes.Item(title);
            }
            catch (ArgumentException)
            {
                // Create a new pane and write to it.
                panes.Add(title);
            }

            foreach (EnvDTE.OutputWindowPane pane in panes)
            {
                if (pane.Name.Contains(title))
                {
                    pane.OutputString(message + "\n");
                    pane.Activate();
                    return;
                }
            }
        }
    }
}

