// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

using System.Collections.Immutable;

using gg.parse.core;

using Range = gg.parse.util.Range;

namespace gg.parse.script.parser
{
    /// <summary>
    /// Class used to map an annotation to a position in the text in
    /// the form (linenumber, column)
    /// </summary>
    public class TextPositionMap
    {
        public static TextPositionMap CreateOrUpdate(TextPositionMap? current, string text) =>
            current == null || current.Text != text
                ? new TextPositionMap(text)
                : current;

        private readonly List<Range> _lineRanges;

        public string Text
        {
            get;
            init;
        }

        public TextPositionMap(string text)
        {
            Text = text;
            _lineRanges = CollectLineRanges(text);
        }
        public (int line, int column) GetTokenPosition(Annotation token) =>
            MapRangeToLineColumn(token.Range, _lineRanges);

        public (int line, int column) GetGrammarPosition(Annotation grammarNode, ImmutableList<Annotation> tokens) =>
            MapAnnotationRangeToLineColumn(grammarNode, tokens, _lineRanges);

        private static List<Range> CollectLineRanges(string text)
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

        public static (int line, int column) MapRangeToLineColumn(Range textRange, List<Range> lineRanges)
        {
            int line;

            for (line = 0; line < lineRanges.Count; line++)
            {
                if (textRange.Start >= lineRanges[line].Start && textRange.Start <= lineRanges[line].End)
                {
                    break;
                }
            }

            return (line + 1, textRange.Start - lineRanges[line].Start + 1);
        }

        private static (int line, int column) MapAnnotationRangeToLineColumn(
            Annotation annotation,
            ImmutableList<Annotation> tokens,
            List<Range> lineRanges) =>

           MapRangeToLineColumn(tokens.CombinedRange(annotation.Range), lineRanges);
    }
}
