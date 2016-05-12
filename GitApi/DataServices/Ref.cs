using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GitScc.DataServices
{
    public class Ref
    {
        private readonly string _name;
        private readonly string _refName;
        private readonly string _id;
        private readonly RefTypes _refType;

        public Ref(string name, string refName, string id, RefTypes refType)
        {
            _name = name;
            _refName = refName;
            _refType = refType;
            _id = id;
        }

        public string Id => _id;
        public string RefName => _refName;
        public string Name => _name;

        public RefTypes Type => _refType;
       

        public override string ToString()
        {
            return Name;
        }
    }

    public enum RefTypes
    {
        Unknown,
        HEAD,
        Branch,
        Tag,
        RemoteBranch
    }

}