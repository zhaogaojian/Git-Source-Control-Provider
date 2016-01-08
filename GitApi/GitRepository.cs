using GitScc.DataServices;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Caching;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GitScc.Extensions;
using LibGit2Sharp;
using static GitScc.GitFile;
using Commit = GitScc.DataServices.Commit;


namespace GitScc
{
    public delegate void GitFileUpdateEventHandler(object sender, GitFileUpdateEventArgs e);
    public delegate void GitFilesUpdateEventHandler(object sender, GitFilesUpdateEventArgs e);


    public class GitRepository : IDisposable
    {

        private const string RFC2822Format = "ddd dd MMM HH:mm:ss yyyy K";

        private string workingDirectory;
        private readonly string _gitDirectory;
        private string _lastTipId;
        private List<GitFile> _changedFiles;
        private bool isGit;
        private string _branchDisplayName;
        private string _cachedBranchName;
        private CurrentOperation _cachedBranchOperation;
        private string repositoryPath;
        private IEnumerable<string> remotes;
        private IDictionary<string, string> configs;
        FileSystemWatcher _watcher;
        private MemoryCache _fileCache;
        private object _repoUpdateLock = new object();
        private List<GitBranchInfo> _branchInfoList;

        private DateTime _lastFileEvent = DateTime.MinValue;
        private DateTime _lastGitEvent = DateTime.MinValue;
        private static int _fileEventDelay = 2;
        private static int _gitEventDelay = 2;

        private event GitFileUpdateEventHandler _onFileUpdateEventHandler;

        private event GitFilesUpdateEventHandler _onFilesUpdateEventHandler;

        private event EventHandler<string> _onBranchChanged;

        public event GitFileUpdateEventHandler FileChanged
        {
            add
            {
                _onFileUpdateEventHandler += value;
            }
            remove
            {
                _onFileUpdateEventHandler -= value;
            }
        }

        public event GitFilesUpdateEventHandler FilesChanged
        {
            add
            {
                _onFilesUpdateEventHandler += value;
            }
            remove
            {
                _onFilesUpdateEventHandler -= value;
            }
        }

        public event EventHandler<string> BranchChanged
        {
            add
            {
                _onBranchChanged += value;
            }
            remove
            {
                _onBranchChanged -= value;
            }
        }

        //private Repository repository;


        public string WorkingDirectory { get { return workingDirectory; } }
        public bool IsGit { get { return Repository.IsValid(workingDirectory); } }

        public GitRepository(string directory)
        {
            _gitDirectory = Repository.Discover(directory);
            using (var repository = GetRepository())
            {
                this.workingDirectory = repository.Info.WorkingDirectory;
                repositoryPath = repository.Info.Path;
            }
            _cachedBranchOperation = CurrentOperation.None;
            //_lastTipId = repository.Head.Tip.Sha;
            Refresh();
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

        private void CreateGitFileEvent(FileSystemEventArgs e)
        {
            var fullPath = e.FullPath;
            var filename = e.Name;

            var extension = Path.GetExtension(fullPath)?.ToLower();

            if (extension != null && (string.Equals(extension, ".suo") || extension.EndsWith("~")))
            {
                return;
            }

            if (string.Equals(extension, ".tmp"))
            {
                if (fullPath.Contains("~RF") || fullPath.Contains("\\ve-"))
                {
                    return;
                }
            }


            if (fullPath.IsSubPathOf(repositoryPath))
            {
                if ((DateTime.UtcNow - _lastGitEvent).TotalSeconds > _gitEventDelay)
                {
                    HandleGitFileSystemChange();
                    _lastGitEvent = DateTime.UtcNow;
                }
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
                        //if ((DateTime.UtcNow - _lastFileEvent).TotalSeconds > _fileEventDelay)
                        //
                        FireFileChangedEvent(filename, fullPath);
                        //    _lastFileEvent = DateTime.UtcNow;
                        //}
                    }
                }

            }
        }


