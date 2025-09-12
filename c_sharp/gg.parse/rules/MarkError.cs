
using gg.core.util;
using System;
using System.Data;

namespace gg.parse.rulefunctions
{
    public class MarkError<T>(string name, AnnotationProduct product, string? description = null, RuleBase<T>? testFunction = null, int maxLength = 0)
        : RuleBase<T>(name, product), IRuleComposition<T>
        where T : IComparable<T>
    {
        /// <summary>
        /// Message describing the nature of the error
        /// </summary>
        public string? Message { get; } = description;

        /// <summary>
        /// Function which when matching succesfull, marks the end of this error.
        /// If no function is defined, the error will stop when the maxLength is reached
        /// </summary>
        public RuleBase<T>? TestFunction { get; set; } = testFunction;

        public IEnumerable<RuleBase<T>> Rules => [TestFunction];

        public override ParseResult Parse(T[] input, int start)
        {
            if (start < input.Length)
            {
                var index = start;

                while (index < input.Length
                    && (maxLength <= 0 || (index - start) < maxLength)
                    && (TestFunction == null || !TestFunction.Parse(input, index).FoundMatch))
                {
                    index++;
                } 

                return this.BuildDataRuleResult(new(start, index - start));
            }

            return this.BuildDataRuleResult(new(start, 0));
        }
    }
}
