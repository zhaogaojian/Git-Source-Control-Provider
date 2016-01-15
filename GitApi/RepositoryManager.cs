using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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

        private event GitFileUpdateEventHandler _onFileUpdateEventHandler;

        private event GitFilesUpdateEventHandler _onFilesUpdateEventHandler;

        private event GitRepositoryEventHandler _onActiveTrackerUpdateEventHandler;

        private event EventHandler<string> _onActiveTrackerBranchChanged;

        private event EventHandler<string> _onSolutionTrackerBranchChanged;

        private event EventHandler _trackerPauseRequest;

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
                    _activeTracker.BranchChanged += _activeTracker_BranchChanged;
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
                repo.FileChanged -= Repo_FileChanged;
                repo.FilesChanged -= Repo_FilesChanged;
            }

            if (_solutionTracker != null)
            {
                _solutionTracker.BranchChanged -= _solutionTracker_BranchChanged;
                _solutionTracker = null;
            }

            if (_activeTracker != null)
            {
                _activeTracker.BranchChanged -= _solutionTracker_BranchChanged;
                _activeTracker = null;
            }

            _repositories = new List<GitFileStatusTracker>();
            _fileRepoLookup = new ConcurrentDictionary<string, GitFileStatusTracker>();
            _basePathRepoLookup = new ConcurrentDictionary<string, GitFileStatusTracker>();
        }


        public void SetSolutionTracker(string solutionFilePath)
        {
            if (!string.IsNullOrWhiteSpace(solutionFilePath))
            {
                if (_solutionTracker != null)
                {
                    _solutionTracker.BranchChanged -= _solutionTracker_BranchChanged;
                }
                _solutionTracker = GetTrackerForPath(solutionFilePath, true, true);
                if (_solutionTracker != null)
                {
                    _solutionTracker.BranchChanged += _solutionTracker_BranchChanged;
                }
            }
        }



        public GitFileStatusTracker GetTrackerForPath(string filename,bool setActiveTracker = false ,bool createTracker = true)
        {
            if (string.IsNullOrWhiteSpace(filename)) return null;

            GitFileStatusTracker repo = null;

            //check out quick list to see if we have he file first. 
            if (!_fileRepoLookup.TryGetValue(filename, out repo))
            {
                var basePath = GetGitRepository(filename);
                if (!string.IsNullOrWhiteSpace(basePath) && !_basePathRepoLookup.TryGetValue(basePath, out repo))
                {
                    repo = new GitFileStatusTracker(basePath);
                    repo.EnableRepositoryWatcher();
                    repo.FileChanged += Repo_FileChanged;
                    repo.FilesChanged += Repo_FilesChanged;
                    //repo.BranchChanged += Repo_BranchChanged;

                    //add our refrences so we can do a quick lookup later
                    _repositories.Add(repo);
                    _basePathRepoLookup.TryAdd(basePath, repo);
                    _fileRepoLookup.TryAdd(filename, repo);
                }
            }

            if (setActiveTracker)
            {
                ActiveTracker = repo;
            }

            return repo;
        }

        #region Public Events

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

        private void Repo_FileChanged(object sender, GitFileUpdateEventArgs e)
        {
            FireFileChangedEvent(sender, e);
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

        private void FireFileChangedEvent(object sender, GitFileUpdateEventArgs e)
        {
            _onFileUpdateEventHandler?.Invoke(sender, e);
        }

        private void FireActiveTrackerChangedEvent(GitFileStatusTracker repository)
        {
            var eventArgs = new GitRepositoryEvent(repository);
            _onActiveTrackerUpdateEventHandler?.Invoke(this, eventArgs);
        }
        private void FireFilesChangedEvent(object sender, GitFilesUpdateEventArgs e)
        {
            _onFilesUpdateEventHandler?.Invoke(sender, e);
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
            return Repository.Discover(Path.GetFullPath(path));
        }
        #endregion


    }
}
