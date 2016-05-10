using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using GitScc.UI;
using GitScc.Utilities;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace GitScc.StatusBar
{
    public sealed class StandardGitStatusBarManager : GitApiStatusBarManager
    {
        private List<string> _branchMenuCommands = new List<string>() {"Create Branch"};
        public StandardGitStatusBarManager(Guid commandSetGuid, int branchMenuCmId, int branchCommandMenuCmId, int repositoryCommandMenuCmId, IServiceContainer serviceProvider, IStatusBarService statusBarService) 
            : base(commandSetGuid, branchMenuCmId, branchCommandMenuCmId, repositoryCommandMenuCmId, serviceProvider, statusBarService)
        {
        }

        protected override async Task UpdateBranchMenu()
        {
            await LoadBranches(CurrentRepository.LocalBranchNames);
            await LoadBranchCommands(_branchMenuCommands);
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

        protected override async Task OnBranchCommandSelection(string command)
        {
            if (string.Equals("Create Branch", command, StringComparison.OrdinalIgnoreCase))
            {
                var branchPicker = new BranchPicker(CurrentRepository);
                var branchInfo = branchPicker.Show();

                var switchResult = await GitCommandWrappers.SwitchCommand(branchInfo);
                if (!switchResult.Succeeded)
                {
                    MessageBox.Show(switchResult.ErrorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
            }


        }
    }
}
