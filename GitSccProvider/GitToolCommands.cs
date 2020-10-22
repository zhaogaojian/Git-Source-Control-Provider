﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GitScc
{
    class GitToolCommand
    {
        public CommandScope Scope { get; set; }
        public string Name { get; set; }
        public string Command { get; set; }
        public bool SaveOnExecution { get; set; }

        public GitToolCommand(string name, string Command, CommandScope scope = CommandScope.Project, bool saveOnExecution = false)
        {
            this.Name = name;
            this.Command = Command;
            Scope = scope;
            SaveOnExecution = saveOnExecution;
        }
    }

    public enum CommandScope
    {
        File, Project
    }

    static class GitToolCommands
    {
        internal static List<string> IgnoreCommands = new List<String> {
            "Open",
            "Update"
        };

        internal static List<GitToolCommand> GitTorCommands = new List<GitToolCommand> { 
            new GitToolCommand("TortoiseGit", "/command:log"), // workaround to missing of the first command in menu
            new GitToolCommand("Blame", "/command:blame", CommandScope.File), 
            new GitToolCommand("Branch", "/command:branch"), 
            new GitToolCommand("Commit", "/command:commit", saveOnExecution: true), 
            new GitToolCommand("Export", "/command:export"), 
            new GitToolCommand("Merge", "/command:merge"),
            new GitToolCommand("Pull", "/command:pull"),
            new GitToolCommand("Push", "/command:push", CommandScope.File),
            new GitToolCommand("Rebase", "/command:rebase"), 
            new GitToolCommand("Resolve", "/command:resolve"), 
            new GitToolCommand("Revert", "/command:revert"), 
            new GitToolCommand("Settings", "/command:settings"), 
            new GitToolCommand("Show Log", "/command:log", CommandScope.File), 
            new GitToolCommand("Switch", "/command:switch"), 
            new GitToolCommand("Sync", "/command:sync", saveOnExecution: true), 
            new GitToolCommand("Tag", "/command:tag"), 
        };

        internal static List<GitToolCommand> GitExtCommands = new List<GitToolCommand> { 
            new GitToolCommand("Git Extensions", "browse"), // workaround to missing of the first command in menu
            new GitToolCommand("Add Files", "add"), 
            new GitToolCommand("Apply Patch", "applypatch"), 
            new GitToolCommand("Browse", "browse"), 
            new GitToolCommand("Create Branch", "branch"), 
            new GitToolCommand("Checkout Branch", "checkout"), 
            new GitToolCommand("Cherry Pick", "cherry"), 
            new GitToolCommand("Commit", "commit", saveOnExecution: true), 
            new GitToolCommand("Edit .gitignore", "gitignore"), 
            new GitToolCommand("Format Patch", "formatpatch"), 
            new GitToolCommand("Manage Remotes", "remotes"), 
            new GitToolCommand("Merge", "merge"), 
            new GitToolCommand("Pull", "pull"), 
            new GitToolCommand("Push", "push"), 
            new GitToolCommand("Rebase", "rebase"), 
            new GitToolCommand("Stash", "stash"), 
            new GitToolCommand("Settings", "settings"), 
            new GitToolCommand("Solve Merge Conflicts", "mergeconflicts"), 
        };

    }
}
