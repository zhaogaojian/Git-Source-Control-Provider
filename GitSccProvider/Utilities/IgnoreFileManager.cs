using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace GitSccProvider.Utilities
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
                SolutionExtensions.WriteMessageToOutputPane("Getting Latest ignore file from github");
                using (WebClient client = new WebClient())
                {
                    await client.DownloadFileTaskAsync(new Uri("https://raw.githubusercontent.com/github/gitignore/master/VisualStudio.gitignore"), path);
                    SolutionExtensions.WriteMessageToOutputPane("Success");
                }
            }
            catch (Exception)
            {
                SolutionExtensions.WriteMessageToOutputPane("Download Failed");
            }
          
        }

        public static async Task UpdateGitIgnore(string repoPath)
        {
            var path = Path.Combine(repoPath, ".gitignore");
            var encoding = GetEncoding(path);
            await GetLatestIgnoreFile();
            SolutionExtensions.WriteMessageToOutputPane("Updating .gitignore file");
            var text = await  BuildNewIgnoreFile(repoPath,path, encoding);
            await WriteTextAsync(path, text, FileMode.Create, encoding);
            SolutionExtensions.WriteMessageToOutputPane("Updating .gitignore done!");
        }

        private static async Task<string> BuildNewIgnoreFile(string repoPath, string ignorePath, Encoding encoding)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(HEADER1);
            sb.AppendLine(HEADER2);
            sb.AppendLine(HEADER3);


            var workingPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            var path = Path.Combine(workingPath, "VisualStudio.gitignore");

            if (!File.Exists(path))
            {
                SolutionExtensions.WriteMessageToOutputPane("Updated .ignorefile not found, using cached ignore file");
                path = Path.Combine(workingPath, "Resources\\VisualStudio.gitignore");
            }

            var mainBody = File.ReadAllText(path);
            sb.Append(mainBody);
            sb.AppendLine();
            sb.AppendLine();
            sb.AppendLine();
            sb.AppendLine(IGNORESECTION);
            sb.AppendLine();

            if (HasGitIgnore(repoPath))
            {
                var linesToAdd = await GetCustomAllLinesAsync(ignorePath, encoding);
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


        private static async Task<string> ReadTextAsync(string filePath)
        {
            try
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
            catch (Exception)
            {
                return string.Empty;
            }
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

        private static  async Task WriteTextAsync(string filePath, string text, FileMode mode, Encoding encoding)
        {
            byte[] encodedText = encoding.GetBytes(text);

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
            if (file != null)
            {
                return true;
            }
            else
            {
               return false;
            }
        }

        /// Determines a text file's encoding by analyzing its byte order mark (BOM).
        /// Defaults to ASCII when detection of the text file's endianness fails.
        /// </summary>
        /// <param name="filename">The text file to analyze.</param>
        /// <returns>The detected encoding.</returns>
        public static Encoding GetEncoding(string filename)
        {
            // Read the BOM
            if (File.Exists(filename))
            {
                var bom = new byte[4];
                using (var file = new FileStream(filename, FileMode.Open, FileAccess.Read))
                {
                    file.Read(bom, 0, 4);
                }

                // Analyze the BOM
                if (bom[0] == 0x2b && bom[1] == 0x2f && bom[2] == 0x76) return Encoding.UTF7;
                if (bom[0] == 0xef && bom[1] == 0xbb && bom[2] == 0xbf) return Encoding.UTF8;
                if (bom[0] == 0xff && bom[1] == 0xfe) return Encoding.Unicode; //UTF-16LE
                if (bom[0] == 0xfe && bom[1] == 0xff) return Encoding.BigEndianUnicode; //UTF-16BE
                if (bom[0] == 0 && bom[1] == 0 && bom[2] == 0xfe && bom[3] == 0xff) return Encoding.UTF32;
            }
            return Encoding.ASCII;
        }

    }
}
