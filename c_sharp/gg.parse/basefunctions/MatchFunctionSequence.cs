namespace gg.parse.basefunctions
{
    public class MatchFunctionSequence<T>(string name, int id, ProductionEnum action, params ParseFunctionBase<T>[] sequence) 
        : ParseFunctionBase<T>(name, id, action)
        where T : IComparable<T>
    {
        public ParseFunctionBase<T>[] Sequence { get; set; } = sequence;

        public override Annotation? Parse(T[] input, int start)
        {
            var index = start;
            var children = new List<Annotation>();

            foreach (var function in Sequence)
            {
                var result = function.Parse(input, index);
                
                if (result == null)
                {
                    return null;
                }

                if (function.ActionOnMatch == ProductionEnum.ProduceItem)
                {
                    children.Add(result);
                }

                index += result.Range.Length;
            }

            return new Annotation(AnnotationDataCategory.Data, Id, new(start, index - start), children.Count > 0 ? children : null);
        }
    }
}
