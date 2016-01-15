using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitScc
{
    public class GitChangesetManager
    {
        private List<GitFile> _changedFiles;
        private ConcurrentDictionary<string, GitFileStatus> _fileStatus;


        public GitChangesetManager()
        {
            _fileStatus = new ConcurrentDictionary<string, GitFileStatus>();
        }

        #region Public Methods


        /// <summary>
        /// Send filename and status, and returns true if file status is different than last known status
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public bool StatusChanged(string filename, GitFileStatus status)
        {

            var file = filename.ToLower();
            var fileStatus = GitFileStatus.NotControlled;


            if (_fileStatus.TryGetValue(file, out fileStatus))
            {
                if (fileStatus == status)
                {
                    return false;
                }
                _fileStatus[file] = status;
                return true;
            }
            _fileStatus.TryAdd(file, status);
            return true;
        }

        #endregion


        /// <summary>
        /// Takes the new changeset and returns a list of files that have changed status
        /// </summary>
        /// <param name="newChangeSet"></param>
        /// <returns></returns>
        private List<string> CreateRepositoryUpdateChangeSet(List<GitFile> newChangeSet)
        {
            var _lastChanged = _changedFiles;

            //clean out the current _filestatus .. keeps the list small-ish and makes sure it's not out of date.. 
            _fileStatus = new ConcurrentDictionary<string, GitFileStatus>();
            foreach (var gitFile in newChangeSet)
            {
                _fileStatus.TryAdd(gitFile.FilePath, gitFile.Status);
            }
            //we have no idea what changes happened before.. so update everthing 
            if (_lastChanged == null)
            {
                return GetFullPathForGitFiles(newChangeSet);
            }
            var changedFileCache = newChangeSet;
            var currentChangeList = GetFullPathForGitFiles(changedFileCache);
            var lastChangeList = GetFullPathForGitFiles(_lastChanged);
            foreach (var path in lastChangeList.Where(path => !currentChangeList.Contains(path)))
            {
                currentChangeList.Add(path);
                _fileStatus.TryAdd(path, GitFileStatus.Unaltered);
            }
            _changedFiles = changedFileCache;
            return currentChangeList;
        }

        private List<string> GetFullPathForGitFiles(List<GitFile> files)
        {
            return files.Select(gitFile => gitFile.FilePath).ToList();
        }
    }
}
