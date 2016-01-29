using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GitScc;

namespace GitSccProvider.Utilities
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
            }
            
        }
    }
}
