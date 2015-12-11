using System;
using System.ComponentModel.Design;
using System.Runtime.InteropServices;
using Gitscc;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio;
using GitSccProvider;

namespace GitScc
{
    /// <summary>
    /// Summary description for SccProviderToolWindow.
    /// </summary>
    [Guid("75EDECF4-68D8-4B7B-92A9-5915461DA6D9")]
    public class PendingChangesToolWindow : ToolWindowWithEditor<PendingChangesView>
    {
        private SccProviderService sccProviderService;

        public PendingChangesToolWindow()
        {
            // set the window title
            this.Caption = Resources.ResourceManager.GetString("PendingChangesToolWindowCaption");

            //// set the CommandID for the window ToolBar
            base.ToolBar = new CommandID(GuidList.guidSccProviderCmdSet, CommandId.imnuPendingChangesToolWindowToolbarMenu);

            // set the icon for the frame
            this.BitmapResourceID = CommandId.ibmpToolWindowsImages;  // bitmap strip resource ID
            this.BitmapIndex = CommandId.iconSccProviderToolWindow;   // index in the bitmap strip
        }

        protected override void Initialize()
        {
            base.Initialize();
            control = new PendingChangesView(this);

            // This is the user control hosted by the tool window; Note that, even if this class implements IDisposable,
            // we are not calling Dispose on this object. This is because ToolWindowPane calls Dispose on 
            // the object returned by the Content property.
            base.Content = control;

            OleMenuCommandService mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;

            //var cmd = new CommandID(GuidList.guidSccProviderCmdSet, CommandId.icmdPendingChangesCommit);
            //var menu = new MenuCommand(new EventHandler(OnCommitCommand), cmd);
            //mcs.AddCommand(menu);

            //cmd = new CommandID(GuidList.guidSccProviderCmdSet, CommandId.icmdPendingChangesAmend);
            //menu = new MenuCommand(new EventHandler(OnAmendCommitCommand), cmd);
            //mcs.AddCommand(menu);

            var cmd = new CommandID(GuidList.guidSccProviderCmdSet, CommandId.icmdPendingChangesRefresh);
            var menu = new MenuCommand(new EventHandler(OnRefreshCommand), cmd);
            mcs.AddCommand(menu);


            //CommandID menuMyDynamicComboCommandID = new CommandID(GuidList.guidComboBoxCmdSet,(int)PkgCmdIDList.cmdidMyDynamicCombo);
            //OleMenuCommand menuMyDynamicComboCommand = new OleMenuCommand(new EventHandler(OnMenuMyDynamicCombo),menuMyDynamicComboCommandID);
            //menuMyDynamicComboCommand.ParametersDescription = "$";
            //mcs.AddCommand(menuMyDynamicComboCommand);


            mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            CommandID commandId = new CommandID(GuidList.guidSccProviderCmdSet, (int)CommandId.icmdGitIgnoreCommand1);
            OleMenuCommand menuMyDynamicComboCommand = new OleMenuCommand(new EventHandler(OnRefreshCommand), commandId);
            mcs.AddCommand(menuMyDynamicComboCommand);


            //cmd = new CommandID(GuidList.guidSccProviderCmdSet, CommandId.icmdSccCommandEditIgnore);
            //menu = new MenuCommand(new EventHandler(OnRefreshCommand), cmd);
            //mcs.AddCommand(menu);

            //sccProviderService = BasicSccProvider.GetServiceEx<SccProviderService>();
            //var test = sccProviderService.CurrentTracker;
            //Refresh(sccProviderService.CurrentTracker); // refresh when the tool window becomes visible

        }

        #region Overrides of WindowPane

        protected override void Dispose(bool disposing)
        {
            control?.Dispose();
            base.Dispose(disposing);
        }

        #endregion

        public override void OnToolWindowCreated()
        {
            sccProviderService = BasicSccProvider.GetServiceEx<SccProviderService>();
            Refresh(sccProviderService.CurrentTracker); // refresh when the tool window becomes visible
        }

        internal bool hasFileSaved()
        {
            var dte = BasicSccProvider.GetServiceEx<EnvDTE.DTE>();
            return dte.ItemOperations.PromptToSave != EnvDTE.vsPromptResult.vsPromptResultCancelled;
        }

        internal void OnCommitCommand()
        {
            if (!hasFileSaved()) return;
            control.Commit();
        }

        internal void OnAmendCommitCommand()
        {
            if (!hasFileSaved()) return;
            control.AmendCommit();
        }

        private void OnRefreshCommand(object sender, EventArgs e)
        {
            hasFileSaved(); //just a reminder, refresh anyway
            sccProviderService.Refresh();
        }

        internal void Refresh(GitFileStatusTracker tracker)
        {
            try
            {
                var repository = (tracker == null || !tracker.IsGit) ? "" :
                    string.Format(" ({0})", tracker.CurrentBranch, tracker.WorkingDirectory);

                this.Caption = Resources.ResourceManager.GetString("PendingChangesToolWindowCaption") + repository;

                control.Refresh(tracker);
                if (GitSccOptions.Current.DisableAutoRefresh)
                {
                    this.Caption += " - [AUTO REFRESH DISABLED]";
                }
            }
            catch (Exception ex)
            {
                Log.WriteLine("Pending Changes Tool Window Refresh: {0}", ex.ToString());
            }
        }

        internal void OnSettings()
        {
            control.OnSettings();
        }
    }
}
