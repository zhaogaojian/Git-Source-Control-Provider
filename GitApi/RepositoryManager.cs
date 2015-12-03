using System;
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
        }


        public static GitFileStatusTracker GetTrackerForPath(string filename, bool createTracker = true)
        {
            if (string.IsNullOrEmpty(filename)) return null;

            var repo = _repositories.Where(t => t.IsGit &&
                                  IsFileBelowDirectory(filename, t.WorkingDirectory, "\\"))
                           .OrderByDescending(t => t.WorkingDirectory.Length)
                           .FirstOrDefault();
            if (repo == null && createTracker && IsGitRepository(filename))
            {
                 repo = new GitFileStatusTracker(GetGitRepository(filename));
                repo.EnableRepositoryWatcher();
                repo.FileChanged += Repo_FileChanged;
                _repositories.Add(repo);
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


        public static  bool IsGitRepository(string path)
        {
            return Repository.IsValid(GetGitRepository(path));
        }

        /// <inheritdoc/>
        public static string GetGitRepository(string path)
        {
            return Repository.Discover(Path.GetFullPath(path));
        }

    }
}
