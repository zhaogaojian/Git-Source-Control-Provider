namespace GitScc
{
    public class SwitchBranchInfo
    {
        public GitBranchInfo BranchInfo { get; set; }

        public string BranchName { get; set; }

        public bool CreateBranch { get; set; }

        public bool Switch { get; set; }

        public GitRepository Repository { get; set; }
    }
}