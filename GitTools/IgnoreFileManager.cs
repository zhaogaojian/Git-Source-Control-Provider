using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace GitTools
{
    public class IgnoreFileManager
    {
        private const string HEADER1 = "###### -- File Created With Git Source Control Provider 2015 -- ######";
        private const string HEADER2 = "###### -- From https://github.com/github/gitignore -- ######";
        private const string HEADER3 = "###### -- Warning Regenerating this file will erase all your custom ignores, unless you add them below the Custom Ignore section at the bottom --  ######";
        private const string IGNORESECTION = "###### -- Custom Ignore Section, Make sure all files you add to the git repo are below this line  --  ######";


        private static async Task GetLatestIgnoreFile()
        {
            var workingPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            var path = Path.Combine(workingPath, "VisualStudio.gitignore");
            try
            {
                using (WebClient client = new WebClient())
                {
                    await client.DownloadFileTaskAsync(new Uri("https://raw.githubusercontent.com/github/gitignore/master/VisualStudio.gitignore"), path);
                }
            }
            catch (Exception)
            {
                { }
                throw;
            }
          
        }

        public static async Task UpdateGitIgnore(string repoPath)
        {
            await GetLatestIgnoreFile();
            var linesToAdd = await GetCustomAllLinesAsync(repoPath);
        }

        private async Task<string> BuildNewIgnoreFile(string repoPath)
        {
            StringBuilder sb = new StringBuilder(HEADER1);
            sb.AppendLine(HEADER2);
            sb.AppendLine(HEADER3);


            var workingPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var path = Path.Combine(workingPath, "VisualStudio.gitignore");

            var mainBody = await ReadTextAsync(path);
            sb.Append(mainBody);
            sb.AppendLine();
            sb.AppendLine();
            sb.AppendLine();
            sb.AppendLine(IGNORESECTION);
            sb.AppendLine();

            if (HasGitIgnore(repoPath))
            {
                var linesToAdd = await GetCustomAllLinesAsync(repoPath);
                if (linesToAdd?.Count > 0)
                {
                    int start = string.IsNullOrWhiteSpace(linesToAdd[0]) ? 1: 0;

                        for (int i = start; i < linesToAdd.Count; i++)
                        {
                            sb.AppendLine(linesToAdd[i]);
                        }
                }
            }

            return sb.ToString();
        }


        private async Task<string> ReadTextAsync(string filePath)
        {
            using (FileStream sourceStream = new FileStream(filePath,
                FileMode.Open, FileAccess.Read, FileShare.Read,
                bufferSize: 4096, useAsync: true))
            {
                StringBuilder sb = new StringBuilder();
                byte[] buffer = new byte[0x1000];
                int numRead;
                while ((numRead = await sourceStream.ReadAsync(buffer, 0, buffer.Length)) != 0)
                {
                    string text = Encoding.Unicode.GetString(buffer, 0, numRead);
                    sb.Append(text);
                }

                return sb.ToString();
            }
        }

        public static Task<List<string>> GetCustomAllLinesAsync(string path)
        {
            return GetCustomAllLinesAsync(path, Encoding.UTF8);
        }

        private static async Task<List<string>> GetCustomAllLinesAsync(string path, Encoding encoding)
        {
            var lines = new List<string>();

            using (var reader = new StreamReader(path, encoding))
            {
                string line;
                bool addLines = false;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    if (addLines)
                    {
                        lines.Add(line);
                    }
                    if (string.Equals(line, IGNORESECTION))
                    {
                        addLines = true;
                    }
                }
            }

            return lines;
        }

        private async Task WriteTextAsync(string filePath, string text, FileMode mode)
        {
            byte[] encodedText = Encoding.Unicode.GetBytes(text);

            using (FileStream sourceStream = new FileStream(filePath,
                mode, FileAccess.Write, FileShare.None,
                bufferSize: 4096, useAsync: true))
            {
                await sourceStream.WriteAsync(encodedText, 0, encodedText.Length);
            };
        }


        private static bool HasGitIgnore(string repoPath)
        {
            
            var file = Directory.GetFiles(repoPath, ".gitignore").FirstOrDefault();
            if (file == null)
            {
                return true;
            }
            else
            {
               return false;
            }
        }


    }
}
