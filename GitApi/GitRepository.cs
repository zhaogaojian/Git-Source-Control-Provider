using GitScc.DataServices;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Threading;
using LibGit2Sharp;
using static GitScc.GitFile;

namespace GitScc
{
	public class GitRepository : IDisposable
	{
		private string workingDirectory;
        private List<GitFile> _changedFiles;
        private bool isGit;
        private string _branch;
        private IEnumerable<string> remotes;
        private IDictionary<string, string> configs;

	    private Repository _repository;


        public string WorkingDirectory { get { return workingDirectory; } }
        public bool IsGit { get { return Repository.IsValid(workingDirectory); } }

        public GitRepository(string directory)
		{
            this.workingDirectory = Repository.Discover(directory);
            _repository =  new Repository(workingDirectory);
            this.workingDirectory = _repository.Info.WorkingDirectory;

            //Repository
            //_repository.
            Refresh();
		}
        public void Refresh()
        {
            this.repositoryGraph = null;
            this._changedFiles = null;
            this._branch = null;
            this.remotes = null;
            this.configs = null;

            //var result = GitBash.Run("rev-parse --show-toplevel", WorkingDirectory); 
            //if (!result.HasError && !result.Output.StartsWith("fatal:"))
            //{
            //    this.workingDirectory = result.Output.Trim();
            //    result = GitBash.Run("rev-parse --is-inside-work-tree", WorkingDirectory);
            //    isGit = string.Compare("true", result.Output.Trim(), true) == 0;
            //}
        }

		#region Git commands

        private string GitRun(string cmd)
        {
            if (!this.IsGit) return null;
            var result = GitBash.Run(cmd, this.WorkingDirectory);
            if (result.HasError) throw new GitException(result.Error);
            if (result.Output.StartsWith("fatal:")) throw new GitException(result.Output);
            return result.Output;
        }

		internal string AddTag(string name, string id)
		{
			return GitRun(string.Format("tag \"{0}\" {1}", name, id));
		}

		internal string GetTagId(string name)
		{
			return GitRun("show-ref refs/tags/" + name);
		}

		internal string DeleteTag(string name)
		{
			return GitRun("tag -d " + name);
		}

		internal string AddBranch(string name, string id)
		{
			return GitRun(string.Format("branch \"{0}\" {1}", name, id));
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

		internal string Archive(string id, string fileName)
		{
			return GitRun(string.Format("archive {0} --format=zip --output \"{1}\"", id, fileName));
		}

		internal void Patch(string id1, string fileName)
		{
			GitRun(string.Format("format-patch {0} -1 --stdout > \"{1}\"", id1, fileName));
		}

		internal void Patch(string id1, string id2, string fileName)
		{
			GitRun(string.Format("format-patch {0}..{1} -o \"{2}\"", id1, id2, fileName));
		}

		#endregion    
	
        public static void Init(string folderName)
        {
            GitBash.Run("init", folderName);
            GitBash.Run("config core.ignorecase true", folderName);
        }

        private bool IsBinaryFile(string fileName)
        {
            FileStream fs = File.OpenRead(fileName);
            try
            {
                int len = Convert.ToInt32(fs.Length);
                if (len > 1000) len = 1000;
                byte[] bytes = new byte[len];
                fs.Read(bytes, 0, len);
                for (int i = 0; i < len - 1; i++)
                {
                    if (bytes[i] == 0) return true;
                }
                return false;
            }
            finally
            {
                fs.Close();
            }
        }


	    public string Diff(string fileName)
	    {
	        var diffTree = _repository.Diff.Compare<Patch>(_repository.Head.Tip.Tree,
	            DiffTargets.Index | DiffTargets.WorkingDirectory);

	        return diffTree[fileName].Patch;

	    }

        public string DiffFile(string fileName)
        {
          
            var tmpFileName = Path.ChangeExtension(Path.GetTempFileName(), ".diff");

            //var changes = _repository.Diff.Compare<Patch>(_repository.Head.Tip.Tree,
            //    DiffTargets.Index | DiffTargets.WorkingDirectory);
            //changes["File"].Patch

            foreach (var c in _repository.Diff.Compare<Patch>(_repository.Head.Tip.Tree,
                                                  DiffTargets.Index | DiffTargets.WorkingDirectory))
            {
                
                Console.WriteLine(c.Patch);
            }

            try
            {
                var status = GetFileStatus(fileName);
                if (status == GitFileStatus.NotControlled || status == GitFileStatus.New || status == GitFileStatus.Added)
                {
                    tmpFileName = Path.ChangeExtension(tmpFileName, Path.GetExtension(fileName));
                    File.Copy(Path.Combine(WorkingDirectory, fileName), tmpFileName);

                    if (IsBinaryFile(tmpFileName))
                    {
                        File.Delete(tmpFileName);
                        File.WriteAllText(tmpFileName, "Binary file: " + fileName);
                    }
                    return tmpFileName;
                }

                GitBash.RunCmd(string.Format("diff HEAD -- \"{0}\" > \"{1}\"", fileName, tmpFileName), WorkingDirectory);
            }
            catch (Exception ex)
            {
                File.WriteAllText(tmpFileName, ex.Message);
            }
            return tmpFileName;
        }

        public string ChangedFilesStatus
        {
            get
            {
                var changed = ChangedFiles;
                return string.Format(this.CurrentBranch + " +{0} ~{1} -{2} !{3}",
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
                        _changedFiles = new List<GitFile>();
                        
                        foreach (var item in _repository.RetrieveStatus(new StatusOptions() { IncludeUnaltered = false, RecurseIgnoredDirs = false}))
                        {
                            if (IsChangedStatus(item.State))
                            {
                                _changedFiles.Add(new GitFile(_repository, item));
                            }
                        }
                        //_changedFiles = _files.Where(x => x.Changed == true).ToList();
                    }
                    catch
                    {
                        _changedFiles = new List<GitFile>();
                    }
                }
                return _changedFiles;
            }
        }

