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
        private ConcurrentDictionary<string, GitFileStatus> _fileStatus;


        public GitChangesetManager()
        {
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


        public void SetStatus(string filename, GitFileStatus status)
        {
            if (!String.IsNullOrWhiteSpace(filename))
            {
                var fileKey = filename.ToLower();
                var changeStatus = GitFile.IsChangedStatus(status) ? status : GitFileStatus.Unaltered;
                if (changeStatus != GitFileStatus.Unaltered || _fileStatus.ContainsKey(fileKey))
                {
                    _fileStatus.AddOrUpdate(fileKey, changeStatus, (key, val) => changeStatus);
                }
            }
        }

        /// <summary>
        /// Takes the new changeset and returns a list of files that have changed status
        /// </summary>
        /// <param name="newChangeSet"></param>
        /// <returns></returns>
        private Dictionary<string, GitFileStatus> CreateRepositoryUpdateChangeSet(List<GitFile> newChangeSet)
        {
            var updatedFiles = new Dictionary<string, GitFileStatus>();
            var lastChangeList = _fileStatus.Where(x => x.Value != GitFileStatus.Unaltered).Select(x => x.Key).ToList();
            foreach (var file in lastChangeList)
            {
                if (!newChangeSet.Exists(x => x.FilePath.ToLower() == file.ToLower()))
                {
                    updatedFiles.Add(file.ToLower(), GitFileStatus.Unaltered);
                    _fileStatus.AddOrUpdate(file, GitFileStatus.Unaltered, (key, value) => GitFileStatus.Unaltered);
                }
            }

            foreach (var gitFile in newChangeSet)
            {
                GitFileStatus fileStatus;
                if (_fileStatus.TryGetValue(gitFile.FilePath, out fileStatus))
                {
                    if (fileStatus != gitFile.Status)
                    {
                        updatedFiles.Add(gitFile.FilePath.ToLower(), gitFile.Status);
                        _fileStatus.AddOrUpdate(gitFile.FilePath.ToLower(), gitFile.Status, (key, value) => gitFile.Status);
                    }
                }
                else
                {
                    updatedFiles.Add(gitFile.FilePath.ToLower(), gitFile.Status);
                    _fileStatus.AddOrUpdate(gitFile.FilePath.ToLower(), gitFile.Status, (key, value) => gitFile.Status);
                }
            }
            return updatedFiles;
        }



        private List<string> GetFullPathForGitFiles(List<GitFile> files)
        {
            return files.Select(gitFile => gitFile.FilePath).ToList();
        }
    }
}
