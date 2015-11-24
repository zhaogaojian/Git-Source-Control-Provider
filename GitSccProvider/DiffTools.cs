using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitSccProvider
{
    public enum DiffTools :int
    {
        [Description("Visual Studio")]
        VisualStudio,
        [Description("Default Git Tool (Set in your .gitconfig)")]
        GitDefault
    }
}
