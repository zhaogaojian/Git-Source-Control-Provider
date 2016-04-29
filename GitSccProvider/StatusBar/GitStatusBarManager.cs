using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using IVsSccCurrentBranch = Microsoft.VisualStudio.Shell.IVsSccCurrentBranch;
using OleMenuCommandService = Microsoft.VisualStudio.Shell.OleMenuCommandService;
using Task = System.Threading.Tasks.Task;
using MsVsShell = Microsoft.VisualStudio.Shell;

namespace GitScc.StatusBar
{
    public abstract class GitStatusBarManager : IOleCommandTarget
    {
        private GitRepository _repository;
        protected IStatusBarService StatusBarService { get; }
        protected Guid CommandSetGuid { get; }
        protected int BranchMenuCmId { get; }
        protected int BranchCommandMenuCmId { get; }

        private List<Tuple<string,MenuCommand>> _branchMenuCommands;
        private List<MenuCommand> _branchCommandMenuCommands;

        private OleMenuCommandService _menuCommandService;
        private IServiceContainer _serviceProvider;

        protected GitStatusBarManager(Guid commandSetGuid, int branchMenuCmId, int branchCommandMenuCmId, IServiceContainer serviceProvider, IStatusBarService statusBarService)
        {
            BranchMenuCmId = branchMenuCmId;
            BranchCommandMenuCmId = branchCommandMenuCmId;
            StatusBarService = statusBarService;
            CommandSetGuid = commandSetGuid;
            _menuCommandService = serviceProvider.GetService(typeof(IMenuCommandService)) as MsVsShell.OleMenuCommandService;
            _serviceProvider = serviceProvider;
            _branchMenuCommands = new List<Tuple<string, MenuCommand>>();
        }


        protected GitRepository CurrentRepository
        {
            get { return _repository; }
            set
            {
                SetupTracker(value);
            }
        }

        public async Task SetActiveRepository(GitRepository repository)
        {
            if (repository != null)
            {
                CurrentRepository = repository;
                await UpdateBranchMenu();
            }
        }

        protected async Task LoadBranches(List<string> branchNames)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            if (_menuCommandService != null)
            {
                if (_branchMenuCommands?.Count > 0)
                {
                    //clear old branches
                    foreach (var branchMenuCommand in _branchMenuCommands)
                    {
                        _menuCommandService.RemoveCommand(branchMenuCommand.Item2);
                    }
                }
                _branchMenuCommands = new List<Tuple<string, MenuCommand>>();
                for (int i = 0; i < branchNames.Count; i++)
                {
                    var cmdID = new CommandID(CommandSetGuid, BranchMenuCmId + i);
                    var mc = new OleMenuCommand(
                        new EventHandler(OnBranchSelection), cmdID, branchNames[i]);
                    _menuCommandService.AddCommand(mc);
                   // mc.BeforeQueryStatus += new EventHandler(OnBranchQueryStatus);
                    _branchMenuCommands.Add(new Tuple<string, MenuCommand>(branchNames[i],mc));

                }
                await UpdateUI();
            }
        }

        private void OnBranchQueryStatus(object sender, EventArgs e)
        {
            OleMenuCommand menuCommand = sender as OleMenuCommand;
            if (null != menuCommand)
            {
                int idx = (int) menuCommand.CommandID.ID - BranchMenuCmId;
                //int MRUItemIndex = menuCommand.CommandID.ID - this.baseMRUID;
                if (idx >= 0 && idx < _branchMenuCommands.Count)
                {
                    menuCommand.Text = _branchMenuCommands[idx].Item1;
                }
            }
        }

