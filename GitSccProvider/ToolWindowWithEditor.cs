using System;
using System.Runtime.InteropServices;
using System.Windows.Controls;
using Microsoft.VisualStudio;
//using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Task = System.Threading.Tasks.Task;


namespace GitScc
{
    public abstract class ToolWindowWithEditor<T> : ToolWindowPane //, IOleCommandTarget, IVsFindTarget
        where T : Control
    {

        #region Constants

        private const int WM_KEYFIRST = 0x0100;
        private const int WM_KEYLAST = 0x0109;

        #endregion

        #region Private Fields

        protected T control;

        private IOleCommandTarget cachedEditorCommandTarget;
        private IVsTextView textView;
        private IVsCodeWindow codeWindow;
        private IVsInvisibleEditor invisibleEditor;
        private IVsFindTarget cachedEditorFindTarget;
        private Microsoft.VisualStudio.OLE.Interop.IServiceProvider cachedOleServiceProvider;

        #endregion

        public ToolWindowWithEditor()
            : base(null)
        {

        }

        #region Public Methods


        public abstract void UpdateRepositoryName(string repositoryName);

        #endregion

        #region Protected Overrides

        #endregion

        #region IOleCommandTarget Members

        

        #endregion

        #region IVsFindTarget Members

        public int Find(string pszSearch, uint grfOptions, int fResetStartPoint, IVsFindHelper pHelper, out uint pResult)
        {
            pResult = 0;
            return EditorFindTarget == null ? 0
                   : EditorFindTarget.Find(pszSearch, grfOptions, fResetStartPoint, pHelper, out pResult);
        }

        public int GetCapabilities(bool[] pfImage, uint[] pgrfOptions)
        {
            return EditorFindTarget == null ? 0
                   : EditorFindTarget.GetCapabilities(pfImage, pgrfOptions);
        }

        public int GetCurrentSpan(TextSpan[] pts)
        {
            return EditorFindTarget == null ? 0
                   : EditorFindTarget.GetCurrentSpan(pts);
        }

        public int GetFindState(out object ppunk)
        {
            ppunk = null;
            return EditorFindTarget == null ? 0
                   : EditorFindTarget.GetFindState(out ppunk);
        }

        public int GetMatchRect(RECT[] prc)
        {
            return EditorFindTarget == null ? 0
                   : EditorFindTarget.GetMatchRect(prc);
        }

        public int GetProperty(uint propid, out object pvar)
        {
            pvar = null;
            return EditorFindTarget == null ? 0
                   : EditorFindTarget.GetProperty(propid, out pvar);
        }

        public int GetSearchImage(uint grfOptions, IVsTextSpanSet[] ppSpans, out IVsTextImage ppTextImage)
        {
            ppTextImage = null;
            return EditorFindTarget == null ? 0
                   : EditorFindTarget.GetSearchImage(grfOptions, ppSpans, out ppTextImage);
        }

        public int MarkSpan(TextSpan[] pts)
        {
            return EditorFindTarget == null ? 0
                   : EditorFindTarget.MarkSpan(pts);
        }

        public int NavigateTo(TextSpan[] pts)
        {
            return EditorFindTarget == null ? 0
                   : EditorFindTarget.NavigateTo(pts);
        }

        public int NotifyFindTarget(uint notification)
        {
            return EditorFindTarget == null ? 0
                   : EditorFindTarget.NotifyFindTarget(notification);
        }

        public int Replace(string pszSearch, string pszReplace, uint grfOptions, int fResetStartPoint, IVsFindHelper pHelper, out int pfReplaced)
        {
            pfReplaced = 0;
            return EditorFindTarget == null ? 0
                   : EditorFindTarget.Replace(pszSearch, pszReplace, grfOptions, fResetStartPoint, pHelper, out pfReplaced);
        }

        public int SetFindState(object pUnk)
        {
            return EditorFindTarget == null ? 0
                   : EditorFindTarget.SetFindState(pUnk);
        }
        #endregion

        #region Private Properties

        /// <summary>
        /// The IOleCommandTarget for the editor that our tool window will forward all command requests to when it is the active tool window
        /// and the editor we are hosting has keyboard focus.
        /// </summary>
        private IOleCommandTarget EditorCommandTarget
        {
            get
            {
                return (this.cachedEditorCommandTarget ?? (this.cachedEditorCommandTarget = this.textView as IOleCommandTarget));
            }
        }

        /// <summary>
        /// The IVsFindTarget for the editor that our tool window will forward all find releated requests to when it is the active tool window
        /// and the editor we are hosting has keyboard focus.
        /// </summary>
        private IVsFindTarget EditorFindTarget
        {
            get
            {
                return (this.cachedEditorFindTarget ?? (this.cachedEditorFindTarget = this.textView as IVsFindTarget));
            }
        }

        /// <summary>
        /// The shell's service provider as an OLE service provider (needed to create the editor bits).
        /// </summary>
        private Microsoft.VisualStudio.OLE.Interop.IServiceProvider OleServiceProvider
        {
            get
            {
                if (this.cachedOleServiceProvider == null)
                {
                    //ServiceProvider.GlobalProvider is a System.IServiceProvider, but the editor pieces want an OLE.IServiceProvider, luckily the
                    //global provider is also IObjectWithSite and we can use that to extract its underlying (OLE) IServiceProvider object.
                    IObjectWithSite objWithSite = (IObjectWithSite)ServiceProvider.GlobalProvider;

                    Guid interfaceIID = typeof(Microsoft.VisualStudio.OLE.Interop.IServiceProvider).GUID;
                    IntPtr rawSP;
                    objWithSite.GetSite(ref interfaceIID, out rawSP);
                    try
                    {
                        if (rawSP != IntPtr.Zero)
                        {
                            //Get an RCW over the raw OLE service provider pointer.
                            this.cachedOleServiceProvider = (Microsoft.VisualStudio.OLE.Interop.IServiceProvider)Marshal.GetObjectForIUnknown(rawSP);
                        }
                    }
                    finally
                    {
                        if (rawSP != IntPtr.Zero)
                        {
                            //Release the raw pointer we got from IObjectWithSite so we don't cause leaks.
                            Marshal.Release(rawSP);
                        }
                    }
                }

                return this.cachedOleServiceProvider;
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Cleans up an existing editor if we are about to put a new one in place, used to close down the old editor bits as well as
        /// nulling out any cached objects that we have that came from the now dead editor.
        /// </summary>
        internal void ClearEditor()
        {
            if (this.codeWindow != null)
            {
                this.codeWindow.Close();
                this.codeWindow = null;
            }

            if (this.textView != null)
            {
                this.textView.CloseView();
                this.textView = null;
            }

            this.cachedEditorCommandTarget = null;
            this.cachedEditorFindTarget = null;
            this.invisibleEditor = null;
        }

        #endregion

    }
}
