using System.IO;
using LibGit2Sharp;

namespace GitScc
{
    public static class GitCommands
    {
        //TODO mode ot RepoManager
        public static DiffFileInfo GenerateDiffFileInfo(GitRepository repository, string filename)
        {
            var info = new DiffFileInfo();

            info.ActualFilename = Path.GetFileName(filename);
            info.ModifiedFilePath = filename;
            info.LastRevision = repository.GetRevision(filename);

            var filetype = Path.GetExtension(filename);

            //write unmodified file to disk
            var unmodifiedFile = repository.GetUnmodifiedFileByAbsolutePath(filename);
            var tempFileName = Path.GetTempFileName() + filetype;
            File.WriteAllText(tempFileName, unmodifiedFile);
            info.UnmodifiedFilePath = tempFileName;

            return info;
        }
    }
}
