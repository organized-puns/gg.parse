using gg.core.util;
using gg.parse.rulefunctions;

namespace gg.parse.compiler
{
    public class CompileContext<T> where T : IComparable<T>
    {
        public string? Text { get; set; }

        public List<Annotation>? Tokens { get; set; } 

        public List<Annotation>? AstNodes { get; set; }

        public RuleTable<int>? Parser { get; set; }
        
        //public RuleTable<T> Output { get; init; }

        public Dictionary<int, CompileFunction<T>> Functions { get; set; } = [];

        public (int functionId, AnnotationProduct product)[]? ProductLookup { get; set; }

        public CompileContext()
        {
            //Output = new RuleTable<T>();
        }

        public CompileContext(
            string text, 
            List<Annotation> tokens, 
            List<Annotation> astNodes,
            Dictionary<int, CompileFunction<T>> functions
            /*RuleTable<T>? output = null*/)
        {
            Text = text;
            Tokens = tokens;
            AstNodes = astNodes;
            //Output = output ?? new RuleTable<T>();
            Functions = functions; 
        }

        public Range GetTextRange(Range tokenRange)
        {
            Contract.RequiresNotNull(Tokens);

            var start = Tokens[tokenRange.Start].Start;
            var length = 0;

            for (var i = 0; i < tokenRange.Length; i++)
            {
                length += Tokens[tokenRange.Start + i].Length;
            }

            return new Range(start, length);
        }

        public string GetText(Range tokenRange)
        {
            var range = GetTextRange(tokenRange);
            
            return Text.Substring(range.Start, range.Length);
        }


        public CompileContext<T> WithFunction(int id, CompileFunction<T> function)
        {
            Functions.Add(id, function);
            return this;
        }

        public CompileContext<T> WithText(string text)
        {
            Text = text;
            return this;
        }

        public CompileContext<T> WithTokens(params Annotation[] tokens) => 
            WithTokens(tokens.ToList());
        

        public CompileContext<T> WithTokens(List<Annotation> tokens)
        {
            Tokens = tokens;
            return this;
        }

        public CompileContext<T> WithAstNodes(List<Annotation> nodes)
        {
            AstNodes = nodes;
            return this;
        }

        public bool TryGetProduct(int functionId, out AnnotationProduct product)
        {
            product = AnnotationProduct.Annotation;

            if (ProductLookup != null)
            {
                for (var i = 0; i < ProductLookup.Length; i++)
                {
                    if (functionId == ProductLookup[i].functionId)
                    {
                        product = ProductLookup[i].product;
                        return true;
                    }
                }
            }

            return false;
        }

    }
}
