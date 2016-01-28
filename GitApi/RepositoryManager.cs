using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibGit2Sharp;

namespace GitScc
{
    public delegate void GitRepositoryEventHandler(object sender, GitRepositoryEvent e);

    public sealed class RepositoryManager 
    {
        #region Properties 

        static readonly RepositoryManager _instance = new RepositoryManager();

        //Private 
        private List<GitFileStatusTracker> _repositories; //= new List<GitFileStatusTracker>();

        private ConcurrentDictionary<string, GitFileStatusTracker> _fileRepoLookup; //= new ConcurrentDictionary<string, GitFileStatusTracker>();

        private ConcurrentDictionary<string, GitFileStatusTracker> _basePathRepoLookup; //= new ConcurrentDictionary<string, GitFileStatusTracker>();

        private event GitFilesUpdateEventHandler _onFilesUpdateEventHandler;

        private event GitRepositoryEventHandler _onActiveTrackerUpdateEventHandler;
        private event GitFilesStatusUpdateEventHandler _onFilesStatusUpdateEventHandler;

        private event EventHandler<string> _onActiveTrackerBranchChanged;

        private event EventHandler<string> _onSolutionTrackerBranchChanged;

        private event EventHandler<string> _trackerPauseRequest;

        private event EventHandler _trackerUnPauseRequest;

        private DateTime _pauseTime = DateTime.MinValue;
        private bool _paused;

        private GitFileStatusTracker _activeTracker;
        private GitFileStatusTracker _solutionTracker;


        //Public

        public bool Active
        {
            get
            {
                if (_solutionTracker != null || _activeTracker != null)
                {
                    return true;
                }
                return false;
            }
        }

        public bool IsPaused
        {
            get { return _paused; }
            
        }

        public void PauseUpdates(string reason)
        {
            if (_paused)
            {
                _paused = true;
            }
        }


        public GitFileStatusTracker ActiveTracker
        {
            get { return _activeTracker; }
            set
            {
                if (_activeTracker != value)
                {
                    if (_activeTracker != null)
                    {
                        _activeTracker.BranchChanged-= _activeTracker_BranchChanged;
                    }
                    
                    _activeTracker = value;
                    if (_activeTracker != null)
                    {
                        _activeTracker.BranchChanged += _activeTracker_BranchChanged;
                    }
                    FireActiveTrackerChangedEvent(value);
                }
            }
        }

        public GitFileStatusTracker SolutionTracker
        {
            get
            {
                if (_solutionTracker == null)
                {
                    return _activeTracker;
                }
                return _solutionTracker;
            }
        }

        public static RepositoryManager Instance
        {
            get { return _instance; }
        }

        #endregion

        private RepositoryManager()
        {
            _repositories = new List<GitFileStatusTracker>();
            _fileRepoLookup = new ConcurrentDictionary<string, GitFileStatusTracker>();
            _basePathRepoLookup = new ConcurrentDictionary<string, GitFileStatusTracker>();
        }



        public List<GitFileStatusTracker> GetRepositories()
        {
             return _repositories; 
        }

        public void Clear()
        {
            foreach (var repo in _repositories)
            {
                repo.FilesChanged -= Repo_FilesChanged;
                repo.FileStatusUpdate -= Repo_FileStatusUpdate;
            }

            if (_solutionTracker != null)
            {
                _solutionTracker.BranchChanged -= _solutionTracker_BranchChanged;
                _solutionTracker = null;
            }
            ActiveTracker = null;

            //if (_activeTracker != null)
            //{
            //    _activeTracker.BranchChanged -= _solutionTracker_BranchChanged;
            //    _activeTracker = null;
            //}

            _repositories = new List<GitFileStatusTracker>();
            _fileRepoLookup = new ConcurrentDictionary<string, GitFileStatusTracker>();
            _basePathRepoLookup = new ConcurrentDictionary<string, GitFileStatusTracker>();
        }


        public async Task SetSolutionTracker(string solutionFilePath)
        {
            if (!string.IsNullOrWhiteSpace(solutionFilePath))
            {
                if (_solutionTracker != null)
                {
                    _solutionTracker.BranchChanged -= _solutionTracker_BranchChanged;
                }
                _solutionTracker = await GetTrackerForPathAsync(solutionFilePath, true, true);
                if (_solutionTracker != null)
                {
                    _solutionTracker.BranchChanged += _solutionTracker_BranchChanged;
                }
            }
        }


        public GitFileStatusTracker GetTrackerForPath(string filename, bool setActiveTracker = false, bool createTracker = true)
        {
            if (string.IsNullOrWhiteSpace(filename)) return null;

            GitFileStatusTracker repo = null;
            filename = filename.ToLower();

            //check out quick list to see if we have he file first. 
            if (!_fileRepoLookup.TryGetValue(filename, out repo))
            {
                var basePath = GetGitRepository(filename);
                if (createTracker && 
                    !string.IsNullOrWhiteSpace(basePath) && !_basePathRepoLookup.TryGetValue(basePath, out repo))
                {
                    repo = new GitFileStatusTracker(basePath);
                    repo.EnableRepositoryWatcher();
                    repo.FilesChanged += Repo_FilesChanged;
                    repo.FileStatusUpdate += Repo_FileStatusUpdate;
                    //repo.BranchChanged += Repo_BranchChanged;

                    //add our refrences so we can do a quick lookup later
                    _repositories.Add(repo);
                    _basePathRepoLookup.TryAdd(basePath, repo);
                }
                _fileRepoLookup.TryAdd(filename, repo);
            }


            //if (repo == null)
            //{
            //    return ActiveTracker;
            //}

            if (setActiveTracker && repo != null)
            {
                ActiveTracker = repo;
            }

            return repo;
        }

