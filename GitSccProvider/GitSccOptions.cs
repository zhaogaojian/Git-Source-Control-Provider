using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Xml.Serialization;
using GitSccProvider;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell.Settings;

namespace GitScc
{


   [Serializable]
    public class GitSccOptions
    {
        private static string configFileName = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "gitscc.config");

       const string CollectionPath = "GitExtreme";
        //Ok lame, but when you add a setting now, remember to add it to the load and save function
        public string GitBashPath       { get; set; }
        public string GitExtensionPath  { get; set; }
        public string DifftoolPath      { get; set; }
        public string TortoiseGitPath   { get; set; }
        public bool NotExpandTortoiseGit { get; set; }
        public bool NotExpandGitExtensions { get; set; }
        public bool UseTGitIconSet { get; set; }
        public bool DisableAutoRefresh { get; set; }
        public bool DisableAutoLoad { get; set; }
        public bool NotUseUTF8FileNames { get; set; }
        public bool DisableDiffMargin { get; set; }
        public bool UseVsDiff { get; set; }
        public bool AutoAddFiles { get; set; }
        public bool AutoAddProjects { get; set; }
        public DiffTools DiffTool { get; set; }

        public bool TrackActiveGitRepo { get; set; }

        private static GitSccOptions gitSccOptions;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public static GitSccOptions Current
        {
            get
            {
                if (gitSccOptions == null)
                {
                    gitSccOptions = LoadFromConfig();
                }
                return gitSccOptions;
            }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public static bool IsVisualStudio2010
        {
            get
            {
                return !IsVisualStudio2012 && BasicSccProvider.GetGlobalService(typeof(SVsSolution)) is IVsSolution4;
            }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public static bool IsVisualStudio2012
        {
            get
            {
                return BasicSccProvider.GetGlobalService(typeof(SVsDifferenceService)) != null;
            }
        }

        private static WritableSettingsStore WritableSettingsStore
        {
            get
            {
                //var service = BasicSccProvider.GetServiceEx<SVsServiceProvider>();
                var shellSettingsManager = new ShellSettingsManager(ServiceProvider.GlobalProvider);
                return shellSettingsManager.GetWritableSettingsStore(SettingsScope.UserSettings);
            }
        }

        private GitSccOptions()
        {
            //As Homer Simpson would say.. "Default? Woohoo! The two sweetest words in the English language!"
            //Default settings
            TrackActiveGitRepo = true;
            DisableAutoRefresh = true;
            AutoAddFiles = true;
        }

        internal static GitSccOptions LoadFromConfig()
        {
            var options = new GitSccOptions();
            options.GitBashPath = LoadStringFromConfig("GitBashPath");
            options.GitExtensionPath = LoadStringFromConfig("GitExtensionPath");
            options.DifftoolPath = LoadStringFromConfig("DifftoolPath");
            options.TortoiseGitPath = LoadStringFromConfig("TortoiseGitPath");

            //get bools
            options.NotExpandTortoiseGit = LoadBoolFromConfig("NotExpandTortoiseGit");
            options.NotExpandGitExtensions = LoadBoolFromConfig("NotExpandGitExtensions");
            options.UseTGitIconSet = LoadBoolFromConfig("UseTGitIconSet");
            options.DisableAutoRefresh = LoadBoolFromConfig("DisableAutoRefresh",true);
            options.DisableAutoLoad = LoadBoolFromConfig("DisableAutoLoad",true);
            options.NotUseUTF8FileNames = LoadBoolFromConfig("NotUseUTF8FileNames");
            options.DisableDiffMargin = LoadBoolFromConfig("DisableDiffMargin",true);
            options.UseVsDiff = LoadBoolFromConfig("UseVsDiff");
            options.AutoAddFiles = LoadBoolFromConfig("AutoAddFiles");
            options.AutoAddProjects = LoadBoolFromConfig("AutoAddProjects");

            options.DiffTool = (DiffTools) LoadIntFromConfig("DiffTool");
            options.Init();

            return options;
        }

        private void Init()
        {
            if (string.IsNullOrEmpty(GitBashPath))
            {
                GitBashPath = TryFindFile(new string[]{
                    @"C:\Program Files\Git\bin\sh.exe",
                    @"C:\Program Files (x86)\Git\bin\sh.exe",
                });
            }
            if (string.IsNullOrEmpty(GitExtensionPath))
            {
                GitExtensionPath = TryFindFile(new string[]{
                    @"C:\Program Files\GitExtensions\GitExtensions.exe",
                    @"C:\Program Files (x86)\GitExtensions\GitExtensions.exe",
                });
            }
            if (string.IsNullOrEmpty(TortoiseGitPath))
            {
                TortoiseGitPath = TryFindFile(new string[]{
                    @"C:\Program Files\TortoiseGit\bin\TortoiseProc.exe",
                    @"C:\Program Files (x86)\TortoiseGit\bin\TortoiseProc.exe",
                });
            }
            if (string.IsNullOrEmpty(TortoiseGitPath))
            {
                TortoiseGitPath = TryFindFile(new string[]{
                    @"C:\Program Files\TortoiseGit\bin\TortoiseGitProc.exe",
                    @"C:\Program Files (x86)\TortoiseGit\bin\TortoiseGitProc.exe",
                });
            }
            if (string.IsNullOrEmpty(DifftoolPath)) DifftoolPath = "diffmerge.exe";

            bool diffServiceAvailable = Package.GetGlobalService(typeof(SVsDifferenceService)) != null;
            if (!diffServiceAvailable)
                UseVsDiff = false;
        }

        internal void SaveConfig()
        {
            if (!WritableSettingsStore.CollectionExists(CollectionPath))
            {
                WritableSettingsStore.CreateCollection(CollectionPath);
            }

            //Save Strings
            WritableSettingsStore.SetString(CollectionPath, "GitBashPath", GitBashPath ?? "");
            WritableSettingsStore.SetString(CollectionPath, "GitExtensionPath", GitExtensionPath ?? "");
            WritableSettingsStore.SetString(CollectionPath, "DifftoolPath", DifftoolPath ?? "");
            WritableSettingsStore.SetString(CollectionPath, "TortoiseGitPath", TortoiseGitPath ?? "");

            //save bool
            WritableSettingsStore.SetBoolean(CollectionPath, "NotExpandTortoiseGit", NotExpandTortoiseGit);
            WritableSettingsStore.SetBoolean(CollectionPath, "NotExpandGitExtensions", NotExpandGitExtensions);
            WritableSettingsStore.SetBoolean(CollectionPath, "UseTGitIconSet", UseTGitIconSet);
            WritableSettingsStore.SetBoolean(CollectionPath, "DisableAutoRefresh", DisableAutoRefresh);
            WritableSettingsStore.SetBoolean(CollectionPath, "DisableAutoLoad", DisableAutoLoad);
            WritableSettingsStore.SetBoolean(CollectionPath, "NotUseUTF8FileNames", NotUseUTF8FileNames);
            WritableSettingsStore.SetBoolean(CollectionPath, "DisableDiffMargin", DisableDiffMargin);
            WritableSettingsStore.SetBoolean(CollectionPath, "UseVsDiff", UseVsDiff);
            WritableSettingsStore.SetBoolean(CollectionPath, "AutoAddFiles", AutoAddFiles);
            WritableSettingsStore.SetBoolean(CollectionPath, "AutoAddProjects", AutoAddProjects);

            //save int
            WritableSettingsStore.SetInt32(CollectionPath,"DiffTool",(int)DiffTool);

        //try
        //{
        //    XmlSerializer x = new XmlSerializer(typeof(GitSccOptions));
        //    using (TextWriter tw = new StreamWriter(configFileName))
        //    {
        //        x.Serialize(tw, this);
        //    }
        //}
        //catch { }
    }

       private static string LoadStringFromConfig(string propertyName, string defaultValue = "")
       {
          return WritableSettingsStore.GetString(CollectionPath, propertyName, defaultValue);
       }

        private static bool LoadBoolFromConfig(string propertyName, bool defaultValue = false)
        {
            return WritableSettingsStore.GetBoolean(CollectionPath, propertyName, defaultValue);
        }

        private static int LoadIntFromConfig(string propertyName, int defaultValue = 0)
        {
            return WritableSettingsStore.GetInt32(CollectionPath, propertyName, defaultValue);
        }

        private string TryFindFile(string[] paths)
        {
            foreach (var path in paths)
            {
                if (File.Exists(path)) return path;
            }
            return null;
        }
    }
}