        /// <summary>
        /// Update status when some file change is detected in the .git dir
        /// </summary>
        private void HandleGitFileSystemChange()
        {
            lock (_repoUpdateLock)
            {
                var files = CreateRepositoryUpdateChangeSet();
                SetBranchName();
                _branchInfoList = null;
                if (files != null && files.Count > 0)
                {
                    FireFilesChangedEvent(files);
                }
            }
        }

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
            _cachedBranchName = null;
            _cachedBranchOperation = CurrentOperation.None;
            this.remotes = null;
            this.configs = null;
            _branchInfoList = null;
            SetBranchName();
        }
        #region Checkout Functions

        public async Task<GitActionResult<GitBranchInfo>> CheckoutAsync(GitBranchInfo info, bool force = false)
        {
            return await Task.Run(() => Checkout(info, force));
        }


        private GitActionResult<GitBranchInfo> Checkout(GitBranchInfo info, bool force = false)
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
                    var checkoutBranch = repository.Checkout(branch, options);
                    if (checkoutBranch != null)
                    {
                        result.Item = new GitBranchInfo
                        {
                            CanonicalName = checkoutBranch.CanonicalName,
                            RemoteName = checkoutBranch.Remote?.Name,
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

        public void StageFile(string fileName)
        {
            using (var repository = GetRepository())
            {
                repository.Stage(fileName);
            }

        }

        public void UnStageFile(string fileName)
        {
            using (var repository = GetRepository())
            {
                repository.Unstage(fileName);
            }
        }



        #endregion

        #region Commit Functions


        public string Commit(string message, bool amend = false, bool signoff = false)
        {
            using (var repository = GetRepository())
            {
                if (string.IsNullOrEmpty(message))
                {
                    throw new ArgumentException("Commit message must not be null or empty!", "message");
                }
                Signature author = repository.Config.BuildSignature(DateTimeOffset.Now);
                Signature committer = author;

                CommitOptions opts = new CommitOptions();
                opts.AmendPreviousCommit = amend;
                var commit = repository.Commit(message, author, committer);
                return commit.Sha;
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
                AuthorDate = DateTime.Parse(commit.Author.When.ToString(RFC2822Format, CultureInfo.InvariantCulture)),
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


        public GitActionResult<GitBranchInfo> CreateBranch(string branchName)
        {
            var result = new GitActionResult<GitBranchInfo>();
            try
            {
                using (var repository = GetRepository())
                {
                    var branch = repository.CreateBranch(branchName, "HEAD");
                    if (branch != null)
                    {
                        result.Item = new GitBranchInfo
                        {
                            CanonicalName = branch.CanonicalName,
                            RemoteName = branch.Remote?.Name,
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
                        RemoteName = branch.Remote?.Name,
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

        public List<GitBranchInfo> GetBranchInfo(bool includeRemote = true, bool includeLocal = true)
        {
            using (var repository = GetRepository())
            {
                if (_branchInfoList == null || _branchInfoList.Count <= 0)
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
                RemoteName = branch.Remote?.Name,
                Name = branch.FriendlyName,
                IsRemote = branch.IsRemote,
                Sha = branch.Tip?.Sha,
                IsCurrentRepoHead = branch.IsCurrentRepositoryHead
            };
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

        public static void Init(string folderName)
        {
            GitBash.Run("init", folderName);
            GitBash.Run("config core.ignorecase true", folderName);
        }

        //private bool IsBinaryFile(string fileName)
        //{
        //    FileStream fs = File.OpenRead(fileName);
        //    try
        //    {
        //        int len = Convert.ToInt32(fs.Length);
        //        if (len > 1000) len = 1000;
        //        byte[] bytes = new byte[len];
        //        fs.Read(bytes, 0, len);
        //        for (int i = 0; i < len - 1; i++)
        //        {
        //            if (bytes[i] == 0) return true;
        //        }
        //        return false;
        //    }
        //    finally
        //    {
        //        fs.Close();
        //    }
        //}


        public string Diff(string fileName)
        {
            using (var repository = GetRepository())
            {
                var diffTree = repository.Diff.Compare<Patch>(repository.Head.Tip.Tree,
                    DiffTargets.Index | DiffTargets.WorkingDirectory);

                return diffTree[fileName].Patch;
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

        //public string DiffFile(string fileName)
        //{
        //    using (var repository = GetRepository())
        //    {
        //        var tmpFileName = Path.ChangeExtension(Path.GetTempFileName(), ".diff");
        //        foreach (var c in repository.Diff.Compare<Patch>(repository.Head.Tip.Tree,
        //            DiffTargets.Index | DiffTargets.WorkingDirectory))
        //        {

        //            Console.WriteLine(c.Patch);
        //        }

        //        try
        //        {
        //            var status = GetFileStatus(fileName);
        //            if (status == GitFileStatus.NotControlled || status == GitFileStatus.New ||
        //                status == GitFileStatus.Added)
        //            {
        //                tmpFileName = Path.ChangeExtension(tmpFileName, Path.GetExtension(fileName));
        //                File.Copy(Path.Combine(WorkingDirectory, fileName), tmpFileName);

        //                if (IsBinaryFile(tmpFileName))
        //                {
        //                    File.Delete(tmpFileName);
        //                    File.WriteAllText(tmpFileName, "Binary file: " + fileName);
        //                }
        //                return tmpFileName;
        //            }

        //            GitBash.RunCmd(string.Format("diff HEAD -- \"{0}\" > \"{1}\"", fileName, tmpFileName),
        //                WorkingDirectory);
        //        }
        //        catch (Exception ex)
        //        {
        //            File.WriteAllText(tmpFileName, ex.Message);
        //        }
        //        return tmpFileName;
        //    }
        //}

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
                    try
                    {

                        _changedFiles = GetCurrentChangedFiles();
                    }
                    catch
                    {
                        _changedFiles = new List<GitFile>();
                    }

                }
                return _changedFiles;

            }
        }

        private List<string> CreateRepositoryUpdateChangeSet()
        {
            var _lastChanged = _changedFiles;
            //we have no idea what changes happened before.. so update everthing 
            if (_lastChanged == null)
            {
                return GetFullPathForGitFiles(GetCurrentFilesStatus());
            }
            var changedFileCache = GetCurrentChangedFiles();
            var currentChangeList = GetFullPathForGitFiles(changedFileCache);
            var lastChangeList = GetFullPathForGitFiles(_lastChanged);
            foreach (var path in lastChangeList)
            {
                if (!currentChangeList.Contains(path))
                {
                    currentChangeList.Add(path);
                }
            }
            _changedFiles = changedFileCache;
            return currentChangeList;


        }

        private List<string> GetFullPathForGitFiles(List<GitFile> files)
        {
            return files.Select(gitFile => gitFile.FilePath).ToList();
        }

        private List<GitFile> GetCurrentChangedFiles(bool retryAllowed = true)
        {
            var files = new List<GitFile>();
            try
            {
                using (var repository = GetRepository())
                {
                    var repoFiles = repository.RetrieveStatus(new StatusOptions()
                    {
                        IncludeUnaltered = false,
                        RecurseIgnoredDirs = false
                    });
                    files.AddRange(from item in repoFiles where IsChangedStatus(item.State) select new GitFile(repository, item));
                }
            }
            catch (Exception ex)
            {
                if (retryAllowed)
                {
                    return Task.Run(() =>
                    {
                        Thread.Sleep(500);
                        return GetCurrentChangedFiles(false);
                    }).Result;
                }
                else
                {
                    Debug.WriteLine("Error In GetCurrentChangedFiles: " + ex.Message);
                }

            }
            return files;
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
                //logic getting complicated time to breka it out
                if (_cachedBranchOperation != repository.Info.CurrentOperation)
                {
                    _cachedBranchOperation = repository.Info.CurrentOperation;
                    _cachedBranchName = null;
                }
                if (string.IsNullOrWhiteSpace(_cachedBranchName) ||
                    !string.Equals(_cachedBranchName, repository.Head.FriendlyName))
                {
                    //if ((string.IsNullOrWhiteSpace(repository.Head.FriendlyName) && string.Equals(_cachedBranchName, "master")) )
                    //{

                    var newBranchName = string.IsNullOrWhiteSpace(repository.Head.FriendlyName)
                        ? "master"
                        : repository.Head.FriendlyName;
                    if (string.Equals(_cachedBranchName, newBranchName))
                    {
                        supressEvent = true;
                    }
                    else
                    {
                        _cachedBranchName = newBranchName;
                        _branchDisplayName = null;
                    }

                    if (!supressEvent)
                    {
                        FireBranchChangedEvent(_cachedBranchName);
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
                if (_cachedBranchName == null)
                {
                    SetBranchName(true);
                }
                if (_branchDisplayName == null)
                {
                    using (var repository = GetRepository())
                    {
                        _branchDisplayName = _cachedBranchName;
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

        private void FireFilesChangedEvent(List<string> files)
        {
            _onFilesUpdateEventHandler?.Invoke(this, new GitFilesUpdateEventArgs(files));
        }

        private void FireFileChangedEvent(string filename, string fullpath)
        {
            GitFileUpdateEventHandler changedHandler = _onFileUpdateEventHandler;

            if (changedHandler != null)
            {
                var eventArgs = new GitFileUpdateEventArgs(fullpath, filename);
                changedHandler(this, eventArgs);
            }
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

        public GitFileStatus GetFileStatus(string fileName)
        {
            fileName = Path.GetFullPath(fileName);
            var file = ChangedFiles.FirstOrDefault(f => string.Equals(f.FilePath, fileName, StringComparison.OrdinalIgnoreCase));
            if (file != null) return file.Status;

            if (FileExistsInRepo(fileName)) return GitFileStatus.Tracked;
            // did not check if the file is ignored for performance reason
            return GitFileStatus.NotControlled;
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


        public string GetUnmodifiedFile(string filename)
        {
            var relativePath = "";
            Blob oldBlob = null;
            using (var repository = GetRepository())
            {
                if (TryGetRelativePath(filename, out relativePath))
                {
                    string objectName = Path.GetFileName(filename);

                    var indexEntry = repository.Index[relativePath];
                    if (indexEntry != null)
                    {
                        oldBlob = repository.Lookup<Blob>(indexEntry.Id);
                    }
                }
            }
            return oldBlob != null ? oldBlob.GetContentText(new FilteringOptions(relativePath)) : string.Empty;
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

      


        public string Blame(string fileName)
        {
            if (!this.IsGit) return "";

            var tmpFileName = Path.ChangeExtension(Path.GetTempFileName(), ".blame");
            var fileNameRel = fileName;
            GitBash.RunCmd(string.Format("blame -M -w -- \"{0}\" > \"{1}\"", fileNameRel, tmpFileName), WorkingDirectory);
            return tmpFileName;
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
            //repository.Dispose();
            //repository = null;
        }
    }

    public class GitFileStatusTracker : GitRepository
    {
        public GitFileStatusTracker(string directory) : base(directory)
        {
        }
    }
}
