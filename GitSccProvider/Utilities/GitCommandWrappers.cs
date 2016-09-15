using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Threading;
using GitScc;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;
using ThreadHelper = Microsoft.VisualStudio.Shell.ThreadHelper;
using GitSccProvider.Utilities;

namespace GitScc.Utilities
{

    /// <summary>
    /// Static GIT Command Wrappers .. This will be the home of all the git commands in the BasicSccProvider.cs and SccProviderService.cs 
    /// </summary>
    public static class GitCommandWrappers
    {
        public static async Task InitRepo(string solutionFile)
        {
            var solutionPath = Path.GetDirectoryName(solutionFile);
            SolutionExtensions.WriteMessageToOutputPane("Creating Repo");
            GitRepository.Init(solutionPath);
            SolutionExtensions.WriteMessageToOutputPane("Repo Created");
            SolutionExtensions.WriteMessageToOutputPane("Adding .gitignore file");
            await IgnoreFileManager.UpdateGitIgnore(solutionPath);
            //File.WriteAllText(Path.Combine(solutionPath, ".tfignore"), @"\.git");
            RepositoryManager.Instance.Clear();
            Thread.Sleep(1000);
            var repo = RepositoryManager.Instance.GetTrackerForPath(solutionFile);
            if (repo != null)
            {
                repo.AddFile(Path.Combine(solutionPath, ".gitignore"));
                repo.AddFile(solutionFile);
                repo.Commit("Repo Created");
            }
        }

        internal static async Task<GitActionResult<string>> Commit(GitRepository repository, string message, bool signoff = false)
        {
            var result = new GitActionResult<string>();

            SolutionExtensions.WriteMessageToOutputPane("Commiting");

            if (String.IsNullOrWhiteSpace(message))
            {
                result.Succeeded = false;
                result.ErrorMessage = ErrorMessages.CommitMissingComment;
                SolutionExtensions.WriteMessageToOutputPane(ErrorMessages.CommitMissingComment);
                return result;
            }

            var stagedCount = repository?.ChangedFiles.Count(f => f.IsStaged) ?? 0;

            if (stagedCount <= 0)
            {
                result.Succeeded = false;
                result.ErrorMessage = ErrorMessages.CommitNoFilesStaged;
                SolutionExtensions.WriteMessageToOutputPane(ErrorMessages.CommitNoFilesStaged);
                return result;
            }

            result = repository?.Commit(message, false, signoff);
            if (result.Succeeded)
            {
                SolutionExtensions.WriteMessageToOutputPane("Commit successfully. Commit Hash: " + result.Item);
            }
            return result;

        }

        internal static async Task<GitActionResult<string>> AmendCommit(GitRepository repository, string message, bool signoff = false)
        {
            var result = new GitActionResult<string>();

            SolutionExtensions.WriteMessageToOutputPane("Amending Commiti");

            if (String.IsNullOrWhiteSpace(message))
            {
                result.Succeeded = false;
                result.ErrorMessage = ErrorMessages.CommitMissingComment;
                SolutionExtensions.WriteMessageToOutputPane(ErrorMessages.CommitMissingComment);
                return result;
            }

            result = repository?.Commit(message, true, signoff);
            if (result.Succeeded)
            {
                SolutionExtensions.WriteMessageToOutputPane("Amend last commit successfully. Commit Hash: " + result.Item);
            }
            return result;

        }

        internal static async Task<GitActionResult<GitBranchInfo>> SwitchCommand(SwitchBranchInfo result)
        {
            if (!result.CreateBranch && !result.Switch)
            {
                return new GitActionResult<GitBranchInfo>() { ErrorMessage = "No Branch Operation", Succeeded = false };
            }

            GitActionResult<GitBranchInfo> branchResult = new GitActionResult<GitBranchInfo>();
            bool inError = false;
            var branch = result.BranchInfo;
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            SolutionExtensions.WriteMessageToOutputPane("Branch Operation Started");
            if (result.CreateBranch)
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                SolutionExtensions.WriteMessageToOutputPane("Creating Branch");
                await TaskScheduler.Default;
                branchResult = result.Repository.CreateBranch(result.BranchName);
                if (branchResult.Succeeded)
                {
                    await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                    SolutionExtensions.WriteMessageToOutputPane("Branch: ' " + branchResult.Item.Name + "' Created");
                    branch = branchResult.Item;
                }
                else
                {
                    await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                    inError = true;
                    SolutionExtensions.WriteMessageToOutputPane(branchResult.ErrorMessage);
                }
            }
            if (result.Switch && !inError)
            {
                if (branch != null)
                {
                    await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                    SolutionExtensions.WriteMessageToOutputPane("Switching Branch");
                    await TaskScheduler.Default;
                    if (branch.IsRemote)
                    {
                        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                        SolutionExtensions.WriteMessageToOutputPane("Creating Local Branch");
                        var createResult = result.Repository.CreateBranch(branch.Name.Remove(0,branch.RemoteName.Length +1),branch.Sha);
                        if (!createResult.Succeeded)
                        {
                            return createResult;
                        }
                        result.Repository.SetRemoteBranch(createResult.Item,branch.RemoteName);
                        branchResult = result.Repository.Checkout(createResult.Item);
                    }
                    else
                    {
                        branchResult = result.Repository.Checkout(branch);
                    }
                    
                    if (!branchResult.Succeeded)
                    {
                        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                        inError = true;
                        SolutionExtensions.WriteMessageToOutputPane(branchResult.ErrorMessage);
                    }
                }
            }

            if (!inError)
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                SolutionExtensions.WriteMessageToOutputPane("Branch Operation Complete");
            }
            return branchResult;
        }
    }
}
