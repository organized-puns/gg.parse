using gg.parse.basefunctions;

namespace gg.parse.rulefunctions
{
    public static class RuleTableExtensions
    {
        public static RuleBase<char> Digit(
            this RuleTable<char> table, string? name = null, AnnotationProduction action = AnnotationProduction.Annotation)
        {
            // {'0'..'9'}
            var ruleName = name ?? TokenNames.Digit;

            if (table.TryFindRule(ruleName, out MatchDataRange<char>? existingRule))
            {
                return existingRule!;
            }

            return table.RegisterRule(
                new MatchDataRange<char>(ruleName, '0', '9', action));
        }

        public static RuleBase<char> DigitSequence(
            this RuleTable<char> table, string? name = null, AnnotationProduction action = AnnotationProduction.Annotation)
        {
            // {'0'..'9'}+
            var ruleName = name ?? TokenNames.DigitSequence;

            if (table.TryFindRule(ruleName, out MatchFunctionCount<char>? existingRule))
            {
                return existingRule!;
            }

            var digitRule = Digit(table, $"~{TokenNames.Digit}", AnnotationProduction.None);

            return table.RegisterRule(
                new MatchFunctionCount<char>(ruleName, digitRule, action, 1, 0));
        }
    }
}
