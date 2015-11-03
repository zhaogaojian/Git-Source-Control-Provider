using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.IO;
using LibGit2Sharp;

namespace GitScc
{
    public enum GitFileStatus
    {
        NotControlled,
        New,
        Tracked,
        Modified,
        Staged,
        Removed,
        Added,
        Deleted,
        Conflict,
        Merged,
        Ignored,
        Renamed,
        Copied,
        Nonexistent,
        Unaltered,

        Unreadable
    }

    public class GitFile : INotifyPropertyChanged
    {
        private string _fileName;
        private string _path;
        private const string PATH_STRING = @"{0}{1}";

        public GitFile(Repository repository, StatusEntry fileStatusEntry)
        {
            FileStatusEntry = fileStatusEntry;
            _path = Path.GetFullPath(string.Format(PATH_STRING, repository.Info.WorkingDirectory, FileStatusEntry.FilePath));
        }

        public StatusEntry FileStatusEntry { get; set; }


        public string FilePath
        {
            get { return _path; }
        }
        public string FileName
        {
            get
            {
                return FileStatusEntry.FilePath;
            }
        }
        public bool IsStaged {
            get
            {
                return Status == GitFileStatus.Added ||
                       Status == GitFileStatus.Staged ||
                       Status == GitFileStatus.Removed ||
                       Status == GitFileStatus.Renamed;
            }
        }


        public GitFileStatus Status
        {
            get
            {
                switch (FileStatusEntry.State)
                {
                    case FileStatus.Nonexistent:
                        return GitFileStatus.Nonexistent;
                    case FileStatus.Unaltered:
                        return GitFileStatus.Unaltered;
                    case FileStatus.Added:
                        return GitFileStatus.Added;
                    case FileStatus.Staged:
                        return GitFileStatus.Staged;
                    case FileStatus.Removed:
                        return GitFileStatus.Removed;
                    case FileStatus.RenamedInIndex:
                        return GitFileStatus.Renamed;
                    case FileStatus.StagedTypeChange:
                        return GitFileStatus.Modified;
                    case FileStatus.Untracked:
                        return GitFileStatus.New;
                    case FileStatus.Modified:
                        return GitFileStatus.Modified; 
                    case FileStatus.Missing:
                        return GitFileStatus.Deleted;
                    case FileStatus.TypeChanged:
                        return GitFileStatus.Modified;
                    case FileStatus.RenamedInWorkDir:
                        return GitFileStatus.Renamed;
                    case FileStatus.Unreadable:
                        return GitFileStatus.Unreadable; 
                    case FileStatus.Ignored:
                        return GitFileStatus.Ignored;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public bool Changed
        {
            get
            {
                switch (FileStatusEntry.State)
                {
                    case FileStatus.Nonexistent:
                        return false;
                    case FileStatus.Unaltered:
                        return false;
                    case FileStatus.Added:
                        return true;
                    case FileStatus.Staged:
                        return true;
                    case FileStatus.Removed:
                        return true;
                    case FileStatus.RenamedInIndex:
                        return true;
                    case FileStatus.StagedTypeChange:
                        return true;
                    case FileStatus.Untracked:
                        return true;
                    case FileStatus.Modified:
                        return true;
                    case FileStatus.Missing:
                        return true;
                    case FileStatus.TypeChanged:
                        return true;
                    case FileStatus.RenamedInWorkDir:
                        return true;
                    case FileStatus.Unreadable:
                        return false;
                    case FileStatus.Ignored:
                        return false;
                    default:
                        return false;
                }
            }
        }

        public bool isSelected;

        public bool IsSelected
        {
            get { return isSelected; }
            set
            {
                isSelected = value;
                OnPropertyChanged("IsSelected");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}