        #region copied and modified from git extensions
      

        #endregion

        #region repository status: branch, in the middle of xxx
        public string CurrentBranch
        {
            get
            {
                if (_branch == null)
                {
                    _branch = "master";
                    var result = GitBash.Run("rev-parse --abbrev-ref HEAD", this.WorkingDirectory);
                    if (!result.HasError && !result.Output.StartsWith("fatal:"))
                    {
                        _branch = result.Output.Trim();
                        if (IsInTheMiddleOfBisect) _branch += " | BISECTING";
                        if (IsInTheMiddleOfMerge) _branch += " | MERGING";
                        if (IsInTheMiddleOfPatch) _branch += " | AM";
                        if (IsInTheMiddleOfRebase) _branch += " | REBASE";
                        if (IsInTheMiddleOfRebaseI) _branch += " | REBASE-i";
                        if (IsInTheMiddleOfCherryPick) _branch += " | CHERRY-PIKCING";
                    }
                }
                return _branch;
            }
        }

        public bool IsInTheMiddleOfBisect
        {
            get
            {
                return this.IsGit && FileExistsInGit("BISECT_START");
            }
        }

        public bool IsInTheMiddleOfMerge
        {
            get
            {
                return this.IsGit && FileExistsInGit("MERGE_HEAD");
            }
        }

        public bool IsInTheMiddleOfPatch
        {
            get
            {
                return this.IsGit && FileExistsInGit("rebase-*", "applying");
            }
        }

        public bool IsInTheMiddleOfRebase
        {
            get
            {
                return this.IsGit && FileExistsInGit("rebase-*", "rebasing");
            }
        }

        public bool IsInTheMiddleOfRebaseI
        {
            get
            {
                return this.IsGit && FileExistsInGit("rebase-*", "git-rebase-todo");
            }
        }

        private bool FileExistsInGit(string fileName)
        {
            return this.IsGit && File.Exists(Path.Combine(GitDirectory, fileName));
        }

        public bool IsInTheMiddleOfCherryPick
        {
            get
            {
                return this.IsGit && FileExistsInGit("CHERRY_PICK_HEAD");
            }
        }

