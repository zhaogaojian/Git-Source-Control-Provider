using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibGit2Sharp;

namespace GitScc
{
    public class GitHeadState
    {
        public string BranchName { get; set; }
        public CurrentOperation Operation { get; set; }
        public string Sha { get; set; }
    }
}
