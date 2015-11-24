using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GitScc
{
    public class DiffFileInfo
    {
        public string UnmodifiedFilePath { get; set; }
        public string ModifiedFilePath { get; set; }
        public string ActualFilename { get; set; }
        public string LastRevision { get; set; }
    }
}