        public async Task<GitFileStatusTracker> GetTrackerForPathAsync(string filename,bool setActiveTracker = false ,bool createTracker = true)
        {
            if (string.IsNullOrWhiteSpace(filename)) return null;

            GitFileStatusTracker repo = null;
            filename = filename.ToLower();

            //check out quick list to see if we have he file first. 
            if (!_fileRepoLookup.TryGetValue(filename, out repo))
            {
                var basePath = await GetGitRepositoryAsync(filename);
                if (!string.IsNullOrWhiteSpace(basePath) && !_basePathRepoLookup.TryGetValue(basePath, out repo))
                {
                    repo = new GitFileStatusTracker(basePath);
                    repo.EnableRepositoryWatcher();
                    repo.FilesChanged += Repo_FilesChanged;
                    repo.FileStatusUpdate += Repo_FileStatusUpdate;
                    //repo.BranchChanged += Repo_BranchChanged;

                    //add our refrences so we can do a quick lookup later
                    _repositories.Add(repo);
                    _basePathRepoLookup.TryAdd(basePath, repo);
                }
                _fileRepoLookup.TryAdd(filename, repo);
            }


            if (repo == null)
            {
                return ActiveTracker;
            }

            if (setActiveTracker)
            {
                ActiveTracker = repo;
            }

            return repo;
        }


        public bool IsProjectInGitRepoitory(string filename)
        {
            if (string.IsNullOrWhiteSpace(filename))
            {
                return false;
            }
            var tracker = GetTrackerForPath(filename);
            return tracker != null;
        }


        #region Public Events

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

        public event GitFilesStatusUpdateEventHandler FileStatusUpdate
        {
            add
            {
                _onFilesStatusUpdateEventHandler += value;
            }
            remove
            {
                _onFilesStatusUpdateEventHandler -= value;
            }
        }

        public event GitRepositoryEventHandler ActiveTrackerChanged
        {
            add
            {
                _onActiveTrackerUpdateEventHandler += value;
            }
            remove
            {
                _onActiveTrackerUpdateEventHandler -= value;
            }
        }

        public event EventHandler<string> ActiveTrackerBranchChanged
        {
            add
            {
                _onActiveTrackerBranchChanged += value;
            }
            remove
            {
                _onActiveTrackerBranchChanged -= value;
            }
        }


        public event EventHandler<string> SolutionTrackerBranchChanged
        {
            add
            {
                _onSolutionTrackerBranchChanged += value;
            }
            remove
            {
                _onSolutionTrackerBranchChanged -= value;
            }
        }

        #endregion

        #region Event Handlers

        private void Repo_FileStatusUpdate(object sender, GitFilesStatusUpdateEventArgs e)
        {
            FireFileStatusUpdateEvent(sender,e);
        }

        private void Repo_FilesChanged(object sender, GitFilesUpdateEventArgs e)
        {
            FireFilesChangedEvent(sender, e);
        }


        private void _solutionTracker_BranchChanged(object sender, string e)
        {
            FireSolutionTrackerBranchChangedEvent(sender,e);
        }

        private void _activeTracker_BranchChanged(object sender, string e)
        {
            FireActiveTrackerBranchChangedEvent(sender,e);
        }

        #endregion

        #region Event Triggers

        private void FireActiveTrackerChangedEvent(GitFileStatusTracker repository)
        {
            var eventArgs = new GitRepositoryEvent(repository);
            _onActiveTrackerUpdateEventHandler?.Invoke(this, eventArgs);
        }
        private void FireFilesChangedEvent(object sender, GitFilesUpdateEventArgs e)
        {
            _onFilesUpdateEventHandler?.Invoke(sender, e);
        }
        private void FireFileStatusUpdateEvent(object sender, GitFilesStatusUpdateEventArgs e)
        {
            _onFilesStatusUpdateEventHandler?.Invoke(sender, e);
        }

        private void FireSolutionTrackerBranchChangedEvent(object sender, string name)
        {
            _onSolutionTrackerBranchChanged?.Invoke(sender, name);
        }
        private void FireActiveTrackerBranchChangedEvent(object sender, string name)
        {
            _onActiveTrackerBranchChanged?.Invoke(sender, name);
        }

        #endregion

        #region Public Static Helper Methods

        public static  bool IsGitRepository(string path)
        {
            return Repository.IsValid(GetGitRepository(path));
        }

        /// <inheritdoc/>
        public static string GetGitRepository(string path)
        {
            try
            {
                var repoPath = Repository.Discover(Path.GetFullPath(path));
                return repoPath;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static async Task<string> GetGitRepositoryAsync(string path)
        {
            var gitPath = await Task.Run(() =>
            {
                return Repository.Discover(Path.GetFullPath(path));
            });
            return gitPath;
        }
        #endregion


    }
}
