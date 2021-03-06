﻿using Gitscc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GitScc.DataServices
{
    public class RepositoryGraph
    {
        private const int CommitsToLoad = 200;
        private const string LogFormat = "--pretty=format:%H%n%P%n%ar%n%an%n%ae%n%ci%n%T%n%s%n%b";

        private string workingDirectory;

        private IList<Commit> commits;
        private IList<Ref> refs;
        private IList<GraphNode> nodes;
        private IList<GraphLink> links;
        private bool isSimplified;
        private GitRepository _repository;

        public RepositoryGraph(string repository)
        {
            this.workingDirectory = repository;

            _repository = RepositoryManager.Instance.GetTrackerForPath(workingDirectory);
        }

        public IEnumerable<Commit> Commits
        {
            get
            {
                if (commits == null)
                {
                    commits = _repository.GetLatestCommits(CommitsToLoad);

                    //old code starts again
                    commits.ToList().ForEach(
                        commit => commit.ChildIds =
                                  commits.Where(c => c.ParentIds.Contains(commit.Id))
                                         .Select(c => c.Id).ToList());
                }

                return commits;
            }
        }

        public IList<Ref> Refs
        {
            get
            {
                if (refs == null)
                {
                    refs = new List<Ref>();
                
                    var branches = _repository.GetBranchInfo();
                    foreach (var info in branches)
                    {
                        refs.Add(new Ref(
                            name: info.Name,
                            refName: info.FullName,
                            id: info.Sha,
                            refType:
                                info.IsCurrentRepoHead
                                    ? RefTypes.HEAD
                                    : info.IsRemote ? RefTypes.RemoteBranch : RefTypes.Branch));
                    }
                }
                return refs;
            }
        }

        public IList<GraphNode> Nodes
        {
            get
            {
                if (nodes == null) GenerateGraph();
                return nodes;
            }
        }

        public IEnumerable<GraphLink> Links
        {
            get
            {
                if (links == null) GenerateGraph();
                return links;
            }
        }

        private void GenerateGraph()
        {
            GenerateGraph(Commits);
            if (IsSimplified)
            {
                GenerateGraph(GetSimplifiedCommits());
            }
        }

        private void GenerateGraph(IEnumerable<Commit> commits)
        {
            nodes = new List<GraphNode>();
            links = new List<GraphLink>();
            var lanes = new List<string>();

            var buf = new List<string>();
            int i = 0;

            foreach (var commit in commits)
            {
                var id = commit.Id;

                var refs = from r in this.Refs
                           where r.Id == id
                           select r;

                var children = (from c in commits
                                where c.ParentIds.Contains(id)
                                select c).ToList();

                var parents = (from c in commits
                               where c.ChildIds.Contains(id)
                               select c).ToList();
                var lane = lanes.IndexOf(id);

                if (lane < 0)
                {
                    lanes.Add(id);
                    lane = lanes.Count - 1;
                }

                int m = parents.Count() - 1;
                for (int n = m; n >= 0; n--)
                {
                    if (lanes.IndexOf(parents[n].Id) <= 0)
                    {
                        if (n == m)
                            lanes[lane] = parents[n].Id;
                        else
                            lanes.Add(parents[n].Id);
                    }
                }
                lanes.Remove(id);

                var node = new GraphNode
                {
                    X = lane,
                    Y = i++,
                    Id = id,
                    Subject = commit.Subject,
                    Message = commit.Message,
                    AuthorName = commit.AuthorName,
                    AuthorDateRelative = commit.AuthorDateRelative,
                    Refs = refs.ToArray(),
                };

                nodes.Add(node);

                foreach (var ch in children)
                {
                    var cnode = (from n in nodes
                                 where n.Id == ch.Id
                                 select n).FirstOrDefault();

                    if (cnode != null)
                    {
                        links.Add(new GraphLink
                        {
                            X1 = cnode.X,
                            Y1 = cnode.Y,
                            X2 = node.X,
                            Y2 = node.Y,
                            Id = id
                        });
                    }
                }

            }
        }

        private IEnumerable<Commit> GetSimplifiedCommits()
        {
            foreach (var commit in Commits)
            {
                if (commit.ParentIds.Count() == 1 && commit.ChildIds.Count() == 1 && !this.Refs.Any(r => r.Id == commit.Id))
                {
                    var cid = commit.ChildIds[0];
                    var pid = commit.ParentIds[0];

                    var parent = Commits.Where(c => c.Id == pid).FirstOrDefault();
                    var child = Commits.Where(c => c.Id == cid).FirstOrDefault();

                    if (parent != null && child != null)
                    {
                        int x1 = GetLane(parent.Id);
                        int x2 = GetLane(commit.Id);
                        int x3 = GetLane(child.Id);

                        if (x1 == x2 && x2 == x3)
                        {
                            commit.deleted = true;
                            parent.ChildIds[parent.ChildIds.IndexOf(commit.Id)] = cid;
                            child.ParentIds[child.ParentIds.IndexOf(commit.Id)] = pid;
                        }
                    }
                }
            }

            return commits.Where(c => !c.deleted);
        }

        private int GetLane(string id)
        {
            return Nodes.Where(n => n.Id == id).Select(n => n.X).FirstOrDefault();
        }

        public bool IsSimplified
        {
            get { return isSimplified; }
            set { isSimplified = value; commits = null; nodes = null; links = null; }
        }

        public Commit GetCommit(string commitId)
        {
            try
            {
                return _repository.GetCommitById(commitId);
            }
            catch (Exception ex)
            {
                Log.WriteLine("Repository.GetCommit: {0} \r\n{1}", commitId, ex.ToString());
            }
            return null;
        }

        private GitTreeObject GetTree(string commitId)
        {
            var commit = GetCommit(commitId);
            if (commit == null) return null;

            return new GitTreeObject
            {
                Id = commitId,
                Name = "",
                FullName = "",
                Type = "tree",
                IsExpanded = true,
                Repository = this.workingDirectory,
            };
        }

        public IEnumerable<Change> GetChanges(string commitId)
        {
            return GetChanges(commitId + "~1", commitId);
        }

        public IEnumerable<Change> GetChanges(string fromCommitId, string toCommitId)
        {
            try
            {
                return _repository.GetChanges(fromCommitId, toCommitId);

            }
            catch (Exception ex)
            {
                Log.WriteLine("Repository.GetChanges: {0} - {1}\r\n{2}", fromCommitId, toCommitId, ex.ToString());
            }
            return new List<Change>();
        }



        public byte[] GetFileContent(string commitId, string fileName)
        {
            try
            {
                var tmpFileName = GetFile(commitId, fileName);
                var content = File.ReadAllBytes(tmpFileName);
                if (File.Exists(tmpFileName)) File.Delete(tmpFileName);
                return content;
            }
            catch (Exception ex)
            {
                Log.WriteLine("Repository.GetFileContent: {0} - {1}\r\n{2}", commitId, fileName, ex.ToString());
            }

            return null;
        }

        public string GetFile(string commitId, string fileName)
        {
            var tmpFileName = Path.GetTempFileName();
            tmpFileName = Path.ChangeExtension(tmpFileName, Path.GetExtension(fileName));
            try
            {
                GitBash.RunCmd(string.Format("cat-file blob {0}:./{1} > {2}", commitId, fileName, tmpFileName),
                    this.workingDirectory);
            }
            catch (Exception ex)
            {
                Log.WriteLine("Repository.GetFile: {0} - {1}\r\n{2}", commitId, fileName, ex.ToString());
            }
            return tmpFileName;
        }
    }
}