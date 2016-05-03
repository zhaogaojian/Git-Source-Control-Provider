using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace GitScc.StatusBar
{
    public class StandardGitStatusBarManager : GitApiStatusBarManager
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
            System.Windows.Forms.MessageBox.Show(string.Format(CultureInfo.CurrentCulture,
        "Selected {0}", command));
        }
    }
}
