
namespace gg.parse.rulefunctions
{
    public class MatchDataSequence<T>(string name, T[] dataArray, AnnotationProduction production = AnnotationProduction.Annotation)
        : RuleBase<T>(name, production)
        where T : IComparable<T>
    {
        public T[] DataArray { get; } = dataArray;

        public override ParseResult Parse(T[] input, int start)
        {
            var index = start;

            for (var i = 0; i < DataArray.Length; i++)
            {
                if (index >= input.Length || input[index].CompareTo(DataArray[i]) != 0)
                {
                    return ParseResult.Failure;
                }
                index++;
            }

            return this.BuildDataRuleResult(new(start, index - start));
        }
    }
}
