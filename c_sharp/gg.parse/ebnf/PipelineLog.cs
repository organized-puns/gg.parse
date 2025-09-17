using gg.core.util;

using gg.parse.rulefunctions;

namespace gg.parse.ebnf
{
    public class PipelineLog
    {
        /// <summary>
        /// If set to true an exception will be thrown when a warning is encountered
        /// </summary>
        public bool FailOnWarning { get; set; } = false;

        public Action<string>? Out { get; set; } = null;

        public Func<int, RuleBase<int>>? FindAstRule { get; set; } = null;

        public void ProcessAstLogs(string text, List<Annotation> tokens, List<Annotation> astNodes) 
        {
            Contract.Requires(Out != null, "No output defined in logger, cannot process logs.");
            Contract.Requires(FindAstRule != null, "No method to locate ast rules defined, cannot process logs.");

            var logAnnotations = astNodes.Select(a=> 
                                    a.SelectNotNull( ax =>
                                        FindAstRule(ax.RuleId) is not LogRule<int> rule 
                                            ? null 
                                            : new Tuple<Annotation, LogRule<int>>(ax, rule)
                                    ));

            foreach (var logList in logAnnotations) 
            {
                foreach (var (annotation, log) in logList)
                {
                    Out!($"{log.Level}, {log.Text}: {annotation.GetText(text, tokens)}");
                }
           }
        }
    }
}
