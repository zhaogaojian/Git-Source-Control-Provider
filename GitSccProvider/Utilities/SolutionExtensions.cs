using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using EnvDTE;
using EnvDTE80;
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


        /// <summary>
        /// Gets the list of source controllable files in the specified project
        /// </summary>
        public static IList<string>  GetProjectFiles(IVsSccProject2 pscp2Project)
        {
            return GetProjectFiles(pscp2Project, VSConstants.VSITEMID_ROOT);
        }

        /// <summary>
        /// Gets the list of source controllable files in the specified project
        /// </summary>
        public static IList<string> GetProjectFiles(IVsSccProject2 pscp2Project, uint startItemId)
        {
            IList<string> projectFiles = new List<string>();
            IVsHierarchy hierProject = (IVsHierarchy)pscp2Project;
            IList<uint> projectItems = GetProjectItems(hierProject, startItemId);

            foreach (uint itemid in projectItems)
            {
                IList<string> sccFiles = GetNodeFiles(pscp2Project, itemid);
                foreach (string file in sccFiles)
                {
                    projectFiles.Add(file);
                }
            }

            return projectFiles;
        }


        private static IList<string> GetNodeFiles(IVsSccProject2 pscp2, uint itemid)
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

                if (pscp2.GetSccFiles(itemid, pathStr, flags) == VSConstants.S_OK)
                {
                    for (int elemIndex = 0; elemIndex < pathStr[0].cElems; elemIndex++)
                    {
                        IntPtr pathIntPtr = Marshal.ReadIntPtr(pathStr[0].pElems, elemIndex * IntPtr.Size);
                        String path = Marshal.PtrToStringAuto(pathIntPtr);

                        sccFiles.Add(path);

                        // See if there are special files
                        if (flags.Length > 0 && flags[0].cElems > 0)
                        {
                            int flag = Marshal.ReadInt32(flags[0].pElems, elemIndex * IntPtr.Size);

                            if (flag != 0)
                            {
                                // We have special files
                                CALPOLESTR[] specialFiles = new CALPOLESTR[1];
                                CADWORD[] specialFlags = new CADWORD[1];

                                if (pscp2.GetSccSpecialFiles(itemid, path, specialFiles, specialFlags) == VSConstants.S_OK)
                                {
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
                        }

                        Marshal.FreeCoTaskMem(pathIntPtr);
                    }
                    if (pathStr[0].cElems > 0)
                    {
                        Marshal.FreeCoTaskMem(pathStr[0].pElems);
                    }
                }
            }

            return sccFiles;
        }

        private static IList<uint> GetProjectItems(IVsHierarchy pHier, uint startItemid)
        {
            List<uint> projectNodes = new List<uint>();

            //if (pHier == null)
            //{
            //    return projectNodes;
            //}

            // The method does a breadth-first traversal of the project's hierarchy tree
            Queue<uint> nodesToWalk = new Queue<uint>();
            nodesToWalk.Enqueue(startItemid);

            while (nodesToWalk.Count > 0)
            {
                uint node = nodesToWalk.Dequeue();
                projectNodes.Add(node);

               // DebugWalkingNode(pHier, node);

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
        public static bool IsSolutionFolderProject(this IVsHierarchy pHier)
        {
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


        public static async Task<IList<Project>> GetProjectsAsync()
        {
             return await Task.Run(() => Projects());
        }


        public static DTE2 GetActiveIDE()
        {
            // Get an instance of currently running Visual Studio IDE.
            DTE2 dte2 = Package.GetGlobalService(typeof(DTE)) as DTE2;
            return dte2;
        }

        public static IList<Project> Projects()
        {
            Projects projects = GetActiveIDE().Solution.Projects;
            List<Project> list = new List<Project>();
            var item = projects.GetEnumerator();
            while (item.MoveNext())
            {
                var project = item.Current as Project;
                if (project == null)
                {
                    continue;
                }

                if (project.Kind == ProjectKinds.vsProjectKindSolutionFolder)
                {
                    list.AddRange(GetSolutionFolderProjects(project));
                }
                else
                {
                    list.Add(project);
                }
            }

            return list;
        }

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
    }
}

