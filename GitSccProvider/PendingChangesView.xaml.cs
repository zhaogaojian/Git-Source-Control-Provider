using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using System.Xml;
using Gitscc;
using GitScc.UI;
using GitSccProvider.UI;
using GitSccProvider.Utilities;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using CancellationToken = System.Threading.CancellationToken;
using IVsTextView = Microsoft.VisualStudio.TextManager.Interop.IVsTextView;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Threading;
using ThreadHelper = Microsoft.VisualStudio.Shell.ThreadHelper;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;
using GitScc.Utilities;

namespace GitScc
{
    public partial class PendingChangesView : UserControl, IDisposable
    {
        private SccProviderService service;
        private GitRepository _tracker;
        private readonly ToolWindowWithEditor<PendingChangesView> _toolWindow;
        private IVsTextView textView;
        private string[] diffLines;
        private bool _diffHightlighted = false;
        private bool _refreshing = false;
        private bool _showOnlySolutionFiles = false;

        private GridViewColumnHeader _currentSortedColumn;
        private ListSortDirection _lastSortDirection;

        public PendingChangesView(ToolWindowWithEditor<PendingChangesView> toolWindow)
        {
            InitializeComponent();
            this._toolWindow = toolWindow;
            this.service = BasicSccProvider.GetServiceEx<SccProviderService>();
            SetDiffCodeHightlighter();
            VSColorTheme.ThemeChanged += VSColorTheme_ThemeChanged;
            CurrentTracker = RepositoryManager.Instance.ActiveTracker;
            RepositoryManager.Instance.ActiveTrackerChanged += Instance_ActiveTrackerChanged;
        }



        private void VSColorTheme_ThemeChanged(ThemeChangedEventArgs e)
        {
            SetDiffCodeHightlighter(true);
        }

        private void SetupTracker(GitRepository tracker)
        {
            if (tracker == _tracker)
            {
                return;
            }
            else
            {
                if (_tracker != null)
                {
                    CurrentTracker.FilesChanged -= CurrentTracker_FilesChanged;
                    CurrentTracker.BranchChanged -= CurrentTracker_BranchChanged;
                    CurrentTracker.FileStatusUpdate -= CurrentTracker_FileStatusUpdate;

                }
                _tracker = tracker;
                if (CurrentTracker != null)
                {
                    CurrentTracker.FilesChanged += CurrentTracker_FilesChanged;
                    CurrentTracker.BranchChanged += CurrentTracker_BranchChanged;
                    CurrentTracker.FileStatusUpdate += CurrentTracker_FileStatusUpdate;
                }
            }
        }





        #region Git Tracker Event Handlers

        private async void CurrentTracker_FileStatusUpdate(object sender, GitFilesStatusUpdateEventArgs e)
        {
            await Refresh(e.Files);
        }

        private async void Instance_ActiveTrackerChanged(object sender, GitRepositoryEvent e)
        {
            CurrentTracker = e.Repository;
            if (CurrentTracker == null)
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                ClearUI();
                _toolWindow.UpdateRepositoryName("");
            }
            else
            {
                await UpdateRepositoryName();
            }

        }

        private async void CurrentTracker_FilesChanged(object sender, GitFilesUpdateEventArgs e)
        {
            await Refresh();
        }

        private async void CurrentTracker_BranchChanged(object sender, string e)
        {
            await UpdateRepositoryName();
        }

        private async Task UpdateRepositoryName()
        {
            await ThreadHelper.JoinableTaskFactory.RunAsync(
                   VsTaskRunContext.UIThreadBackgroundPriority,
                   async delegate
                   {
                       await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                       await Task.Yield();
                       var displayName = " (" + CurrentTracker.CurrentBranchDisplayName + ")";
                       _toolWindow.UpdateRepositoryName(displayName);
                       await Refresh();
                   });
        }

        #endregion

        protected GitRepository CurrentTracker
        {
            get { return _tracker; }
            set
            {
                SetupTracker(value);
            }
        }

