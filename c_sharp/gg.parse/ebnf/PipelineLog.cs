using gg.core.util;

using gg.parse.rulefunctions;
using static System.Net.Mime.MediaTypeNames;

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

            var logAnnotations = astNodes.Select(a => 
                                    a!.SelectNotNull( ax =>
                                        FindAstRule(ax.RuleId) is LogRule<int> rule 
                                            ? new Tuple<Annotation, LogRule<int>>(ax, rule)
                                            : null
                                    ));

            var lineRanges = CollectLineRanges(text);

            foreach (var logList in logAnnotations) 
            {
                foreach (var (annotation, log) in logList)
                {
                    var (line, column) = MapAnnotationRangeToLineColumn(annotation, text, tokens, lineRanges);
                    Out!($"({line}, {column}) {log.Level}, {log.Text}: {GetAnnotationText(annotation, text, tokens)}");
                }
            }
        }

        private string GetAnnotationText(Annotation annotation, string text, List<Annotation> tokens, int minStringLength = 8, int maxStringLength = 60)
        {
            Contract.Requires(minStringLength < maxStringLength);
            Contract.Requires(maxStringLength > 4);

            var annotationText = annotation.GetText(text, tokens);

            if (annotationText.Length < minStringLength)
            {
                if (annotation.Parent != null)
                {
                    return GetAnnotationText(annotation.Parent, text, tokens, minStringLength, maxStringLength);
                }

                return annotationText;
            }

            if (annotation.Length >= maxStringLength)
            {
                return annotationText.Substring(maxStringLength - 4) + " ...";
            }

            return annotationText;
        }

        private List<Range> CollectLineRanges(string text)
        {
            var result = new List<Range>();
            var start = 0;

            for (var i = 0; i < text.Length; i++)
            {
                if (text[i] == '\n')
                {
                    result.Add(new Range(start, i - start));
                    start = i + 1;
                }
            }

            result.Add(new(start, text.Length - start));

            return result;
        }

        private (int line, int column) MapAnnotationRangeToLineColumn(Annotation annotation, string text, List<Annotation> tokens, List<Range> lineRanges)
        {
            var textRange = tokens.UnionOfRanges(annotation.Range);
            var line = 0;
            var column = 0;

            for (line = 0; line < lineRanges.Count; line++)
            {
                if (textRange.Start >= lineRanges[line].Start && textRange.Start <= lineRanges[line].End)
                {
                    break;
                }
            }

            return (line + 1, (textRange.Start - lineRanges[line].Start) + 1);
        }
    }
}
