using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GitScc
{
    public class GitActionResult<T>
    {
        public T Item { get; set; }
        public bool Succeeded { get; set; }
        public string ErrorMessage { get; set; }
    }
}
