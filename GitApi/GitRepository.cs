using GitScc.DataServices;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reflection;
using System.Runtime.Caching;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GitScc.Extensions;
using LibGit2Sharp;
using Nito.AsyncEx;
using static GitScc.GitFile;
using Commit = GitScc.DataServices.Commit;


namespace GitScc
{

    public delegate void GitFilesUpdateEventHandler(object sender, GitFilesUpdateEventArgs e);
    public delegate void GitFilesStatusUpdateEventHandler(object sender, GitFilesStatusUpdateEventArgs e);


    public class GitRepository : IDisposable
    {

        private const string RFC2822Format = "ddd dd MMM HH:mm:ss yyyy K";

        private string workingDirectory;
        private readonly string _gitDirectory;
        private Dictionary<string,GitFile> _changedFiles;
        private bool isGit;
        private string _branchDisplayName;
        //private string _cachedBranchName;
        //private string _cachedHeadSha;
        //private CurrentOperation _cachedBranchOperation;
        private readonly string _repositoryPath;
        private readonly string _objectPath;
        private IEnumerable<string> remotes;
        private IDictionary<string, string> configs;
        FileSystemWatcher _watcher;
        private MemoryCache _fileCache;
        private List<GitBranchInfo> _branchInfoList;

        private GitHeadState _savedState;

        private event GitFilesUpdateEventHandler _onFilesUpdateEventHandler;

        private event GitFilesStatusUpdateEventHandler _onFilesStatusUpdateEventHandler;
        private event EventHandler<string> _onBranchChanged;
        private event EventHandler<string> _onCommitChanged;
        private event EventHandler _gitfileEvent;

        private event EventHandler _fileEvent;


        private readonly IObservable<EventPattern<object>> _gitEventObservable;
        private readonly IObservable<EventPattern<object>> _fileChangedEventObservable;

        private readonly AsyncLock _statusMutex = new AsyncLock();
        private readonly AsyncLock _gitStatusMutex = new AsyncLock();

        public event GitFilesUpdateEventHandler FilesChanged
        {
            add { _onFilesUpdateEventHandler += value; }
            remove { _onFilesUpdateEventHandler -= value; }
        }

        public event GitFilesStatusUpdateEventHandler FileStatusUpdate
        {
            add { _onFilesStatusUpdateEventHandler += value; }
            remove { _onFilesStatusUpdateEventHandler -= value; }
        }

        public event EventHandler<string> BranchChanged
        {
            add { _onBranchChanged += value; }
            remove { _onBranchChanged -= value; }
        }

        public event EventHandler<string> OnCommitChanged
        {
            add { _onCommitChanged += value; }
            remove { _onCommitChanged -= value; }
        }

        private Repository _statusRepository;


        public string WorkingDirectory
        {
            get { return workingDirectory; }
        }

        public string Name => new DirectoryInfo(WorkingDirectory).Name;

        public bool IsGit
        {
            get { return Repository.IsValid(workingDirectory); }
        }

        public GitRepository(string directory)
        {
            _gitDirectory = Repository.Discover(directory);
            _savedState = new GitHeadState();
            _statusRepository = GetRepository();
            this.workingDirectory = _statusRepository.Info.WorkingDirectory;
            _repositoryPath = _statusRepository.Info.Path;
            _objectPath = _repositoryPath + "objects\\";

            //_cachedBranchOperation = CurrentOperation.None;
            Refresh();
            _gitEventObservable = Observable.FromEventPattern(ev => _gitfileEvent += ev, ev => _gitfileEvent -= ev)
                .Throttle(TimeSpan.FromMilliseconds(2000));
            _gitEventObservable.Subscribe(x => Task.Run(async () => await DecodeGitEvents()));

            _fileChangedEventObservable = Observable.FromEventPattern(ev => _fileEvent += ev, ev => _fileEvent -= ev)
    .Throttle(TimeSpan.FromMilliseconds(350));
            _fileChangedEventObservable.Subscribe(x => Task.Run(async () => await FileChangedEvent()));

        }


        private Repository GetRepository()
        {
            return new Repository(_gitDirectory);
        }


        public void EnableRepositoryWatcher()
        {
            _watcher = new FileSystemWatcher(workingDirectory);
            _watcher.NotifyFilter =
                NotifyFilters.FileName
                | NotifyFilters.Attributes
                | NotifyFilters.LastWrite
                | NotifyFilters.Size
                | NotifyFilters.CreationTime
                | NotifyFilters.DirectoryName;

            _watcher.IncludeSubdirectories = true;
            _watcher.Changed += HandleFileSystemChanged;
            _watcher.Created += HandleFileSystemChanged;
            _watcher.Deleted += HandleFileSystemChanged;
            _watcher.Renamed += HandleFileSystemChanged;
            _watcher.EnableRaisingEvents = true;
        }

        private async void HandleFileSystemChanged(object sender, FileSystemEventArgs e)
        {
            try
            {
                await Task.Run(() => CreateGitFileEvent(e));
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error In File System Changed Event: " + ex.Message);
            }
        }

