using gg.core.util;
using gg.parse.rulefunctions;

namespace gg.parse.rulefunctions.rulefunctions
{
    public class MatchFunctionSequence<T> : RuleBase<T>, IRuleComposition<T>  where T : IComparable<T>
    {
        private RuleBase<T>[] _sequence;

        public RuleBase<T>[] SequenceSubfunctions
        {
            get => _sequence;
            set
            {
                Contract.Requires(value != null);
                Contract.Requires(value!.Any( v => v != null));

                _sequence = value!;
            }
        }


        public IEnumerable<RuleBase<T>> Rules => SequenceSubfunctions;

        public MatchFunctionSequence(
            string name, 
            AnnotationProduct production = AnnotationProduct.Annotation, 
            int precedence = 0,
            params RuleBase<T>[] sequence
        ) : base(name, production, precedence) 
        {
            SequenceSubfunctions = sequence;
        }

        public override ParseResult Parse(T[] input, int start)
        {
            var index = start;
            List<Annotation>? children = null;

            foreach (var function in SequenceSubfunctions)
            {
                var result = function.Parse(input, index);
                
                if (!result.FoundMatch)
                {
                    return ParseResult.Failure;
                }

                if (result.Annotations != null && result.Annotations.Count > 0 &&
                   (Production == AnnotationProduct.Annotation || Production == AnnotationProduct.Transitive))
                {
                    children ??= [];
                    children.AddRange(result.Annotations);
                }

                index += result.MatchedLength;
            }

            return this.BuildFunctionRuleResult(new Range(start, index - start), children);
        }
    }
}
