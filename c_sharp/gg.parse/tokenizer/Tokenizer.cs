using gg.core.util;

namespace gg.parse.tokenizer
{    
    public class Tokenizer
    {
        public static readonly string ErrNoMatch = "Err_No_Match";

        private readonly List<TokenFunction> _functions = [];
        private readonly List<TokenFunction> _errors = [];

        public TokenFunction NoMatchError { get; init; }
            
        public int NextFunctionId => _functions.Count;

        public TokenFunction this[int id] => _functions[id];

        public Tokenizer()
        {
            NoMatchError = AddError(new MarkErrorFunction(ErrNoMatch, -1, "(Error): Cannot match token to text at the specified location"));
        }



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

                if (annotation != null && annotation.Category == AnnotationCategory.Token)
                {
                    if (errorStart >= 0)
                    {
                        AddNoMatch(result, errorStart, offset - errorStart);
                        errorStart = -1;
                    }

                    if (_functions[annotation.ReferenceId].ActionOnMatch == ProductionEnum.ProduceItem)
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
                AddNoMatch(result, errorStart, offset - errorStart);
            }

            return result;
        }

        private void AddNoMatch(List<Annotation> result, int start, int length)
        {
            result.Add(new Annotation(AnnotationCategory.Error, NoMatchError.Id, new Range(start, length)));
        }

        public T GetFunction<T>(int id) where T : TokenFunction
        {
            Contract.Requires(id >= 0, "Configuration ID must be non-negative.");

            return (T) _functions[id];
        }

        public TokenFunction AddFunction(TokenFunction function)
        {
            Contract.Requires(_functions.All(f => f.Name != function.Name), 
                                $"Function with name '{function.Name}' already exists.");

            function.Id = _functions.Count;
            _functions.Add(function);
            return _functions[^1];
        }

        public TokenFunction FindFunction(int id) => _functions[id];

        public TokenFunction AddError(TokenFunction function)
        {
            Contract.Requires(_errors.All(f => f.Name != function.Name),
                                $"Function with name '{function.Name}' already exists.");

            function.Id = _errors.Count;
            _errors.Add(function);
            return _errors[^1];
        }

        public TokenFunction FindError(int id) => _errors[id];

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
