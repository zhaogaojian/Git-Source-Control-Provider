using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitScc
{
   

    public class GitFilesUpdateEventArgs : EventArgs
    {
        public List<string> Files { get; set; }

        public GitFilesUpdateEventArgs(List<string> files)
        {
            Files = files;
        }
    }

    public class GitFilesStatusUpdateEventArgs : EventArgs
    {
        public List<GitFile> Files { get; set; }

        public GitFilesStatusUpdateEventArgs(List<GitFile> files)
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
