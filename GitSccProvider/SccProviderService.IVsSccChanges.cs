using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

namespace GitScc
{
    public partial class SccProviderService : IVsSccChanges
    {
        private int _pendingChangeCount;
        private string _pendingChangeDetail;
        private string _pendingChangeLabel;

        public int PendingChangeCount
        {
            get { return _pendingChangeCount; }
            set
            {
                if (_pendingChangeCount != value)
                {
                    _pendingChangeCount = value;

                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PendingChangeCount)));
                }
            }
        }

        public string PendingChangeDetail
        {
            get { return _pendingChangeDetail; }
            set
            {
                if (_pendingChangeDetail != value)
                {
                    _pendingChangeDetail = value;

                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PendingChangeDetail)));
                }
            }
        }
        public string PendingChangeLabel
        {
            get { return _pendingChangeLabel; }
            set
            {
                if (_pendingChangeLabel != value)
                {
                    _pendingChangeLabel = value;

                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PendingChangeLabel)));
                }
            }
        }

        public async Task PendingChangesUIClickedAsync(ISccUIClickedEventArgs args, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            Debug.Assert(args != null, "Changes UI coordinates were not received.");

            await _sccProvider.ShowPendingChangesWindow();

            //IVsUIShell uiShell = (IVsUIShell)_sccProvider.GetService(typeof(SVsUIShell));
            //if (uiShell != null)
            //{
            //    int result;
            //    uiShell.ShowMessageBox(dwCompRole: 0,
            //                           rclsidComp: Guid.Empty,
            //                           pszTitle: Resources.ProviderName,
            //                           pszText: string.Format(CultureInfo.CurrentUICulture, "Blah", args.ClickedElementPosition.ToString()),
            //                           pszHelpFile: string.Empty,
            //                           dwHelpContextID: 0,
            //                           msgbtn: OLEMSGBUTTON.OLEMSGBUTTON_OK,
            //                           msgdefbtn: OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST,
            //                           msgicon: OLEMSGICON.OLEMSGICON_INFO,
            //                           fSysAlert: 0,        // false = application modal; true would make it system modal
            //                           pnResult: out result);
            //}
        }
    }
}
