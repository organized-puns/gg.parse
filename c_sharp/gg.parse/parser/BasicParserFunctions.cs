using gg.parse.basefunctions;


namespace gg.parse.parser
{
    public class BasicParserFunctions
    {
        private BasicTokenizer<char> _tokenizer;

        private Dictionary<string, int> _tokenDictionary;

        public BasicParserFunctions(BasicTokenizer<char> tokenizer)
        {
            _tokenizer = tokenizer;
            _tokenDictionary = _tokenizer.GetTokenDictionary();
        }

        public static ParseFunctionBase<int> Token(string name, int tokenId, ProductionEnum action = ProductionEnum.ProduceItem)
        {
            return new MatchDataSet<int>(name ?? $"Token({tokenId})", -1, action, [tokenId]);
        }

        public ParseFunctionBase<int> Sequence(string name, int id, ProductionEnum action, params string[] tokenNames)
        {
            return new MatchDataSequence<int>(name ?? $"{TokenNames.FunctionSequence}(..)", id, action, tokenNames.Select( n => _tokenDictionary[n]).ToArray());
        }
    }
}