        private async Task UpdateUI()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            IVsUIShell vsShell = (IVsUIShell)_serviceProvider.GetService(typeof(IVsUIShell));
            if (vsShell != null)
            {
                int hr = vsShell.UpdateCommandUI(0);
                Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(hr);
            }
        }



        protected void SetupTracker(GitRepository tracker)
        {
            if (tracker == _repository)
            {
                return;
            }
            else
            {
                if (_repository != null)
                {
                    CurrentRepository.BranchChanged -= CurrentTracker_BranchChanged;
                    CurrentRepository.FileStatusUpdate -= CurrentTracker_FileStatusUpdate;

                }
                _repository = tracker;
                if (CurrentRepository != null)
                {
                    CurrentRepository.BranchChanged += CurrentTracker_BranchChanged;
                    CurrentRepository.FileStatusUpdate += CurrentTracker_FileStatusUpdate; 
                }
            }
        }


        private void OnBranchSelection(object sender, EventArgs e)
        {
            var menuCommand = sender as OleMenuCommand;
            if (null != menuCommand)
            {
                int branchIndex = menuCommand.CommandID.ID - BranchMenuCmId;
                if (branchIndex >= 0 && branchIndex < _branchMenuCommands.Count)
                {
                    string selection = this._branchMenuCommands[branchIndex].Item1;
                    System.Windows.Forms.MessageBox.Show(
    string.Format(CultureInfo.CurrentCulture,
                  "Selected {0}", selection));

                }
            }
        }

        private async void CurrentTracker_FileStatusUpdate(object sender, GitFilesStatusUpdateEventArgs e)
        {
           await OnFileStatusUpdate(e.Files);
        }

        private async void CurrentTracker_BranchChanged(object sender, string e)
        {
            await OnBranchChanged(e);
        }

        protected virtual async Task OnFileStatusUpdate(List<GitFile> files)
        {
            StatusBarService.PendingChangeCount = files.Count;
        }

        protected virtual async Task OnBranchChanged(string branchName)
        {
            StatusBarService.BranchName = branchName;
        }

        private bool TryParseBranchName(uint cmdId, out string label)
        {
            label = "";
            int idx = (int)cmdId - BranchMenuCmId;
            if (cmdId >= BranchMenuCmId &&
                       cmdId < BranchMenuCmId + _branchMenuCommands.Count)
            {
                //int idx = (int)cmdId - PackageIds.cmdidBranchMenuCommandStart;
                label = _branchMenuCommands[idx].Item1;
                //menuCommand.Text
                //SetOleCmdText(pCmdText, _branchMenuCommands[idx].Item1);
                //cmdf |= OLECMDF.OLECMDF_ENABLED;
                return true;
            }
            return false;
        }

        public void SetOleCmdText(IntPtr pCmdText, string text)
        {
            OLECMDTEXT CmdText = (OLECMDTEXT)Marshal.PtrToStructure(pCmdText, typeof(OLECMDTEXT));
            char[] buffer = text.ToCharArray();
            IntPtr pText = (IntPtr)((long)pCmdText + (long)Marshal.OffsetOf(typeof(OLECMDTEXT), "rgwz"));
            IntPtr pCwActual = (IntPtr)((long)pCmdText + (long)Marshal.OffsetOf(typeof(OLECMDTEXT), "cwActual"));
            // The max chars we copy is our string, or one less than the buffer size,
            // since we need a null at the end.
            int maxChars = (int)Math.Min(CmdText.cwBuf - 1, buffer.Length);
            Marshal.Copy(buffer, 0, pText, maxChars);
            // append a null
            Marshal.WriteInt16((IntPtr)((long)pText + (long)maxChars * 2), (Int16)0);
            // write out the length + null char
            Marshal.WriteInt32(pCwActual, maxChars + 1);
        }

        protected abstract Task UpdateBranchMenu();

        int IOleCommandTarget.QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
        {
            OLECMDF cmdf = OLECMDF.OLECMDF_SUPPORTED;
            string label;
            if(TryParseBranchName(prgCmds[0].cmdID, out label))
            {
                SetOleCmdText(pCmdText, label);
                cmdf |= OLECMDF.OLECMDF_ENABLED;

                prgCmds[0].cmdf = (uint)(cmdf);
                return VSConstants.S_OK;
            }
            else
            {
                        return (int)(Microsoft.VisualStudio.OLE.Interop.Constants.OLECMDERR_E_NOTSUPPORTED);
            }
           
            //throw new NotImplementedException();
        }

        int IOleCommandTarget.Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            return ((IOleCommandTarget)_menuCommandService).Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
        }
    }
}
