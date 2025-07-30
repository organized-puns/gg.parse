namespace gg.parse.basefunctions
{
    public class MatchFunctionSequence<T>(string name, int id, ProductionEnum action, params ParseFunctionBase<T>[] sequence) 
        : ParseFunctionBase<T>(name, id, action)
        where T : IComparable<T>
    {
        public ParseFunctionBase<T>[] Sequence { get; set; } = sequence;

        public override AnnotationBase? Parse(T[] input, int start)
        {
            var index = start;

            foreach (var function in Sequence)
            {
                var result = function.Parse(input, index);
                if (result == null)
                {
                    return null;
                }

                index += result.Range.Length;
            }

            return new AnnotationBase(AnnotationDataCategory.Data, Id, new(start, index - start));
        }
    }
}
