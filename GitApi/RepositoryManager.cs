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
    public static class RepositoryManager
    {
        private static List<GitFileStatusTracker> _repositories = new List<GitFileStatusTracker>();

        private static ConcurrentDictionary<string, GitFileStatusTracker> _fileRepoLookup =
            new ConcurrentDictionary<string, GitFileStatusTracker>();

        private static ConcurrentDictionary<string, GitFileStatusTracker> _basePathRepoLookup =
    new ConcurrentDictionary<string, GitFileStatusTracker>();

        private static event GitFileUpdateEventHandler _onFileUpdateEventHandler;

        public static List<GitFileStatusTracker> GetRepositories()
        {
             return _repositories; 
        }

        public static void Clear()
        {
            foreach (var repo in _repositories)
            {
                repo.FileChanged -= Repo_FileChanged;
            }
            _repositories = new List<GitFileStatusTracker>();
            _fileRepoLookup = new ConcurrentDictionary<string, GitFileStatusTracker>();
            _basePathRepoLookup = new ConcurrentDictionary<string, GitFileStatusTracker>();
        }


        public static GitFileStatusTracker GetTrackerForPath(string filename, bool createTracker = true)
        {
            if (string.IsNullOrEmpty(filename)) return null;

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

        private static void Repo_FileChanged(object sender, GitFileUpdateEventArgs e)
        {
            FireFileChangedEvent(sender, e);
        }

        public static event GitFileUpdateEventHandler FileChanged
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

        public static  bool IsGitRepository(string path)
        {
            return Repository.IsValid(GetGitRepository(path));
        }

        /// <inheritdoc/>
        public static string GetGitRepository(string path)
        {
            return Repository.Discover(Path.GetFullPath(path));
        }


        private static void FireFileChangedEvent(object sender, GitFileUpdateEventArgs e)
        {
            GitFileUpdateEventHandler changedHandler = _onFileUpdateEventHandler;
            if (changedHandler != null)
            {
                changedHandler(sender, e);
            }
        }

        private static bool IsFileBelowDirectory(string fileInfo, string directoryInfo, string separator)
        {
            var directoryPath = string.Format("{0}{1}"
            , directoryInfo
            , directoryInfo.EndsWith(separator) ? "" : separator);

            return fileInfo.StartsWith(directoryPath, StringComparison.OrdinalIgnoreCase);
        }

    }
}