        private async Task CreateGitFileEvent(FileSystemEventArgs e)
        {
            var fullPath = e.FullPath;
            var filename = e.Name;
            if (FileIgnored(fullPath))
            {
                return;
            }
            if (fullPath.IsSubPathOf(_repositoryPath))
            {
                _gitfileEvent?.Invoke(this,new EventArgs());
            }
            else
            {
                using (var repository = GetRepository())
                {
                    if (repository.Ignore.IsPathIgnored(fullPath.Remove(0, WorkingDirectory.Length)))
                    {
                        return;
                    }
                    else
                    {
                        //queue the event for later. 
                        _fileEvent?.Invoke(this, new EventArgs());
                    }
                }

            }
        }

        private bool FileIgnored(string filepath)
        {
            //maybe don't worry about node_modules
            if (filepath.ToLower().Contains("node_modules"))
            {
                return true;
            }
            var extension = Path.GetExtension(filepath)?.ToLower();

            if (extension != null && (string.Equals(extension, ".suo") || extension.EndsWith("~")))
            {
                return true;
            }

            if (string.Equals(extension, ".tmp"))
            {
                if (filepath.Contains("~RF") || filepath.Contains("\\ve-"))
                {
                    return true;
                }
            }

            //Ignore directory changes that we don't care about 
            if (filepath.ArePathsEqual(_repositoryPath) || filepath.ArePathsEqual(_objectPath))
            {
                //Do nothing here.. 
                return true;
            }

            //ignore all files inside of git directory we don't want to trigger an event
            if (filepath.Contains(_repositoryPath))
            {
                if (filepath.Contains("tmp_object_git2") || filepath.Contains("streamed_git2"))
                {
                    return true;
                }
            }

            return false;
        }

        private async Task DecodeGitEvents()
        {
            await GitFileEventUpdate();
        }

        private async Task FileChangedEvent()
        {
            await HandleFileSystemChanged();
        }

