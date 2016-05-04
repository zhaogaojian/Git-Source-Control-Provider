using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GitScc.Utilities;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace GitScc.StatusBar
{
    public sealed class StandardGitStatusBarManager : GitApiStatusBarManager
    {

        public StandardGitStatusBarManager(Guid commandSetGuid, int branchMenuCmId, int branchCommandMenuCmId, int repositoryCommandMenuCmId, IServiceContainer serviceProvider, IStatusBarService statusBarService) 
            : base(commandSetGuid, branchMenuCmId, branchCommandMenuCmId, repositoryCommandMenuCmId, serviceProvider, statusBarService)
        {
        }



        protected override async Task UpdateBranchMenu()
        {
            await LoadBranches(CurrentRepository.LocalBranchNames);
        }

        protected override async Task UpdateRepsitoryCommands()
        {
           var repos =  RepositoryManager.Instance.Repositories.Select(x => x.Name).ToList();
           await LoadRepositoryCommands(repos);
        }

        protected override async Task OnBranchSelection(string command)
        {
            var branch = CurrentRepository.GetBranchInfo(includeRemote: false)
                .FirstOrDefault(x => string.Equals(x.Name,command,StringComparison.OrdinalIgnoreCase));
            var switchInfo = new SwitchBranchInfo()
            {
                BranchInfo = branch,
                Switch = true,
                Repository = CurrentRepository
            };
            await GitCommandWrappers.SwitchCommand(switchInfo);
        }
    }
}
