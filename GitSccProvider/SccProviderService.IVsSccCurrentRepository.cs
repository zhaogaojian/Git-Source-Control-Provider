using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.VisualStudio.Imaging.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

using Task = System.Threading.Tasks.Task;

namespace GitScc
{
   public partial class SccProviderService :
          IVsSccCurrentRepository
    {
        private string _repositoryName;
        private string _repositoryDetail;

        private ImageMoniker _repositoryIcon;

        #region Implementation of INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Implementation of IVsSccCurrentRepositoryDisplayInformation

        public string RepositoryName
        {
            get { return _repositoryName; }
            set
            {
                if (_repositoryName != value)
                {
                    _repositoryName = value;

                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RepositoryName)));
                }
            }
        }

       

        /// <summary>
        /// Details about the currently active Repository
        /// </summary>
        public string RepositoryDetail
        {
            get { return _repositoryDetail; }
            set
            {
                if (_repositoryDetail != value)
                {
                    _repositoryDetail = value;

                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RepositoryDetail)));
                }
            }
        }

        /// <summary>
        /// Repository Icon
        /// </summary>
        public ImageMoniker RepositoryIcon
        {
            get { return _repositoryIcon; }
            set
            {
                if (!_repositoryIcon.Equals(value))
                {
                    _repositoryIcon = value;

                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RepositoryIcon)));
                }
            }
        }

        public async Task RepositoryUIClickedAsync(ISccUIClickedEventArgs args, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            Debug.Assert(args != null, "Repository UI coordinates were not received.");

            IVsUIShell uiShell = (IVsUIShell)_sccProvider.GetService(typeof(SVsUIShell));
            if (uiShell != null)
            {
                POINTS[] p = new POINTS[1];
                p[0] = new POINTS();
                p[0].x = (short)args.ClickedElementPosition.TopRight.X;
                p[0].y = (short)args.ClickedElementPosition.TopRight.Y;

                Guid commandSet = GuidList.guidSccProviderCmdSet;
                uiShell.ShowContextMenu(0, ref commandSet, PackageIds.RepositoryMenu, p, _statusBarManager);
            }
            //if (uiShell != null)
            //{
            //    int result;
            //    uiShell.ShowMessageBox(dwCompRole: 0,
            //                           rclsidComp: Guid.Empty,
            //                           pszTitle: Resources.ProviderName,
            //                           pszText: string.Format(CultureInfo.CurrentUICulture, "Clicked", args.ClickedElementPosition.ToString()),
            //                           pszHelpFile: string.Empty,
            //                           dwHelpContextID: 0,
            //                           msgbtn: OLEMSGBUTTON.OLEMSGBUTTON_OK,
            //                           msgdefbtn: OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST,
            //                           msgicon: OLEMSGICON.OLEMSGICON_INFO,
            //                           fSysAlert: 0,        // false = application modal; true would make it system modal
            //                           pnResult: out result);
            //}
        }

        #endregion
    }
}
