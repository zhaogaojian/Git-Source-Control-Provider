using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Imaging.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

namespace GitScc
{
    public partial class SccProviderService :
        IVsSccCurrentBranch
    {
        private string _branchName;
        private string _branchDetail;
        private ImageMoniker _branchIcon;



        public string BranchName
        {
            get { return _branchName; }
            set
            {
                if (_branchName != value)
                {
                    _branchName = value;

                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(BranchName)));
                }
            }
        }

        /// <summary>
        /// Details about the currently active branch
        /// </summary>
        public string BranchDetail
        {
            get { return _branchDetail; }
            set
            {
                if (_branchDetail != value)
                {
                    _branchDetail = value;

                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(BranchDetail)));
                }
            }
        }

        /// <summary>
        /// Branch Icon
        /// </summary>
        public ImageMoniker BranchIcon
        {
            get { return _branchIcon; }
            set
            {
                if (!_branchIcon.Equals(value))
                {
                    _branchIcon = value;

                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(BranchIcon)));
                }
            }
        }


        public async Task BranchUIClickedAsync(ISccUIClickedEventArgs args, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            Debug.Assert(args != null, "Branch UI coordinates were not received.");

            IVsUIShell uiShell = (IVsUIShell)_sccProvider.GetService(typeof(SVsUIShell));
            if (uiShell != null)
            {
                POINTS[] p = new POINTS[1];
                p[0] = new POINTS();
                p[0].x = (short)args.ClickedElementPosition.TopRight.X;
                p[0].y = (short)args.ClickedElementPosition.TopRight.Y;

                Guid commandSet = GuidList.guidSccProviderCmdSet;
                uiShell.ShowContextMenu(0, ref commandSet, PackageIds.GitIgnoreSubMenu, p, null);
            }
        }

    }
}
