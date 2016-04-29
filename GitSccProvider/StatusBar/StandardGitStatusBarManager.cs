using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace GitScc.StatusBar
{
    public class StandardGitStatusBarManager : GitStatusBarManager
    {

        public StandardGitStatusBarManager(Guid commandSetGuid, int branchMenuCmId, int branchCommandMenuCmId, IServiceContainer serviceProvider, IStatusBarService statusBarService) 
            : base(commandSetGuid, branchMenuCmId, branchCommandMenuCmId, serviceProvider, statusBarService)
        {
        }

        protected override async Task UpdateBranchMenu()
        {
            await LoadBranches(CurrentRepository.LocalBranchNames);
        }

        
    }
}
