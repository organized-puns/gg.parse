using gg.core.util;
using gg.parse.rulefunctions;

namespace gg.parse.compiler
{
    public class CompileSession<T> where T : IComparable<T>
    {
        public string? Text { get; set; }

        public List<Annotation>? Tokens { get; set; } 

        public List<Annotation>? AstNodes { get; set; }      
        
        public (int functionId, AnnotationProduct product)[]? ProductLookup { get; set; }

        public CompileSession()
        {
        }

        public CompileSession(
            string text, 
            List<Annotation> tokens, 
            List<Annotation> astNodes)

        {
            Text = text;
            Tokens = tokens;
            AstNodes = astNodes;
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
        public CompileSession<T> WithText(string text)
        {
            Text = text;
            return this;
        }

        public CompileSession<T> WithTokens(params Annotation[] tokens) => 
            WithTokens(tokens.ToList());
        

        public CompileSession<T> WithTokens(List<Annotation> tokens)
        {
            Tokens = tokens;
            return this;
        }

        public CompileSession<T> WithAstNodes(List<Annotation> nodes)
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
