using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using IVsSccCurrentBranch = Microsoft.VisualStudio.Shell.IVsSccCurrentBranch;
using OleMenuCommandService = Microsoft.VisualStudio.Shell.OleMenuCommandService;
using Task = System.Threading.Tasks.Task;
using MsVsShell = Microsoft.VisualStudio.Shell;

namespace GitScc.StatusBar
{
    public abstract class GitStatusBarManager
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
                    var cmdID = new CommandID(CommandSetGuid, PackageIds.cmdidBranchmenuStart + i);
                    var mc = new OleMenuCommand(
                        new EventHandler(OnBranchSelection), cmdID, branchNames[i]);
                    mc.Visible = true;
                    mc.Enabled = true;
                    _menuCommandService.AddCommand(mc);

                }
                await UpdateUI();
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
        protected abstract Task UpdateBranchMenu();
    }
}
