using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitScc
{
    public class GitFileUpdateEventArgs : EventArgs
    {
        public string FullPath { get; set; }
        public string Name { get; set; }

        public GitFileUpdateEventArgs(string fullPath, string name)
        {
            FullPath = fullPath;
            Name = name;
        }
    }

    public class GitFilesUpdateEventArgs : EventArgs
    {
        public List<string> Files { get; set; }

        public GitFilesUpdateEventArgs(List<string> files)
        {
            Files = files;
        }
    }

    public class GitRepositoryEvent : EventArgs
    {
        public GitRepository Repository { get; private set; }

        public GitRepositoryEvent(GitRepository repository)
        {
            Repository = repository;
        }
    }
}
