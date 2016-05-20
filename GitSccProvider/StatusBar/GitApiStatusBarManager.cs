using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitScc.StatusBar
{
    public abstract class GitApiStatusBarManager : GitStatusBarManager
    {
        private GitRepository _repository;

        public GitApiStatusBarManager(Guid commandSetGuid, int branchMenuCmId, int branchCommandMenuCmId, int repositoryCommandMenuCmId,IServiceContainer serviceProvider, IStatusBarService statusBarService) 
            : base(commandSetGuid, branchMenuCmId, branchCommandMenuCmId,repositoryCommandMenuCmId ,serviceProvider, statusBarService)
        {
        }

        public async Task RefreshBranches()
        {
            await UpdateBranchMenu();
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
                StatusBarService.RepositoryName = CurrentRepository.Name;
                PendingChangeCount = CurrentRepository.ChangedFiles.Count();
                await UpdateBranchMenu();
                await UpdateRepsitoryCommands();
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
            PendingChangeCount = files.Count;
        }

        protected virtual async Task OnBranchChanged(string branchName)
        {
            StatusBarService.BranchName = branchName;
        }

        protected override async Task OnRepositoryCommandSelection(string command)
        {
            var newRepo =
                RepositoryManager.Instance.Repositories.FirstOrDefault(
                    x => string.Equals(x.Name, command, StringComparison.OrdinalIgnoreCase));
            RepositoryManager.Instance.ActiveTracker = newRepo;
            await SetActiveRepository(newRepo);
        }

        public virtual int PendingChangeCount
        {
            set
            {
                StatusBarService.PendingChangeCount = value;
                StatusBarService.PendingChangeDetail = string.Format(MenuResources.ChangesMenuTooltip,value);
                
            }
        }

    }
}
