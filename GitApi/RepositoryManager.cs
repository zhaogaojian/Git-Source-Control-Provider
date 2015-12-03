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

        public static List<GitFileStatusTracker> GetRepositories()
        {
             return _repositories; 
        }

        public static void Clear()
        {
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
                _repositories.Add(repo);
            }
            return repo;
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
