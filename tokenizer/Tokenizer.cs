namespace gg.parse.tokenizer
{    
    public class Tokenizer
    {
        private readonly List<TokenFunction> _functions = [];

        public TokenFunction NoMatchError { get; set; } = 
            new MarkErrorFunction("Err_No_Match", -1, "(Error): Cannot match token to text at the specified location");

        public int NextFunctionId => _functions.Count;

        public TokenFunction this[int id] => _functions[id];

        public List<Annotation> Tokenize(string text, int start = 0)
        {
            if (text == null)
            {
                throw new ArgumentNullException(nameof(text), "Text cannot be null");
            }

            if (start < 0 || start >= text.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(start), "Start index is out of range.");
            }

            var result = new List<Annotation>();
            var offset = 0;
            var errorStart = -1;

            while (offset < text.Length)
            {
                var annotation = TryParse(text, offset);

                if (annotation != null)
                {
                    if (errorStart >= 0)
                    {
                        result.Add(new Annotation(AnnotationCategory.Error, NoMatchError.Id, new Range(errorStart, offset - errorStart)));
                        errorStart = -1;
                    }

                    if (_functions[annotation.FunctionId].ActionOnMatch == TokenAction.GenerateToken)
                    {
                        result.Add(annotation);
                    }

                    offset += annotation.Range.Length;
                }
                else
                {
                    if (errorStart < 0)
                    {
                        errorStart = offset;
                    }
                    offset++;
                }
            }

            // are we still in an error state?
            if (errorStart >= 0)
            {
                result.Add(new Annotation(AnnotationCategory.Error, NoMatchError.Id, new Range(errorStart, offset - errorStart)));
            }

            return result;
        }

        public T GetFunction<T>(int id) where T : TokenFunction
        {
            if (id < 0 || id >= _functions.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(id), "Invalid configuration ID.");
            }

            return (T) _functions[id];
        }

        public TokenFunction AddFunction(TokenFunction function)
        {
            _functions.Add(function);
            function.Id = _functions.Count;
            return _functions[^1];
        }

        public Annotation? TryParse(string text, int offset)
        {
            // _configs.FirstOherDefault(config => config.Parse(text, offset)); ?
            foreach (var config in _functions)
            {
                var result = config.Parse(text, offset);

                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }
    }
}
