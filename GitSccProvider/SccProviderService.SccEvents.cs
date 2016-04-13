using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using GitScc;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace GitScc
{
    public partial class SccProviderService : IVsSccManager3
    {


        #region IVsSccManager2 interface functions

        public int BrowseForProject(out string pbstrDirectory, out int pfOK)
        {
            // Obsolete method
            pbstrDirectory = null;
            pfOK = 0;
            return VSConstants.E_NOTIMPL;
        }

        public int CancelAfterBrowseForProject()
        {
            // Obsolete method
            return VSConstants.E_NOTIMPL;
        }

        /// <summary>
        /// Returns whether the source control provider is fully installed
        /// </summary>
        public int IsInstalled(out int pbInstalled)
        {
            // All source control packages should always return S_OK and set pbInstalled to nonzero
            pbInstalled = 1;
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Provide source control icons for the specified files and returns scc status of files
        /// </summary>
        /// <returns>The method returns S_OK if at least one of the files is controlled, S_FALSE if none of them are</returns>
        public int GetSccGlyph([InAttribute] int cFiles, [InAttribute] string[] rgpszFullPaths, [OutAttribute] VsStateIcon[] rgsiGlyphs, [OutAttribute] uint[] rgdwSccStatus)
        {
            for (int i = 0; i < cFiles; i++)
            {
                GitFileStatus status = _active ? GetFileStatus(rgpszFullPaths[i]) : GitFileStatus.NotControlled;
                __SccStatus sccStatus;

                switch (status)
                {
                    case GitFileStatus.Tracked:
                        rgsiGlyphs[i] = SccGlyphsHelper.Tracked;
                        sccStatus = __SccStatus.SCC_STATUS_CONTROLLED;
                        break;

                    case GitFileStatus.Modified:
                        rgsiGlyphs[i] = SccGlyphsHelper.Modified;
                        sccStatus = __SccStatus.SCC_STATUS_CONTROLLED | __SccStatus.SCC_STATUS_CHECKEDOUT | __SccStatus.SCC_STATUS_OUTBYUSER;
                        break;

                    case GitFileStatus.New:
                        rgsiGlyphs[i] = SccGlyphsHelper.New;
                        sccStatus = __SccStatus.SCC_STATUS_CONTROLLED | __SccStatus.SCC_STATUS_CHECKEDOUT | __SccStatus.SCC_STATUS_OUTBYUSER;
                        break;

                    case GitFileStatus.Added:
                    case GitFileStatus.Staged:
                        rgsiGlyphs[i] = status == GitFileStatus.Added ? SccGlyphsHelper.Added : SccGlyphsHelper.Staged;
                        sccStatus = __SccStatus.SCC_STATUS_CONTROLLED | __SccStatus.SCC_STATUS_CHECKEDOUT | __SccStatus.SCC_STATUS_OUTBYUSER;
                        break;

                    case GitFileStatus.NotControlled:
                        rgsiGlyphs[i] = SccGlyphsHelper.NotControlled;
                        sccStatus = __SccStatus.SCC_STATUS_NOTCONTROLLED;
                        break;

                    case GitFileStatus.Ignored:
                        rgsiGlyphs[i] = SccGlyphsHelper.Ignored;
                        sccStatus = __SccStatus.SCC_STATUS_NOTCONTROLLED;
                        break;

                    case GitFileStatus.Conflict:
                        rgsiGlyphs[i] = SccGlyphsHelper.Conflict;
                        sccStatus = __SccStatus.SCC_STATUS_CONTROLLED | __SccStatus.SCC_STATUS_CHECKEDOUT | __SccStatus.SCC_STATUS_OUTBYUSER | __SccStatus.SCC_STATUS_MERGED;
                        break;

                    case GitFileStatus.Merged:
                        rgsiGlyphs[i] = SccGlyphsHelper.Merged;
                        sccStatus = __SccStatus.SCC_STATUS_CONTROLLED | __SccStatus.SCC_STATUS_CHECKEDOUT | __SccStatus.SCC_STATUS_OUTBYUSER;
                        break;

                    default:
                        sccStatus = __SccStatus.SCC_STATUS_INVALID;
                        break;
                }

                if (rgdwSccStatus != null)
                    rgdwSccStatus[i] = (uint)sccStatus;
            }
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Determines the corresponding scc status glyph to display, given a combination of scc status flags
        /// </summary>
        public int GetSccGlyphFromStatus([InAttribute] uint dwSccStatus, [OutAttribute] VsStateIcon[] psiGlyph)
        {
            // This method is called when some user (e.g. like classview) wants to combine icons
            // (Unfortunately classview uses a hardcoded mapping)
            psiGlyph[0] = VsStateIcon.STATEICON_BLANK;
            return VSConstants.S_OK;
        }

        /// <summary>
        /// One of the most important methods in a source control provider, is called by projects that are under source control when they are first opened to register project settings
        /// </summary>
        public int RegisterSccProject([InAttribute] IVsSccProject2 pscp2Project, [InAttribute] string pszSccProjectName, [InAttribute] string pszSccAuxPath, [InAttribute] string pszSccLocalPath, [InAttribute] string pszProvider)
        {
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Called by projects registered with the source control portion of the environment before they are closed. 
        /// </summary>
        public int UnregisterSccProject([InAttribute] IVsSccProject2 pscp2Project)
        {
            return VSConstants.S_OK;
        }

        #endregion

        #region IVsSccManager3 Members

        public bool IsBSLSupported()
        {
            return true;
        }

        #endregion

        #region IVsSccManagerTooltip interface functions

        /// <summary>
        /// Called by solution explorer to provide tooltips for items. Returns a text describing the source control status of the item.
        /// </summary>
        public int GetGlyphTipText([In] IVsHierarchy phierHierarchy, [InAttribute] uint itemidNode, out string pbstrTooltipText)
        {
            pbstrTooltipText = "";
            GitFileStatus status = ThreadHelper.JoinableTaskFactory.Run(async delegate
            {
                GitFileStatus fileStatus = await GetFileStatus(phierHierarchy, itemidNode);
                return fileStatus;
            });
            pbstrTooltipText = status.ToString(); //TODO: use resources
            return VSConstants.S_OK;
        }
        #endregion

        #region IVsSccGlyphs Members

        public int GetCustomGlyphList(uint BaseIndex, out uint pdwImageListHandle)
        {
            pdwImageListHandle = SccGlyphsHelper.GetCustomGlyphList(BaseIndex);
            return VSConstants.S_OK;
        }

        #endregion
    }
}