        /// <summary>
        /// This is not very dry.. but It seems to cause a bunch of issues opening repos so fast. 
        /// </summary>
        private async Task GitFileEventUpdate()
        {
            using (await _gitStatusMutex.LockAsync())
            {
                Debug.WriteLine("Git File Event Update");
                bool supressBranchEvent = false;
                var files = new Dictionary<string, GitFile>();
                Repository repository = null;
                try
                {
                    //repository = _statusRepository;
                    repository = GetRepository();
                    files = GetCurrentChangedFiles();

                    //logic getting complicated time to break it out
                    if (_savedState.Operation != repository.Info.CurrentOperation)
                    {
                        _savedState.Operation = repository.Info.CurrentOperation;
                        _savedState.Sha = null;
                        _savedState.BranchName = null;
                    }

                    if (string.IsNullOrWhiteSpace(_savedState.Sha) ||
                        !string.Equals(_savedState.Sha, repository.Head.Tip.Sha))
                    {
                        _savedState.Sha = repository.Head.Tip.Sha;
                        FireCommitShaChangedEvent(_savedState.Sha);
                    }

                    if (string.IsNullOrWhiteSpace(_savedState.BranchName) ||
                        !string.Equals(_savedState.BranchName, repository.Head.FriendlyName))
                    {
                        var newBranchName = string.IsNullOrWhiteSpace(repository.Head.FriendlyName)
                            ? "master"
                            : repository.Head.FriendlyName;
                        if (string.Equals(_savedState.BranchName, newBranchName))
                        {
                            supressBranchEvent = true;
                        }
                        else
                        {
                            _savedState.BranchName = newBranchName;
                            _branchDisplayName = null;
                        }

                        if (!supressBranchEvent)
                        {
                            FireBranchChangedEvent(_savedState.BranchName);
                        }

                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Error In GetCurrentChangedFiles: " + ex.Message);
                    Thread.Sleep(2000);
                }
                finally
                {
                    repository?.Dispose();
                }
                 _changedFiles = files;
                FireFileStatusUpdateEvent(files.Values.ToList());
            }
        }


        private async Task HandleFileSystemChanged()
        {
            using (await _statusMutex.LockAsync())
            {

                var changeFileStatus =  GetCurrentChangedFiles();
                //}

                if (changeFileStatus != null)
                {
                    _changedFiles = changeFileStatus;
                    FireFileStatusUpdateEvent(changeFileStatus.Values.ToList());
                }
            }

        }

        /// <summary>
        /// Update status when some file change is detected in the .git dir
        /// </summary>
        //private void HandleGitFileSystemChange()
        //{
        //    //lock (_repoUpdateLock)
        //    //{
        //        var files = CreateRepositoryUpdateChangeSet();
        //        SetBranchName();
        //        _branchInfoList = null;
        //        if (files != null && files.Count > 0)
        //        {
        //            FireFilesChangedEvent(files);
        //        }
        //    //}
        //}

        public void DisableRepositoryWatcher()
        {
            _watcher.EnableRaisingEvents = false;
            _watcher.Dispose();
        }

        //TODO.. I think I should bea abel to make this private soon
        public void Refresh()
        {
            this.repositoryGraph = null;
            this._changedFiles = null;
            this._branchDisplayName = null;
            this.remotes = null;
            this.configs = null;
            _branchInfoList = null;
            SetBranchName();
            LoadHeadState();
        }

        private void LoadHeadState()
        {
            using (var repository = GetRepository())
            {
                _savedState.Sha = repository.Head?.Tip?.Sha;
                _savedState.BranchName = repository.Head?.FriendlyName;
                _savedState.Operation = repository.Info.CurrentOperation;
            } 
        }
        #region Checkout Functions

        public async Task<GitActionResult<GitBranchInfo>> CheckoutAsync(GitBranchInfo info, bool force = false)
        {
            return await Task.Run(() => Checkout(info, force));
        }


        public GitActionResult<GitBranchInfo> Checkout(GitBranchInfo info, bool force = false)
        {
            using (var repository = GetRepository())
            {
                var result = new GitActionResult<GitBranchInfo>();

                CheckoutOptions options = new CheckoutOptions();
                var branch = repository.Branches.FirstOrDefault(
                        x => string.Equals(x.CanonicalName, info.CanonicalName, StringComparison.OrdinalIgnoreCase));

                if (force)
                {
                    options.CheckoutModifiers = CheckoutModifiers.Force;

                }
                try
                {
                    var checkoutBranch = Commands.Checkout(repository, branch,options);
                    if (checkoutBranch != null)
                    {
                        result.Item = new GitBranchInfo
                        {
                            CanonicalName = checkoutBranch.CanonicalName,
                            RemoteName = checkoutBranch.RemoteName,
                            Name = checkoutBranch.FriendlyName,
                            IsRemote = checkoutBranch.IsRemote
                        };
                        result.Succeeded = true;
                        return result;
                    }
                    result.Succeeded = false;
                }
                catch (CheckoutConflictException conflict)
                {
                    result.Succeeded = false;
                    result.ErrorMessage = conflict.Message;
                }

                return result;
            }
        }

        public void UndoFileChanges(string filename)
        {
            using (var repository = GetRepository())
            {
                CheckoutOptions options = new CheckoutOptions { CheckoutModifiers = CheckoutModifiers.Force };
                repository.CheckoutPaths("HEAD", new string[] { filename }, options);
            }
        }


        public void AddFile(string filename)
        {
            try
            {
                string relPath;
                if (Path.IsPathRooted(filename))
                {
                    if (!TryGetRelativePath(filename,out relPath))
                    {
                        return;
                    }
                }
                else
                {
                    relPath = filename;
                }
                using (var repository = GetRepository())
                {
                    repository.Index.Add(relPath);
                }
            }
            catch (Exception)
            {
            }

        }

        public void StageFile(string fileName)
        {
            using (var repository = GetRepository())
            {
                Commands.Stage(repository, fileName);
            }

        }

        public void UnStageFile(string fileName)
        {
            using (var repository = GetRepository())
            {
                Commands.Unstage(repository, fileName);
            }
        }



        #endregion

        #region Commit Functions


        public GitActionResult<string> Commit(string message, bool amend = false, bool signoff = false)
        {
            var result = new GitActionResult<string>();
            using (var repository = GetRepository())
            {
                if (string.IsNullOrEmpty(message))
                {
                    result.Succeeded = false;
                    result.ErrorMessage = "Commit message must not be null or empty!";
                    //throw new ArgumentException("Commit message must not be null or empty!", "message");
                }
                else
                {
                    Signature author = repository.Config.BuildSignature(DateTimeOffset.Now);
                    Signature committer = author;

                    CommitOptions opts = new CommitOptions();
                    opts.AmendPreviousCommit = amend;
                    var commit = repository.Commit(message, author, committer, opts);
                    result.Succeeded = true;
                    result.Item = commit.Sha;
                }
                return result;
            }
        }

        public bool CurrentCommitHasRefs()
        {
            var head = GetBranchId("HEAD");
            if (head == null) return false;
            var result = GitBash.Run("show-ref --head --dereference", WorkingDirectory);
            if (!result.HasError && !result.Output.StartsWith("fatal:"))
            {
                var refs = result.Output.Split('\n')
                          .Where(t => t.IndexOf(head) >= 0);
                return refs.Count() > 2;
            }
            return false;
        }

        internal List<Commit> GetLatestCommits(int commitCount)
        {
            try
            {
                using (var repository = GetRepository())
                {
                    var filter = new CommitFilter { SortBy = CommitSortStrategies.Time };
                    return repository.Commits.QueryBy(filter)
                        .Take(commitCount)
                        .Select(commit => CreateCommit(commit)).ToList();
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        internal Commit GetCommitById(string sha)
        {
            try
            {
                using (var repository = GetRepository())
                {
                    var commit = repository.Lookup<LibGit2Sharp.Commit>(sha);
                    return CreateCommit(commit);
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        private Commit CreateCommit(LibGit2Sharp.Commit commit)
        {
            return new Commit
            {
                Id = commit.Sha,
                ParentIds = commit.Parents.Select(parent => parent.Sha).ToList(),
                AuthorDateRelative = commit.Author.When.ToString(),
                AuthorName = commit.Author.Name,
                AuthorEmail = commit.Author.Email,
                AuthorDate = commit.Author.When.DateTime,
                Subject = commit.MessageShort,
                Message = commit.Message
            };
        }

        #endregion

        #region Branch Functions


        private Branch GetLib2GitBranch(GitBranchInfo info)
        {
            using (var repository = GetRepository())
            {
                return
                    repository.Branches.FirstOrDefault(
                        x => string.Equals(x.CanonicalName, info.CanonicalName, StringComparison.OrdinalIgnoreCase));
            }
        }


        public void SetRemoteBranch(GitBranchInfo localBranch, string remoteName = "origin")
        {
            using (var repository = GetRepository())
            {
                Remote remote = repository.Network.Remotes[remoteName];
                var branch = GetLib2GitBranch(localBranch);
                repository.Branches.Update(branch,
                    b => b.Remote = remote.Name,
                    b => b.UpstreamBranch = localBranch.CanonicalName);
            }
        }

        public GitActionResult<GitBranchInfo> CreateBranch(string branchName, string commitish = "HEAD")
        {
            var result = new GitActionResult<GitBranchInfo>();
            try
            {
                using (var repository = GetRepository())
                {
                    var branch = repository.CreateBranch(branchName, commitish);
                    if (branch != null)
                    {
                        result.Item = new GitBranchInfo
                        {
                            CanonicalName = branch.CanonicalName,
                            RemoteName = branch.RemoteName,
                            Name = branch.FriendlyName,
                            IsRemote = branch.IsRemote
                        };
                        result.Succeeded = true;
                    }
                    else
                    {
                        result.Succeeded = false;
                    }
                }
            }
            catch (Exception ex)
            {
                result.ErrorMessage = ex.Message;
                result.Succeeded = false;
            }
            return result;
        }

        public GitBranchInfo CurrentBranchInfo
        {
            get
            {
                using (var repository = GetRepository())
                {
                    var branch = repository.Head;
                    return new GitBranchInfo
                    {
                        CanonicalName = branch.CanonicalName,
                        RemoteName = branch.RemoteName,
                        Name = branch.FriendlyName,
                        IsRemote = branch.IsRemote,
                        Sha = branch.Tip?.Sha,
                        IsCurrentRepoHead = true
                    };
                }
            }
        }

        public List<string> LocalBranchNames
        {
            get
            {
                using (var repository = GetRepository())
                {
                    var names = new List<string>();

                    foreach (Branch b in repository.Branches.Where(b => !b.IsRemote))
                    {
                        names.Add(b.FriendlyName);
                    }

                    return names;
                }
            }
        }

        public List<string> RemoteBranchNames
        {
            get
            {
                using (var repository = GetRepository())
                {
                    var names = new List<string>();

                    foreach (Branch b in repository.Branches.Where(b => b.IsRemote))
                    {
                        names.Add(b.FriendlyName);
                    }
                    return names;
                }
            }
        }

        public List<GitBranchInfo> GetBranchInfo(bool includeRemote = true, bool includeLocal = true, bool forceReload = false)
        {
            using (var repository = GetRepository())
            {
                if (forceReload || _branchInfoList == null || _branchInfoList.Count <= 0)
                {
                    _branchInfoList = new List<GitBranchInfo>();

                    if (includeRemote && includeLocal)
                    {
                        _branchInfoList.AddRange(
                            repository.Branches.Select(CreateBranchInfoFromBranch));
                    }
                    else if (includeRemote)
                    {
                        _branchInfoList.AddRange(
                            repository.Branches.Where(b => b.IsRemote)
                               .Select(CreateBranchInfoFromBranch));
                    }
                    else
                    {
                        _branchInfoList.AddRange(
                            repository.Branches.Where(b => !b.IsRemote)
                                .Select(CreateBranchInfoFromBranch));
                    }

                    //we did not find and branch.. must just have a master and never pushed to the server
                    if (_branchInfoList.Count == 0)
                    {
                        _branchInfoList.Add(CurrentBranchInfo);
                    }
                }
                return _branchInfoList;
            }
        }

        private GitBranchInfo CreateBranchInfoFromBranch(Branch branch)
        {
            return new GitBranchInfo
            {
                CanonicalName = branch.CanonicalName,
                RemoteName = branch.RemoteName,
                Name = branch.FriendlyName,
                IsRemote = branch.IsRemote,
                Sha = branch.Tip?.Sha,
                IsCurrentRepoHead = branch.IsCurrentRepositoryHead
            };
        }

        #endregion


        #region Diff Functions

        public string Diff(string fileName)
        {
            using (var repository = GetRepository())
            {
                //var diffTree = repository.Diff.Compare<Patch>(repository.Head?.Tip?.Tree,
                //    DiffTargets.Index | DiffTargets.WorkingDirectory);
                var patch = repository.Diff.Compare<Patch>(new List<string>() { fileName });

                return patch.Content; //diffTree?[fileName].;
            }

        }

        public string DiffFile(string fileName, string commitId1, string commitId2)
        {
            using (var repository = GetRepository())
            {
                var commitOld = repository.Lookup<LibGit2Sharp.Commit>(commitId1);
                var commitNew = repository.Lookup<LibGit2Sharp.Commit>(commitId2);
                var diffTree = repository.Diff.Compare<Patch>(commitOld.Tree, commitNew.Tree);
                return diffTree[fileName].Patch;
            }
        }

        public List<Change> GetChanges(string commitId1, string commitId2)
        {
            List<Change> changes = new List<Change>();
            using (var repository = GetRepository())
            {
                var commitOld = repository.Lookup<LibGit2Sharp.Commit>(commitId1);
                var commitNew = repository.Lookup<LibGit2Sharp.Commit>(commitId2);
                return repository.Diff.Compare<Patch>(commitOld.Tree, commitNew.Tree).Select(BuildChange).ToList();
            }
        }

        public List<Change> GetChangedFilesForCommit(string commitIdSha)
        {
            List<Change> changes = new List<Change>();
            using (var repository = GetRepository())
            {
                try
                {
                    var commit = repository.Lookup<LibGit2Sharp.Commit>(commitIdSha);
                    var commitParent = commit.Parents.Single();
                    return repository.Diff.Compare<Patch>(commit.Tree, commitParent.Tree).Select(BuildChange).ToList();
                }
                catch (Exception)
                {
                    return new List<Change>();
                }
            }
        }



        //TODO
        public string Blame(string fileName)
        {
            return null;
            //if (String.IsNullOrWhiteSpace(fileName))
            //{
            //    return "";
            //}
            //try
            //{
            //    using (var repository = GetRepository())
            //    {
            //        var test = new BlameOptions();
            //        var blame = repository.Blame(fileName).ToList();
            //        foreach (var blameHunk in blame)
            //        {
            //            blameHunk.FinalCommit
            //        }

            //    }
            //}
            //catch (Exception)
            //{

            //    throw;
            //}

            //if (!this.IsGit) return "";

            ////var tmpFileName = Path.ChangeExtension(Path.GetTempFileName(), ".blame");
            ////var fileNameRel = fileName;
            ////GitBash.RunCmd(string.Format("blame -M -w -- \"{0}\" > \"{1}\"", fileNameRel, tmpFileName), WorkingDirectory);
            ////return tmpFileName;
        }



        public string DefaultDiffCommand
        {
            get
            {
                using (var repository = GetRepository())
                {
                    var diffGuiTool = repository.Config.Get<string>("diff.guitool");
                    if (diffGuiTool == null)
                    {
                        diffGuiTool = repository.Config.Get<string>("diff.tool");
                        if (diffGuiTool == null)
                            return string.Empty;
                    }

                    var diffCmd = repository.Config.Get<string>("difftool." + diffGuiTool.Value + ".cmd");
                    if (diffCmd == null || diffCmd.Value == null)
                        return string.Empty;

                    return diffCmd.Value;
                }
            }
        }

        #endregion

        #region File Functions


        public string GetUnmodifiedFileByAbsolutePath(string filename, string sha = null)
        {
            var relativePath = "";
            Blob oldBlob = null;

            if (TryGetRelativePath(filename, out relativePath))
            {
                return GetUnmodifiedFileByRelativePath(relativePath, sha);
            }
            return null;
        }

        public string GetUnmodifiedFileByRelativePath(string relativePath, string sha = null)
        {
            Blob oldBlob = null;
            using (var repository = GetRepository())
            {
                try
                {
                    if (!String.IsNullOrWhiteSpace(sha))
                    {
                        var commit = repository.Lookup<LibGit2Sharp.Commit>(sha);
                        var treeEntry = commit[relativePath];
                        oldBlob = (Blob)treeEntry.Target;
                    }
                    else
                    {
                        var indexEntry = repository.Index[relativePath];
                        if (indexEntry != null)
                        {
                            oldBlob = repository.Lookup<Blob>(indexEntry.Id);
                        }
                    }
                }
                catch (Exception)
                {
                }
                return oldBlob != null ? oldBlob.GetContentText(new FilteringOptions(relativePath)) : string.Empty;
            }
        }



        #endregion

        #region Git commands

        private string GitRun(string cmd)
        {
            if (!this.IsGit) return null;
            var result = GitBash.Run(cmd, this.WorkingDirectory);
            if (result.HasError) throw new GitException(result.Error);
            if (result.Output.StartsWith("fatal:")) throw new GitException(result.Output);
            return result.Output;
        }


        internal string GetBranchId(string name)
        {
            string id = null;
            var result = GitBash.Run("rev-parse " + name, this.WorkingDirectory);
            if (!result.HasError && !result.Output.StartsWith("fatal:"))
            {
                id = result.Output.Trim();
            }
            return id;
        }

        internal string DeleteBranch(string name)
        {
            return GitRun("branch -d " + name);
        }

        public void CheckOutBranch(string branch, bool createNew = false)
        {
            GitRun(string.Format("checkout {0} {1}", (createNew ? "-b" : ""), branch));
        }

        #endregion

        public static string Init(string folderName)
        {
            return Repository.Init(folderName);
        }

     


       

        private Change BuildChange(PatchEntryChanges change)
        {
            var fileChange = new Change();
            fileChange.Name = change.Path;
            switch (change.Status)
            {
                case ChangeKind.Added:
                    fileChange.ChangeType = ChangeType.Added;
                    break;
                case ChangeKind.Copied:
                    fileChange.ChangeType =ChangeType.Copied;
                    break;
                case ChangeKind.Deleted:
                    fileChange.ChangeType =ChangeType.Deleted;
                    break;
                case ChangeKind.Modified:
                    fileChange.ChangeType =ChangeType.Modified;
                    break;
                case ChangeKind.Renamed:
                    fileChange.ChangeType =ChangeType.Renamed;
                    break;
                case ChangeKind.TypeChanged:
                    fileChange.ChangeType =ChangeType.TypeChanged;
                    break;
                case ChangeKind.Unmodified:
                    fileChange.ChangeType =ChangeType.Unmerged;
                    break;
            }
            return fileChange;
        }


        public string ChangedFilesStatus
        {
            get
            {
                var changed = ChangedFiles;
                return string.Format(this.CurrentBranchDisplayName + " +{0} ~{1} -{2} !{3}",
                    changed.Where(f => f.Status == GitFileStatus.New || f.Status == GitFileStatus.Added).Count(),
                    changed.Where(f => f.Status == GitFileStatus.Modified || f.Status == GitFileStatus.Staged).Count(),
                    changed.Where(f => f.Status == GitFileStatus.Deleted || f.Status == GitFileStatus.Removed).Count(),
                    changed.Where(f => f.Status == GitFileStatus.Conflict).Count());
            }
        }

        public IEnumerable<GitFile> ChangedFiles
        {
            get
            {

                if (_changedFiles == null)
                {
                    //_changedFiles = new List<GitFile>();
                    try
                    {

                        _changedFiles = GetCurrentChangedFiles();
                    }
                    catch
                    {
                        _changedFiles = new Dictionary<string, GitFile>();
                    }

                }
                return _changedFiles.Values.ToList();

            }
        }

        //private List<GitFile> GetCurrentChangedFilesList()
        //{
        //    var changedFileCache = GetCurrentChangedFiles();
        //    //var files = changedFileCache.ToDictionary(gitFile => gitFile.FilePath.ToLower(), gitFile => gitFile.Status);
        //    _changedFiles = changedFileCache;
        //    return changedFileCache;
        //}

        //private List<string> CreateRepositoryUpdateChangeSet()
        //{
        //    var _lastChanged = _changedFiles;
        //    //we have no idea what changes happened before.. so update everthing 
        //    if (_lastChanged == null)
        //    {
        //        return GetFullPathForGitFiles(GetCurrentFilesStatus());
        //    }
        //    var changedFileCache = GetCurrentChangedFiles();
        //    var currentChangeList = GetFullPathForGitFiles(changedFileCache);
        //    var lastChangeList = GetFullPathForGitFiles(_lastChanged);
        //    foreach (var path in lastChangeList)
        //    {
        //        if (!currentChangeList.Contains(path))
        //        {
        //            currentChangeList.Add(path);
        //        }
        //    }
        //    _changedFiles = changedFileCache;
        //    return currentChangeList;
        //}

        private List<string> GetFullPathForGitFiles(List<GitFile> files)
        {
            return files.Select(gitFile => gitFile.FilePath).ToList();
        }

        private Dictionary<string, GitFile> GetCurrentChangedFiles(bool retryAllowed = true, Repository repository = null)
        {
            //var files = new List<GitFile>();
            var files = new Dictionary<string, GitFile>();
            //Repository repository = null;
            try
            {
                //let a function 
                if (repository == null)
                {
                    repository = _statusRepository;
                }
                var repoFiles = repository.RetrieveStatus(new StatusOptions()
                {
                    IncludeUnaltered = false,
                    RecurseIgnoredDirs = false
                });

                files = repoFiles.Where(item => IsChangedStatus(item.State) && !(FileIgnored(item.FilePath)))
                    .Select(item => new GitFile(repository, item)).ToDictionary(x=>x.FilePath,x =>x); //);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error In GetCurrentChangedFiles: " + ex.Message);
                Thread.Sleep(2000);
                _statusRepository?.Dispose();
                _statusRepository = GetRepository();
            }
            return files;
        }

        public async Task<List<GitFile>> GetCurrentChangeSet()
        {
            //ist<GitFile> changedFileCache;

            var changedFileCache = GetCurrentChangedFiles();
            return changedFileCache.Values.ToList();
        }

        private List<GitFile> GetCurrentFilesStatus()
        {
            using (var repository = GetRepository())
            {
                return
                    repository.RetrieveStatus(new StatusOptions() { IncludeUnaltered = true, RecurseIgnoredDirs = false })
                        .Select(item => new GitFile(repository, item))
                        .ToList();

            }
        }



        private void SetBranchName(bool supressEvent = false)
        {
            using (var repository = GetRepository())
            {
                //logic getting complicated time to break it out
                if (_savedState.Operation != repository.Info.CurrentOperation)
                {
                    _savedState.Operation = repository.Info.CurrentOperation;
                    _savedState.BranchName = null;
                }
                if (string.IsNullOrWhiteSpace(_savedState.BranchName) ||
                    !string.Equals(_savedState.BranchName, repository.Head.FriendlyName))
                {
                    //if ((string.IsNullOrWhiteSpace(repository.Head.FriendlyName) && string.Equals(_cachedBranchName, "master")) )
                    //{

                    var newBranchName = string.IsNullOrWhiteSpace(repository.Head.FriendlyName)
                        ? "master"
                        : repository.Head.FriendlyName;
                    if (string.Equals(_savedState.BranchName, newBranchName))
                    {
                        supressEvent = true;
                    }
                    else
                    {
                        _savedState.BranchName = newBranchName;
                        _branchDisplayName = null;
                    }

                    if (!supressEvent)
                    {
                        FireBranchChangedEvent(_savedState.BranchName);
                    }
                    //}

                }
            }
        }


        #region repository status: branch, in the middle of xxx
        public string CurrentBranchDisplayName
        {
            get
            {
                if (_savedState.BranchName == null)
                {
                    SetBranchName(true);
                }
                if (_branchDisplayName == null)
                {
                    using (var repository = GetRepository())
                    {
                        _branchDisplayName = _savedState.BranchName;
                        var repoState = repository.Info.CurrentOperation;

                        switch (repoState)
                        {
                            case CurrentOperation.Merge:
                                _branchDisplayName += " | MERGING";
                                break;
                            case CurrentOperation.Revert:
                                _branchDisplayName += " | REVERTING";
                                break;
                            case CurrentOperation.CherryPick:
                                _branchDisplayName += " | CHERRY-PIKCING";
                                break;
                            case CurrentOperation.Bisect:
                                _branchDisplayName += " | BISECTING";
                                break;
                            case CurrentOperation.Rebase:
                                _branchDisplayName += " | REBASE";
                                break;
                            case CurrentOperation.RebaseInteractive:
                                _branchDisplayName += " | REBASE-i";
                                break;
                            case CurrentOperation.RebaseMerge:
                                _branchDisplayName += " | REBASE-MERGE";
                                break;
                        }
                    }
                }
                return _branchDisplayName;
            }
        }



        private bool FileExistsInGit(string fileName)
        {
            return this.IsGit && File.Exists(Path.Combine(GitDirectory, fileName));
        }

        public bool IsInTheMiddleOfCherryPick
        {
            get { return this.IsGit && FileExistsInGit("CHERRY_PICK_HEAD"); }
        }

        private string GitDirectory
        {
            get { return Path.Combine(WorkingDirectory, ".git"); }
        }

        private bool FileExistsInRepo(string fileName)
        {
            return File.Exists(Path.Combine(WorkingDirectory, fileName));
        }

        private bool FileExistsInGit(string directory, string fileName)
        {
            if (Directory.Exists(GitDirectory))
            {
                foreach (var dir in Directory.GetDirectories(GitDirectory, directory))
                {
                    if (File.Exists(Path.Combine(dir, fileName))) return true;
                }
            }
            return false;
        }

        #endregion

        #region Event Triggers

        private void FireBranchChangedEvent(string name)
        {
            _onBranchChanged?.Invoke(this, name);
        }

        private void FireCommitShaChangedEvent(string name)
        {
            _onCommitChanged?.Invoke(this, name);
        }

        private void FireFilesChangedEvent(List<string> files)
        {
            _onFilesUpdateEventHandler?.Invoke(this, new GitFilesUpdateEventArgs(files));
        }

        private void FireFileStatusUpdateEvent(List<GitFile> files)
        {
            _onFilesStatusUpdateEventHandler?.Invoke(this, new GitFilesStatusUpdateEventArgs(files));
        }

        #endregion

        public string LastCommitMessage
        {
            get
            {
                try
                {
                    using (var repository = GetRepository())
                    {
                        return repository.Head.Tip.Message;
                    }
                }
                catch
                {
                    return "";
                }
            }
        }

        public GitFileStatus GetFileStatus(string fileName, bool forceRefresh = false)
        {
            try
            {
                fileName = Path.GetFullPath(fileName).ToLower();
                GitFile file;
                if (_changedFiles == null || forceRefresh)
                {
                    _changedFiles = GetCurrentChangedFiles();
                }
                if (_changedFiles.TryGetValue(fileName, out file))
                {
                    return file.Status;
                }
                //var file = ChangedFiles.FirstOrDefault(f => string.Equals(f.FilePath, fileName, StringComparison.OrdinalIgnoreCase));
                //if (file != null) return file.Status;

                if (FileExistsInRepo(fileName)) return GitFileStatus.Tracked;
                // did not check if the file is ignored for performance reason
                return GitFileStatus.NotControlled;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error In File System Changed Event: " + ex.Message);
                return GitFileStatus.NotControlled;
            }
        }


        public void AddIgnoreItem(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName)) return;

            fileName = fileName.Replace("\\", "/");

            var ignoreFile = Path.Combine(WorkingDirectory, ".gitignore");
            if (!File.Exists(ignoreFile))
            {
                using (StreamWriter sw = File.CreateText(ignoreFile))
                {
                    sw.WriteLine(fileName);
                }
            }
            else
            {
                using (StreamWriter sw = File.AppendText(ignoreFile))
                {
                    sw.WriteLine();
                    sw.Write(fileName);
                }
            }
        }

        public void CheckOutFile(string fileName)
        {
            GitRun(string.Format("checkout -- \"{0}\"", fileName));
        }


     


        public string GetRevision(string filename)
        {
            var relativePath = "";
            var revision = "";
            using (var repository = GetRepository())
            {
                if (TryGetRelativePath(filename, out relativePath))
                {
                    string objectName = Path.GetFileName(filename);
                    var indexEntry = repository.Index[relativePath];
                    if (indexEntry != null)
                    {
                        // determine if the file has been staged
                        var status = GetFileStatus(filename);
                        if (status == GitFileStatus.Added || status == GitFileStatus.Staged)
                            revision = "index";
                        else
                            revision = repository.Head.Tip.Sha.Substring(0, 7);
                    }
                }
            }
            return revision;
        }

        private bool TryGetRelativePath(string fileName, out string relativePath)
        {
            relativePath = null;
            if (fileName.StartsWith(workingDirectory, StringComparison.OrdinalIgnoreCase))
            {
                relativePath = fileName.Substring(workingDirectory.Length);
                return true;
            }
            return false;
        }

      

        public IEnumerable<string> GetCommitsForFile(string fileName)
        {
            try
            {
                var commitList = new List<string>();
                using (var repository = GetRepository())
                {
                   var logs = repository.Commits.QueryBy(fileName);
                    foreach (var log in logs)
                    {
                        commitList.Add(log.Commit.Sha);
                    }
                }
                return commitList;
            }
            catch (Exception)
            {
                return new string[0];
            }
        }


        public void EditIngoreFile()
        {
            var ignoreFile = Path.Combine(WorkingDirectory, ".gitignore");

            var ret = GitBash.Run("config core.editor", WorkingDirectory);
            if (!ret.HasError && ret.Output.Trim() != "")
            {
                var editor = ret.Output.Trim();
                if (editor.Length == 0) editor = "notepad.exe";
                var cmd = string.Format("{0} \"{1}\"", editor, ignoreFile);
                cmd = cmd.Replace("/", "\\");
                var pinfo = new ProcessStartInfo("cmd.exe")
                {
                    Arguments = "/C \"" + cmd + "\"",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WorkingDirectory = this.WorkingDirectory,
                };
                Process.Start(pinfo);
            }
        }

        private RepositoryGraph repositoryGraph;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public RepositoryGraph RepositoryGraph
        {
            get
            {
                if (repositoryGraph == null)
                {
                    repositoryGraph = IsGit ? new RepositoryGraph(this.WorkingDirectory) : null;
                }
                return repositoryGraph;
            }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public IEnumerable<string> Remotes
        {
            get
            {
                if (remotes == null)
                {
                    var result = GitBash.Run("remote", this.WorkingDirectory);
                    if (!result.HasError)
                        remotes = result.Output.Split('\n').Where(s => !string.IsNullOrWhiteSpace(s));
                }
                return remotes;
            }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public IDictionary<string, string> Configs
        {
            get
            {
                if (configs == null)
                {
                    var result = GitBash.Run("config -l", this.WorkingDirectory);
                    if (!result.HasError)
                    {
                        var lines = result.Output.Split('\n').Where(s => !string.IsNullOrWhiteSpace(s) && s.IndexOf("=") > 0).OrderBy(s => s);

                        configs = new Dictionary<string, string>();
                        foreach (var s in lines)
                        {
                            var pos = s.IndexOf("=");
                            var key = s.Substring(0, pos);
                            if (!configs.Keys.Contains(key))
                                configs.Add(key, s.Substring(pos + 1));
                        }
                    }
                }
                return configs ?? new Dictionary<string, string>();
            }
        }

        public void Dispose()
        {
            DisableRepositoryWatcher();
            _statusRepository?.Dispose();
        }
    }

    public class GitFileStatusTracker : GitRepository
    {
        public GitFileStatusTracker(string directory) : base(directory)
        {
        }
    }
}