        private string GitDirectory
        {
            get
            {
                return Path.Combine(WorkingDirectory, ".git");
            }
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

        public string LastCommitMessage
        {
            get
            {
                try
                {
                    return GitRun("log -1 --format=%s\r\n\r\n%b").Trim();
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

        public void StageFile(string fileName)
        {
            _repository.Stage(fileName);
        }

        public void UnStageFile(string fileName)
        {
            _repository.Unstage(fileName);
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

        public string Commit(string message, bool amend = false, bool signoff = false)
        {
            if (string.IsNullOrEmpty(message))
            {
                throw new ArgumentException("Commit message must not be null or empty!", "message");
            }
            Signature author = _repository.Config.BuildSignature(DateTimeOffset.Now);
            Signature committer = author;

            CommitOptions opts = new CommitOptions();
            opts.AmendPreviousCommit = amend;
            var commit = _repository.Commit(message, author,committer);
            return commit.Sha;
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


	    public string GetUnmodifiedFile(string filename)
	    {
	        var relativePath = "";
            Blob oldBlob = null;
            if (TryGetRelativePath(filename, out relativePath))
	        {
                string objectName = Path.GetFileName(filename);
                
                var indexEntry = _repository.Index[relativePath];
                if (indexEntry != null)
                {
                    oldBlob = _repository.Lookup<Blob>(indexEntry.Id);
                    var test = _repository.Config.Get<string>("diff.tool");
                }
	        }
            return oldBlob != null ? oldBlob.GetContentText(new FilteringOptions(relativePath)) : string.Empty;
	    }

	    public string DefaultDiffCommand
	    {
	        get
	        {
	            var diffGuiTool = _repository.Config.Get<string>("diff.guitool");
	            if (diffGuiTool == null)
	            {
	                diffGuiTool = _repository.Config.Get<string>("diff.tool");
	                if (diffGuiTool == null)
	                    return string.Empty;
	            }

	            var diffCmd = _repository.Config.Get<string>("difftool." + diffGuiTool.Value + ".cmd");
	            if (diffCmd == null || diffCmd.Value == null)
	                return string.Empty;

	            return diffCmd.Value;
	        }
	    }

        public string GetRevision(string filename)
        {
            var relativePath = "";
            var revision = "";
            if (TryGetRelativePath(filename, out relativePath))
            {
                string objectName = Path.GetFileName(filename);
                var indexEntry = _repository.Index[relativePath];
                if (indexEntry != null)
                {
                    // determine if the file has been staged
                    var status = GetFileStatus(filename);
                    if (status == GitFileStatus.Added || status == GitFileStatus.Staged)
                        revision = "index";
                    else
                        revision = _repository.Head.Tip.Sha.Substring(0, 7);
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

        public string DiffFile(string fileName, string commitId1, string commitId2)
        {
            if (!this.IsGit) return "";

            var tmpFileName = Path.ChangeExtension(Path.GetTempFileName(), ".diff");
            var fileNameRel = fileName;

            GitBash.RunCmd(string.Format("diff {2} {3} -- \"{0}\" > \"{1}\"", fileNameRel, tmpFileName, commitId1, commitId2), WorkingDirectory);
            return tmpFileName;
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
            if (!this.IsGit) return new string[0];

            var fileNameRel = fileName;

            var result = GitBash.Run(string.Format("log -z --ignore-space-change --pretty=format:%H -- \"{0}\"", fileNameRel), WorkingDirectory);
            if (!result.HasError)
                return result.Output.Split(new char[] { '\0' }, StringSplitOptions.RemoveEmptyEntries);
            return new string[0];
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

        public void SaveFileFromLastCommit(string fileName, string tempFile)
        {
            if (!this.isGit) return;
            var head = GetBranchId("HEAD");
            if (head != null)
            {
                GitBash.RunCmd(string.Format("show \"HEAD:{0}\" > \"{1}\"", fileName, tempFile), this.WorkingDirectory);
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
	        _repository.Dispose();
	        _repository = null;
	    }
	}

    public class GitFileStatusTracker: GitRepository
    {
        public GitFileStatusTracker(string directory) : base(directory) { }
    }
}