        private void SetDiffCodeHightlighter(bool force = false)
        {
            if (!_diffHightlighted || force)
            {
                var defaultBackground = VSColorTheme.GetThemedColor(EnvironmentColors.ToolWindowBackgroundColorKey);
                var defaultForeground = VSColorTheme.GetThemedColor(EnvironmentColors.ToolWindowTextColorKey);

                var theme = ThemeHelper.GetCurrentTheme();
                var filename = "GitScc.Resources.Patch-Mode-Blue.xshd";

                DiffEditor.Background = defaultBackground.ToBrush();
                DiffEditor.Foreground = defaultForeground.ToBrush();

                if (theme == VsTheme.Dark)
                {
                    filename = "GitScc.Resources.Patch-Mode-Dark.xshd";
                }
                var assembly = Assembly.GetExecutingAssembly();

                using (Stream s = assembly.GetManifestResourceStream(filename))
                {
                    using (XmlTextReader reader = new XmlTextReader(s))
                    {
                        DiffEditor.SyntaxHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
                    }
                }
                _diffHightlighted = true;
            }


        }

        #region Events
        private void listView1_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
                e.Handled = true;
        }

        private void listView1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Space)
                return;

            var selectedItem = this.listView1.SelectedItem as GitFile;
            if (selectedItem == null) return;
            var selected = !selectedItem.IsSelected;
            foreach (var item in this.listView1.SelectedItems)
            {
                ((GitFile)item).IsSelected = selected;
            }

            e.Handled = true;
        }

        private void checkBoxSelected_Click(object sender, RoutedEventArgs e)
        {
            var checkBox = sender as CheckBox;
            var listViewItem = FindAncestorOfType<ListViewItem>(checkBox);
            if (listViewItem != null && !listViewItem.IsSelected)
            {
                listView1.SelectedItem = listViewItem.Content;
                return;
            }

            foreach (var item in this.listView1.SelectedItems)
            {
                ((GitFile)item).IsSelected = checkBox.IsChecked == true;
            }

            e.Handled = true;
        }

        private void checkBoxAllStaged_Click(object sender, RoutedEventArgs e)
        {
            var checkBox = sender as CheckBox;
            foreach (var item in this.listView1.Items.Cast<GitFile>())
            {
                item.IsSelected = checkBox?.IsChecked == true;
            }
        }

        private async Task SetEditorText(string text, string otherExtension = "")
        {
            if (string.IsNullOrWhiteSpace(otherExtension))
            {
                SetDiffCodeHightlighter();
            }
            else
            {
                this.DiffEditor.SyntaxHighlighting = HighlightingManager.Instance.GetDefinitionByExtension(
                    otherExtension);
                _diffHightlighted = false;
            }

            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            this.DiffEditor.ShowLineNumbers = true;
            this.DiffEditor.Text = text;
        }

        private async void listView1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            await ThreadHelper.JoinableTaskFactory.RunAsync(
                 VsTaskRunContext.UIThreadBackgroundPriority,
                 async delegate
                 {
                     await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                     await SetDiffEditorText();
                 });
        }

        private async Task SetDiffEditorText()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            var fileName = GetSelectedFileName();
            if (fileName == null)
            {
                this.ClearEditor();
                diffLines = new string[0];
                return;
            }
            if (!_refreshing)
            {
                try
                {

                    await TaskScheduler.Default;
                    var tmpFileName = CurrentTracker.Diff(fileName);
                    await SetEditorText(tmpFileName);

                }
                catch (Exception ex)
                {
                    string message = ex.Message;
                    Action action = () => ShowStatusMessage(message);
                    Dispatcher.Invoke(action);
                }
            };
        }

        private void listView1_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (listView1.SelectedItems.Count != 1)
                return;

            var checkBox = FindAncestorOfType<CheckBox>(e.OriginalSource as DependencyObject);
            if (checkBox != null)
                return;

            GetSelectedFileFullName((fileName) =>
            {
                OpenFile(fileName);
            });
        }

        private T FindAncestorOfType<T>(DependencyObject dependencyObject)
            where T : DependencyObject
        {
            for (var current = dependencyObject; current != null; current = VisualTreeHelper.GetParent(current))
            {
                T typed = current as T;
                if (typed != null)
                    return typed;
            }

            return null;
        }

        private void ClearEditor()
        {
            this._toolWindow.ClearEditor();
            this.DiffEditor.Text = null;
        }


        #endregion

        #region Select File
        private string GetSelectedFileName()
        {
            if (this.listView1.SelectedItems.Count == 0)
                return null;
            var selectedItem = this.listView1.SelectedItems[0] as GitFile;
            if (selectedItem == null) return null;
            return selectedItem.FileName;
        }

        private void GetSelectedFileName(Action<string> action, bool changeToGitPathSeparator = false)
        {
            var fileName = GetSelectedFileName();
            if (fileName == null) return;
            try
            {
                if (changeToGitPathSeparator) fileName.Replace("\\", "/");
                action(fileName);
            }
            catch (Exception ex)
            {
                ShowStatusMessage(ex.Message);
            }
        }

        private void GetSelectedFileFullName(Action<string> action, bool fileMustExists = true)
        {
            List<string> files = new List<string>();
            try
            {
                files = this.listView1.SelectedItems.Cast<GitFile>()
                   .Select(item => System.IO.Path.Combine(this.CurrentTracker.WorkingDirectory, item.FileName))
                   .ToList();

                foreach (var fileName in files)
                {
                    if (fileMustExists && !File.Exists(fileName)) return;
                    action(fileName);
                }
            }
            catch (Exception ex)
            {
                var errorMessage = new StringBuilder();
                errorMessage.Append("Error in File Operation");
                errorMessage.AppendLine(ex.Message);
                errorMessage.AppendLine($"Working Directory: {this.CurrentTracker.WorkingDirectory}");
                if (files?.Count > 0)
                {
                    errorMessage.Append($"File(s):");
                    foreach (var file in files)
                    {
                        errorMessage.AppendLine(file);
                    }
                }
                ShowStatusMessage(errorMessage.ToString());
            }
        }

        #endregion

        #region Git functions


        internal async Task Refresh()
        {
            var files = await Task.Run(async delegate
            {
                return await GetFileList();
            });
            await Refresh(files);
        }

        internal async Task Refresh(List<GitFile> files)
        {
            var updatedFiles = files;
            if (!_refreshing)
            {
                if (_showOnlySolutionFiles)
                {
                    var filteredFiles = new List<GitFile>();
                    foreach (var gitFile in files)
                    {
                        if (service.FileTracked(gitFile.FilePath))
                        {
                            filteredFiles.Add(gitFile);
                        }
                    }
                    updatedFiles = filteredFiles;
                }

                _refreshing = true;
                await ThreadHelper.JoinableTaskFactory.RunAsync(
                    VsTaskRunContext.UIThreadBackgroundPriority,
                    async delegate
                    {
                        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                        await Task.Yield();
                        await RefreshInternal(updatedFiles);
                    });
                _refreshing = false;
            }
        }

        private async Task RefreshInternal(List<GitFile> files)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            label3.Content = "Changed files (Updating...)";

            if (CurrentTracker == null)
            {
                ClearUI();
            }
            else
            {
                ShowStatusMessage("Getting changed files ..."); ;
                await UpdateFileListUI(files);
            }
        }

        private async Task<List<GitFile>> GetFileList()
        {
            if (CurrentTracker != null)
            {
                return await CurrentTracker.GetCurrentChangeSet();
            }
            return new List<GitFile>();
        }

        private async System.Threading.Tasks.Task UpdateFileListUI(List<GitFile> changedFiles)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            var selectedFile = GetSelectedFileName();
            var selectedFiles = this.listView1.Items.Cast<GitFile>()
                .Where(i => i.IsSelected)
                .Select(i => i.FileName).ToList();

            this.listView1.BeginInit();
            try
            {
                this.listView1.ItemsSource = changedFiles;

                SortCurrentColumn();

                this.listView1.SelectedValue = selectedFile;
                selectedFiles.ForEach(fn =>
                {
                    var item = this.listView1.Items.Cast<GitFile>()
                        .Where(i => i.FileName == fn)
                        .FirstOrDefault();
                    if (item != null)
                        item.IsSelected = true;
                });

                ShowStatusMessage("");

                var changed = changedFiles;
                this.label3.Content = string.Format("Changed files (+{0} ~{1} -{2} !{3})",
                    changed.Where(f => f.Status == GitFileStatus.New || f.Status == GitFileStatus.Added).Count(),
                    changed.Where(f => f.Status == GitFileStatus.Modified || f.Status == GitFileStatus.Staged).Count(),
                    changed.Where(f => f.Status == GitFileStatus.Deleted || f.Status == GitFileStatus.Removed).Count(),
                    changed.Where(f => f.Status == GitFileStatus.Conflict).Count());

                if (!changedFiles.Any())
                {
                    this.ClearEditor();
                }
            }
            catch (Exception ex)
            {
                ShowStatusMessage(ex.Message);
            }

            this.listView1.EndInit();
        }



        internal void ClearUI()
        {
            label3.Content = "Changed files";
            this.listView1.ItemsSource = null;
            this.textBoxComments.Document.Blocks.Clear();
            this.ClearEditor();
            var chk = this.listView1.FindVisualChild<CheckBox>("checkBoxAllStaged");
            if (chk != null) chk.IsChecked = false;
            _refreshing = false;
        }

        private string Comments
        {
            get
            {
                TextRange textRange = new TextRange(
                    this.textBoxComments.Document.ContentStart,
                    this.textBoxComments.Document.ContentEnd);
                return textRange.Text;
            }
            set
            {
                TextRange textRange = new TextRange(
                    this.textBoxComments.Document.ContentStart,
                    this.textBoxComments.Document.ContentEnd);
                textRange.Text = value;
            }
        }

        private bool HasComments()
        {
            if (string.IsNullOrWhiteSpace(Comments))
            {
                MessageBox.Show("Please enter comments for the commit.", "Commit",
                    MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return false;
            }
            else
                return true;
        }

        internal async Task Commit()
        {
            StageSelectedFiles();
            try
            {
                ShowStatusMessage("Committing ...");
                var commitResult = await GitCommandWrappers.Commit(CurrentTracker, Comments, chkSignOff.IsChecked == true);
                if (commitResult.Succeeded)
                {
                    ClearUI();
                }
                else
                {
                    MessageBox.Show(commitResult.ErrorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        internal async Task AmendCommit()
        {
            const string amendMsg = @"You are about to amend a commit that has tags or remotes, which could cause issues in local and remote repositories.

Are you sure you want to continue?";

            if (string.IsNullOrWhiteSpace(Comments))
            {
                Comments = CurrentTracker.LastCommitMessage;
                return;
            }
            else
            {
                if (CurrentTracker.CurrentCommitHasRefs() && MessageBox.Show(amendMsg, "Amend Last Commit",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                {
                    return;
                }

                var dte = BasicSccProvider.GetServiceEx<EnvDTE.DTE>();
                if (dte.ItemOperations.PromptToSave == EnvDTE.vsPromptResult.vsPromptResultCancelled) return;

                StageSelectedFiles();

                try
                {
                    ShowStatusMessage("Amending last Commit ...");
                    var commitResult = await GitCommandWrappers.AmendCommit(CurrentTracker, Comments, chkSignOff.IsChecked == true);
                    if (commitResult.Succeeded)
                    {
                        ClearUI();
                        ShowStatusMessage("Amend last commit successfully. Commit Hash: " + commitResult.Item);
                    }
                    else
                    {
                        MessageBox.Show(commitResult.ErrorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    }

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    ShowStatusMessage(ex.Message);
                }
            }
        }

        internal async Task SwitchCommand(SwitchBranchInfo branchInfo)
        {
            var switchResult = await GitCommandWrappers.SwitchCommand(branchInfo);
            if (switchResult.Succeeded)
            {
                await UpdateRepositoryName();
            }
            else
            {
                MessageBox.Show(switchResult.ErrorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        private void StageSelectedFiles()
        {
            var unstaged = this.listView1.Items.Cast<GitFile>()
                               .Where(item => item.IsSelected && !item.IsStaged)
                               .ToList();
            var count = unstaged.Count;
            int i = 0;
            foreach (var item in unstaged)
            {
                CurrentTracker.StageFile(item.FilePath);
                ShowStatusMessage(string.Format("Staged ({0}/{1}): {2}", i++, count, item.FileName));
            }

            CurrentTracker.Refresh();
        }

        private void ShowStatusMessage(string msg)
        {
            var dte = BasicSccProvider.GetServiceEx<EnvDTE.DTE>();
            dte.StatusBar.Text = msg;
        }
        #endregion

        #region Menu Events

        private void listView1_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (this.listView1.SelectedItems.Count == 0)
                return;

            if (this.listView1.SelectedItems.Count == 1)
            {
                var selectedItem = this.listView1.SelectedItems[0] as GitFile;
                if (selectedItem == null) return;

                switch (selectedItem.Status)
                {
                    case GitFileStatus.Added:
                    case GitFileStatus.New:
                        menuCompare.IsEnabled = menuUndo.IsEnabled = false;
                        break;

                    case GitFileStatus.Modified:
                    case GitFileStatus.Staged:
                        menuCompare.IsEnabled = menuUndo.IsEnabled = true;
                        break;

                    case GitFileStatus.Removed:
                    case GitFileStatus.Deleted:
                        menuCompare.IsEnabled = false;
                        menuUndo.IsEnabled = true;
                        break;
                }

                menuStage.Visibility = selectedItem.IsStaged ? Visibility.Collapsed : Visibility.Visible;
                menuUnstage.Visibility = !selectedItem.IsStaged ? Visibility.Collapsed : Visibility.Visible;
                menuDeleteFile.Visibility = (selectedItem.Status == GitFileStatus.New || selectedItem.Status == GitFileStatus.Modified) ?
                    Visibility.Visible : Visibility.Collapsed;
            }
            else
            {
                menuStage.Visibility =
                menuUnstage.Visibility =
                menuDeleteFile.Visibility = Visibility.Visible;
                menuUndo.IsEnabled = true;
                menuIgnore.IsEnabled = false;
                menuCompare.IsEnabled = false;
            }
        }

        private void menuCompare_Click(object sender, RoutedEventArgs e)
        {
            GetSelectedFileFullName(fileName =>
            {
                var service = BasicSccProvider.GetServiceEx<SccProviderService>();
                service.CompareFile(fileName);
            });
        }

        private void menuUndo_Click(object sender, RoutedEventArgs e)
        {
            GetSelectedFileFullName(fileName =>
            {
                var service = BasicSccProvider.GetServiceEx<SccProviderService>();
                service.UndoFileChanges(fileName);
            }, false);        
        }


        private void menuStage_Click(object sender, RoutedEventArgs e)
        {
            GetSelectedFileFullName(fileName =>
            {
                CurrentTracker.StageFile(fileName);
                ShowStatusMessage("Staged file: " + fileName);
            }, false);
        }

        private void menuUnstage_Click(object sender, RoutedEventArgs e)
        {
            GetSelectedFileFullName(fileName =>
            {
                CurrentTracker.UnStageFile(fileName);
                ShowStatusMessage("Un-staged file: " + fileName);
            }, false);
        }

        private void menuDeleteFile_Click(object sender, RoutedEventArgs e)
        {
            const string deleteMsg = @"

Note: if the file is included project, you need to delete the file from project in solution explorer.";

            var filesToDelete = new List<string>();

            GetSelectedFileFullName(fileName => filesToDelete.Add(fileName));

            string title = (filesToDelete.Count == 1) ? "Delete File" : "Delete Files";
            string message = (filesToDelete.Count == 1) ?
                "Are you sure you want to delete file: " + Path.GetFileName(filesToDelete.First()) + deleteMsg :
                String.Format("Are you sure you want to delete {0} selected files", filesToDelete.Count) + deleteMsg;

            if (MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                foreach (var fileName in filesToDelete)
                    File.Delete(fileName);
            }
        }

        #endregion

        #region Ignore files
        private void menuIgnore_Click(object sender, RoutedEventArgs e)
        {

        }

        private void menuIgnoreFile_Click(object sender, RoutedEventArgs e)
        {
            GetSelectedFileName((fileName) =>
            {
                CurrentTracker.AddIgnoreItem(fileName);
            }, true);
        }

        private void menuIgnoreFilePath_Click(object sender, RoutedEventArgs e)
        {
            GetSelectedFileName((fileName) =>
            {
                CurrentTracker.AddIgnoreItem(Path.GetDirectoryName(fileName) + "*/");
            }, true);
        }

        private void menuIgnoreFileExt_Click(object sender, RoutedEventArgs e)
        {
            GetSelectedFileName((fileName) =>
            {
                CurrentTracker.AddIgnoreItem("*" + Path.GetExtension(fileName));
            }, true);
        }
        #endregion

        private void OpenFile(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName)) return;

            var dte = BasicSccProvider.GetServiceEx<EnvDTE.DTE>();
            dte.ExecuteCommand("File.OpenFile", $"\"{fileName}\"");
        }

        private async void UserControl_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && Keyboard.Modifiers == ModifierKeys.Control)
            {
                await Commit();
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            GridViewColumnCollection columns = ((GridView)listView1.View).Columns;
            _currentSortedColumn = (GridViewColumnHeader)columns[columns.Count - 1].Header;
            _lastSortDirection = ListSortDirection.Ascending;
            UpdateColumnHeaderTemplate(_currentSortedColumn, _lastSortDirection);
        }

        private void listView1_Click(object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader header = e.OriginalSource as GridViewColumnHeader;
            if (header == null || header.Role == GridViewColumnHeaderRole.Padding)
                return;

            ListSortDirection direction = ListSortDirection.Ascending;
            if (header == _currentSortedColumn && _lastSortDirection == ListSortDirection.Ascending)
                direction = ListSortDirection.Descending;

            Sort(header, direction);
            UpdateColumnHeaderTemplate(header, direction);
            _currentSortedColumn = header;
            _lastSortDirection = direction;
        }

        private void SortCurrentColumn()
        {
            if (_currentSortedColumn != null)
                Sort(_currentSortedColumn, _lastSortDirection);
        }

        private void Sort(GridViewColumnHeader header, ListSortDirection direction)
        {
            if (listView1.ItemsSource != null)
            {
                ICollectionView view = CollectionViewSource.GetDefaultView(listView1.ItemsSource);
                view.SortDescriptions.Clear();
                view.SortDescriptions.Add(new SortDescription(header.Tag as string, direction));
                view.Refresh();
            }
        }

        private void UpdateColumnHeaderTemplate(GridViewColumnHeader header, ListSortDirection direction)
        {
            GridViewColumn checkStateColumn = ((GridView)listView1.View).Columns[0];
            if (header.Column != checkStateColumn)
            {
                if (direction == ListSortDirection.Ascending)
                    header.Column.HeaderTemplate = Resources["HeaderTemplateArrowUp"] as DataTemplate;
                else
                    header.Column.HeaderTemplate = Resources["HeaderTemplateArrowDown"] as DataTemplate;
            }

            if (_currentSortedColumn != null && _currentSortedColumn != header && _currentSortedColumn.Column != checkStateColumn)
                _currentSortedColumn.Column.HeaderTemplate = null;
        }

        internal void OnSettings()
        {
            Settings.Show();
        }

        public void Dispose()
        {
            VSColorTheme.ThemeChanged -= VSColorTheme_ThemeChanged;
        }

        private async void _cbShowOnlySolutionFiles_Checked(object sender, RoutedEventArgs e)
        {
            _showOnlySolutionFiles = true;
            await Refresh();
        }

        private async void _cbShowOnlySolutionFiles_Unchecked(object sender, RoutedEventArgs e)
        {
            _showOnlySolutionFiles = false;
            await Refresh();
        }

        private async void DiffEditor_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            await HandleDiffOpenFile();
        }

        private async Task HandleDiffOpenFile()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            int start = 1, column = 1;
            try
            {
                if (!string.IsNullOrWhiteSpace(DiffEditor.Text))
                {
                    var doc = DiffEditor.Document;
                    var line = DiffEditor.TextArea.Caret.Line - 1;



                    while (line >= 0)
                    {
                        var text = doc.GetText(doc.Lines[line].Offset, doc.Lines[line].Length);
                        var match = Regex.Match(text, "^@@(.+)@@");
                        if (match.Success)
                        {
                            var s = match.Groups[1].Value;
                            s = s.Substring(s.IndexOf('+') + 1);
                            s = s.Substring(0, s.IndexOf(','));
                            start += Convert.ToInt32(s) - 2;
                            break;
                        }
                        else if (text.StartsWith("-"))
                        {
                            start--;
                        }
                        start++;
                        line--;
                    }
                }
            }
            catch (Exception ex)
            {
                ShowStatusMessage(ex.Message);
                Log.WriteLine("Pending Changes View - DiffEditor_MouseDoubleClick: {0}", ex.ToString());
            }

            GetSelectedFileFullName((fileName) =>
            {
                OpenFile(fileName);
                var dte = BasicSccProvider.GetServiceEx<EnvDTE.DTE>();
                var selection = dte.ActiveDocument.Selection as EnvDTE.TextSelection;
                selection.MoveToLineAndOffset(start, column);
            });
        }
    }

    public static class ExtHelper
    {
        public static TChild FindVisualChild<TChild>(this DependencyObject obj, string name = null) where TChild : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child != null && child is TChild && (name == null || ((Control)child).Name == name))
                {
                    return (TChild)child;
                }
                else
                {
                    TChild childOfChild = FindVisualChild<TChild>(child, name);
                    if (childOfChild != null)
                        return childOfChild;
                }
            }
            return null;
        }
    }
}
