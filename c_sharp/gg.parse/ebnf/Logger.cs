using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gg.parse.ebnf
{
    public class Logger
    {
        /// <summary>
        /// If set to true an exception will be thrown when a warning is encountered
        /// </summary>
        public bool FailOnWarning { get; set; } = false;

        public void HandleLogs(string text, List<Annotation> tokens, List<Annotation> astNodes) { }
    }
}
