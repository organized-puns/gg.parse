
using gg.core.util;

namespace gg.parse.basefunctions
{
    public class BasicParser<T> where T : IComparable<T>
    {
        public static readonly string ErrNoMatch = "Err_No_Match";

        private readonly List<ParseFunctionBase<T>> _functions = [];
        private readonly List<ParseFunctionBase<T>> _errors = [];

        public ParseFunctionBase<T> NoMatchError { get; init; }

        public int NextFunctionId => _functions.Count;

        /// <summary>
        /// Only initialized when GetTokenDictionary() is called.
        /// </summary>
        private Dictionary<string, int>? _tokenDictionary;

        public BasicParser()
        {
            NoMatchError = AddError(new MarkError<T>(ErrNoMatch, -1, "(Error): Cannot match token to text at the specified location"));
        }

        public BasicParser(params ParseFunctionBase<T>[] functions) : this()
        {
            foreach (var f in functions)
            {
                AddFunction(f);
            }
        }

        public List<AnnotationBase> Parse(T[] input, int start = 0)
        {
            Contract.RequiresNotNull(input);
            Contract.Requires(start >= 0 && start < input.Length);

            var result = new List<AnnotationBase>();
            var offset = 0;
            var errorStart = -1;

            while (offset < input.Length)
            {
                var annotation = TryParse(input, offset);

                if (annotation != null && annotation.Category == AnnotationDataCategory.Data)
                {
                    if (errorStart >= 0)
                    {
                        AddNoMatch(result, errorStart, offset - errorStart);
                        errorStart = -1;
                    }

                    if (_functions[annotation.FunctionId].ActionOnMatch == ProductionEnum.ProduceItem)
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

        private void AddNoMatch(List<AnnotationBase> result, int start, int length)
        {
            result.Add(new AnnotationBase(AnnotationDataCategory.Error, NoMatchError.Id, new Range(start, length)));
        }

        

        public TFunc AddFunction<TFunc>(TFunc function) where TFunc : ParseFunctionBase<T>
        {
            Contract.Requires(_functions.All(f => f.Name != function.Name),
                                $"Function with name '{function.Name}' already exists.");

            function.Id = _functions.Count;
            _functions.Add(function);
            return (TFunc) _functions[^1];
        }

        public TFunc FindFunction<TFunc>(int id) where TFunc : ParseFunctionBase<T>
        {
            Contract.Requires(id >= 0, "Function ID must be non-negative.");

            return (TFunc)_functions[id];
        }

        public ParseFunctionBase<T> FindFunctionBase(int id) => _functions[id];

        public ParseFunctionBase<T> FindFunctionBase(string name) => _functions.First( f => f.Name == name);

        public TFunc AddError<TFunc>(TFunc function) where TFunc : ParseFunctionBase<T>
        {
            Contract.Requires(_errors.All(f => f.Name != function.Name),
                                $"Function with name '{function.Name}' already exists.");

            function.Id = _errors.Count;
            _errors.Add(function);
            return (TFunc) _errors[^1];
        }

        public TFunc FindError<TFunc>(int id) where TFunc : ParseFunctionBase<T> =>
            (TFunc)FindErrorBase(id);
        

        public ParseFunctionBase<T> FindErrorBase(int id) 
        {
            Contract.Requires(id >= 0, $"Error ID ({id}) must be non-negative and less than the number of registered errors ({_errors.Count}().");

            return _errors[id];
        }

        public AnnotationBase? TryParse(T[] input, int offset)
        {
            foreach (var config in _functions)
            {
                var result = config.Parse(input, offset);

                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        public Dictionary<string, int> GetTokenDictionay()
        {
            _tokenDictionary ??= new(_functions.Select( f => new KeyValuePair<string, int>(f.Name, f.Id)));
            return _tokenDictionary;
        }
    }
}
