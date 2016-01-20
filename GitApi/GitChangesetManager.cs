using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace GitScc
{
    public class GitChangesetManager
    {
        private List<GitFile> _currentChangeset;
        private ConcurrentDictionary<string, GitFileStatus> _fileStatus;
        private GitRepository _repostory;


        public GitChangesetManager(GitRepository repostory)
        {
            _repostory = repostory;
            _currentChangeset = _repostory.ChangedFiles.ToList();
            _fileStatus = new ConcurrentDictionary<string, GitFileStatus>();
        }

        #region Public Methods


        /// <summary>
        /// 
        /// </summary>
        /// <param name="newChangeSet"></param>
        /// <returns>true if files have changed</returns>
        public Dictionary<string, GitFileStatus> LoadChangeSet(List<GitFile> newChangeSet)
        {
           return CreateRepositoryUpdateChangeSet(newChangeSet);
        }

        public void Clear()
        {
            _currentChangeset = new List<GitFile>();
            _fileStatus = new ConcurrentDictionary<string, GitFileStatus>();
        }


        //public GitFileStatus GetFileStatus(string fileName)
        //{
        //    try
        //    {
        //        fileName = Path.GetFullPath(fileName).ToLower();
        //        var file = _currentChangeset.FirstOrDefault(f => string.Equals(f.FilePath, fileName, StringComparison.OrdinalIgnoreCase));
        //        if (file != null) return file.Status;

        //        if (FileExistsInRepo(fileName)) return GitFileStatus.Tracked;
        //        // did not check if the file is ignored for performance reason
        //        return GitFileStatus.NotControlled;
        //    }
        //    catch (Exception ex)
        //    {
        //        Debug.WriteLine("Error In File System Changed Event: " + ex.Message);
        //        return GitFileStatus.NotControlled;
        //    }
        //}

        private bool FileExistsInRepo(string fileName)
        {
            return File.Exists(Path.Combine(_repostory.WorkingDirectory, fileName));
        }

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
        private Dictionary<string, GitFileStatus> CreateRepositoryUpdateChangeSet(List<GitFile> newChangeSet)
        {
            var _lastChanged = _currentChangeset;
            var updatedFiles = new Dictionary<string, GitFileStatus>();
            //clean out the current _filestatus .. keeps the list small-ish and makes sure it's not out of date.. 
            //_fileStatus = new ConcurrentDictionary<string, GitFileStatus>();
            foreach (var gitFile in newChangeSet)
            {
                GitFileStatus fileStatus;
                if (_fileStatus.TryGetValue(gitFile.FilePath, out fileStatus))
                {
                    if (fileStatus != gitFile.Status)
                    {
                        updatedFiles.Add(gitFile.FilePath, gitFile.Status);
                        _fileStatus[gitFile.FilePath] = gitFile.Status;
                    }
                }
                else
                {
                    _fileStatus.TryAdd(gitFile.FilePath, gitFile.Status);
                    updatedFiles.Add(gitFile.FilePath, gitFile.Status);
                }
               
            }
            if (_lastChanged == null)
            {
                foreach (var gitFile in newChangeSet)
                {
                        updatedFiles.Add(gitFile.FilePath, GitFileStatus.Unaltered);
                }
            }
            else
            {
                foreach (var gitFile in _lastChanged)
                {
                    if (!newChangeSet.Exists(x => x.FilePath == gitFile.FilePath))
                    {
                        updatedFiles.Add(gitFile.FilePath, GitFileStatus.Unaltered);
                        _fileStatus.TryAdd(gitFile.FilePath, GitFileStatus.Unaltered);
                    }
                }
            }
            _currentChangeset = newChangeSet;
            return updatedFiles;
        }



        private List<string> GetFullPathForGitFiles(List<GitFile> files)
        {
            return files.Select(gitFile => gitFile.FilePath).ToList();
        }
    }
}
