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

        private event GitRepositoryEventHandler _onActiveTrackerUpdateEventHandler;

        private GitFileStatusTracker _activeTracker;


        //Public

        public GitFileStatusTracker ActiveTracker
        {
            get { return _activeTracker; }
            set
            {
                _activeTracker = value;
                FireActiveTrackerChangedEvent(value);
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
            }
            _repositories = new List<GitFileStatusTracker>();
            _fileRepoLookup = new ConcurrentDictionary<string, GitFileStatusTracker>();
            _basePathRepoLookup = new ConcurrentDictionary<string, GitFileStatusTracker>();
        }


        public GitFileStatusTracker GetTrackerForPath(string filename, bool createTracker = true)
        {
            if (string.IsNullOrWhiteSpace(filename)) return null;

            GitFileStatusTracker repo = null;

            //check out quick list to see if we have he file first. 
            if (!_fileRepoLookup.TryGetValue(filename, out repo))
            {
                var basePath = GetGitRepository(filename);
                if (!_basePathRepoLookup.TryGetValue(basePath, out repo))
                {
                    repo = new GitFileStatusTracker(basePath);
                    repo.EnableRepositoryWatcher();
                    repo.FileChanged += Repo_FileChanged;
                    //add our refrences so we can do a quick lookup later
                    _repositories.Add(repo);
                    _basePathRepoLookup.TryAdd(basePath, repo);
                    _fileRepoLookup.TryAdd(filename, repo);
                }
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

        #endregion

        #region Event Handlers

        private void Repo_FileChanged(object sender, GitFileUpdateEventArgs e)
        {
            FireFileChangedEvent(sender, e);
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
