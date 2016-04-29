using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;

namespace GitScc.StatusBar
{
    public interface IStatusBarService : IVsSccCurrentBranch, IVsSccChanges
    {
        new string BranchName { get; set; }

        new int PendingChangeCount { get; set; }
    }
}
