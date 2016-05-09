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
        
        protected IStatusBarService StatusBarService { get; }
        protected Guid CommandSetGuid { get; }
        protected int BranchMenuCmId { get; }
        protected int BranchCommandMenuCmId { get; }
        protected int RepositoryCommandMenuCmId { get; }

        private List<Tuple<string,MenuCommand>> _branchMenuCommands;
        private List<Tuple<string, MenuCommand>> _branchRepositoryCommands;
        private List<Tuple<string, MenuCommand>> _repositoryCommands;
        private List<Tuple<string, MenuCommand>> _branchCommandMenuCommands;

        private OleMenuCommandService _menuCommandService;
        private IServiceContainer _serviceProvider;

        protected GitStatusBarManager(Guid commandSetGuid, int branchMenuCmId, int branchCommandMenuCmId, int repositoryCommandMenuCmId,  IServiceContainer serviceProvider, IStatusBarService statusBarService)
        {
            BranchMenuCmId = branchMenuCmId;
            BranchCommandMenuCmId = branchCommandMenuCmId;
            RepositoryCommandMenuCmId = repositoryCommandMenuCmId;
            StatusBarService = statusBarService;
            CommandSetGuid = commandSetGuid;
            _menuCommandService = serviceProvider.GetService(typeof(IMenuCommandService)) as MsVsShell.OleMenuCommandService;
            _serviceProvider = serviceProvider;
            _branchCommandMenuCommands = new List<Tuple<string, MenuCommand>>();

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
                    _branchMenuCommands.Add(new Tuple<string, MenuCommand>(branchNames[i],mc));
                }
            }
        }

        protected async Task LoadRepositoryCommands(List<string> commands)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            if (_menuCommandService != null)
            {
                if (_repositoryCommands?.Count > 0)
                {
                    //clear old branches
                    foreach (var command in _repositoryCommands)
                    {
                        _menuCommandService.RemoveCommand(command.Item2);
                    }
                }
                _repositoryCommands = new List<Tuple<string, MenuCommand>>();
                for (int i = 0; i < commands.Count; i++)
                {
                    var cmdID = new CommandID(CommandSetGuid, RepositoryCommandMenuCmId + i);
                    var mc = new OleMenuCommand(
                        new EventHandler(OnRepositoryCommandSelection), cmdID, commands[i]);
                    _menuCommandService.AddCommand(mc);
                    _repositoryCommands.Add(new Tuple<string, MenuCommand>(commands[i], mc));
                }
            }
        }

        protected async Task LoadBranchCommands(List<string> commands)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            if (_menuCommandService != null)
            {
                if (_repositoryCommands?.Count > 0)
                {
                    //clear old branches
                    foreach (var command in _branchCommandMenuCommands)
                    {
                        _menuCommandService.RemoveCommand(command.Item2);
                    }
                }
                _branchCommandMenuCommands = new List<Tuple<string, MenuCommand>>();
                for (int i = 0; i < commands.Count; i++)
                {
                    var cmdID = new CommandID(CommandSetGuid, BranchCommandMenuCmId + i);
                    var mc = new OleMenuCommand(
                        new EventHandler(OnRepositoryCommandSelection), cmdID, commands[i]);
                    _menuCommandService.AddCommand(mc);
                    _branchCommandMenuCommands.Add(new Tuple<string, MenuCommand>(commands[i], mc));
                }
            }
        }

        #region Command Selections

        protected abstract Task OnRepositoryCommandSelection(string command);
        protected abstract Task OnBranchSelection(string command);

        private void OnRepositoryCommandSelection(object sender, EventArgs e)
        {
            OleMenuCommand menuCommand = sender as OleMenuCommand;
            string command;
            if (TryParseCommandName(menuCommand, RepositoryCommandMenuCmId, _repositoryCommands, out command))
            {
                OnRepositoryCommandSelection(command);
            }
        }

        private void OnBranchSelection(object sender, EventArgs e)
        {
            var menuCommand = sender as OleMenuCommand;
            string command;
            if (TryParseCommandName(menuCommand, BranchMenuCmId, _branchMenuCommands, out command))
            {
                OnBranchSelection(command);
            }
        }

    #endregion


        private bool TryParseCommandName(OleMenuCommand menuCommand, int cmId, List<Tuple<string, MenuCommand>> commands, out string commandText)
        {
            commandText = null;
            if (menuCommand != null)
            {
                int idx = menuCommand.CommandID.ID - cmId;
                if (idx >= 0 && idx < commands.Count)
                {
                    commandText = commands[idx].Item1;
                    return true;
                }
            }
            return false;
        }

        private bool TryParseBranchName(uint cmdId, out string label)
        {
            label = "";
            int idx = (int)cmdId - BranchMenuCmId;
            if (cmdId >= BranchMenuCmId &&
                       cmdId < BranchMenuCmId + _branchMenuCommands.Count)
            {
                label = _branchMenuCommands[idx].Item1;
                return true;
            }
            return false;
        }

        private bool TryParseRepositoryCommand(uint cmdId, out string label)
        {
            label = "";
            if (_repositoryCommands != null)
            {
                int idx = (int) cmdId - RepositoryCommandMenuCmId;
                if (cmdId >= RepositoryCommandMenuCmId &&
                    cmdId < RepositoryCommandMenuCmId + _repositoryCommands?.Count)
                {
                    label = _repositoryCommands[idx].Item1;
                    return true;
                }
            }
            return false;
        }

        private bool TryParseBranchCommand(uint cmdId, out string label)
        {
            label = "";
            if (_branchCommandMenuCommands != null)
            {
                int idx = (int)cmdId - BranchCommandMenuCmId;
                if (cmdId >= BranchCommandMenuCmId &&
                    cmdId < BranchCommandMenuCmId + _branchCommandMenuCommands?.Count)
                {
                    label = _branchCommandMenuCommands[idx].Item1;
                    return true;
                }
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

        protected abstract Task UpdateRepsitoryCommands();



        int IOleCommandTarget.QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
        {
            OLECMDF cmdf = OLECMDF.OLECMDF_SUPPORTED;
            string label;
            if(TryParseBranchName(prgCmds[0].cmdID, out label))
            {
                VsShellUtilities.SetOleCmdText(pCmdText,label);
               // SetOleCmdText(pCmdText, label);
                cmdf |= OLECMDF.OLECMDF_ENABLED;
                if(string.Equals(label,StatusBarService.BranchName,StringComparison.OrdinalIgnoreCase))
                {
                    cmdf |= OLECMDF.OLECMDF_LATCHED;
                }

                prgCmds[0].cmdf = (uint)(cmdf);
                return VSConstants.S_OK;
            }
            else if(TryParseRepositoryCommand(prgCmds[0].cmdID, out label) || TryParseBranchCommand(prgCmds[0].cmdID, out label))
            {
                VsShellUtilities.SetOleCmdText(pCmdText, label);
                //SetOleCmdText(pCmdText, label);
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
