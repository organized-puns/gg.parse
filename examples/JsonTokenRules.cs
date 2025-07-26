using gg.parse.rules;

namespace gg.parse.examples
{
    public static class JsonTokenRules
    {
        public static IRule BooleanRule(string name = "Boolean")
        {
            return new OneOfRule(name, new LiteralRule("true"), new LiteralRule("false"));
        }

        public static IRule NullRule(string name = "Null")
        {
            return new LiteralRule(name, "null");
        }

        public static IRule ObjectStartRule(string name = "ObjectStart")
        {
            return new LiteralRule(name, "{");
        }

        public static IRule ObjectEndRule(string name = "ObjectEnd")
        {
            return new LiteralRule(name, "}");
        }

        public static IRule KeyValueSeparatorRule(string name = "KVSeparator")
        {
            return new LiteralRule(name, ":");
        }

        public static IRule ArrayStartRule(string name = "ArrayStart")
        {
            return new LiteralRule(name, "[");
        }

        public static IRule ArrayEndRule(string name = "ArrayEnd")
        {
            return new LiteralRule(name, "]");
        }

        public static IRule EnumerationSeparatorRule(string name = "EnumerationSeparator")
        {
            return new LiteralRule(name, ",");
        }



        public static Lexer CreateJsonLexer()
        {
            return CreateJsonLexerAndColorRegistry().lexer;
        }

        public static (Lexer lexer, Dictionary<string, string> colorRegistry) CreateJsonLexerAndColorRegistry()
        {
            var lexer = new Lexer();
            var colorRegistry = new Dictionary<string, string>();

            lexer.AddRule(RegisterStyleHint(colorRegistry, BasicTokensRules.FloatRule(), "color: #f78c6c;"));
            lexer.AddRule(RegisterStyleHint(colorRegistry, BasicTokensRules.IntegerRule(), "color: #ff9c7c;"));
            lexer.AddRule(RegisterStyleHint(colorRegistry, BasicTokensRules.StringRule(), "color: #c4ec8d;"));
            lexer.AddRule(RegisterStyleHint(colorRegistry, BooleanRule(), "color: #f0a0f5;"));
            lexer.AddRule(RegisterStyleHint(colorRegistry, NullRule(), "color: #e090e5; font-style: italic;"));
            lexer.AddRule(RegisterStyleHint(colorRegistry, ObjectStartRule(), "color: #f7d2fa;"));
            lexer.AddRule(RegisterStyleHint(colorRegistry, ObjectEndRule(), "color: #f7d2fa;"));
            lexer.AddRule(RegisterStyleHint(colorRegistry, KeyValueSeparatorRule(), "color: #89ddff;"));
            lexer.AddRule(RegisterStyleHint(colorRegistry, ArrayStartRule(), "color: #f7e2fa;"));
            lexer.AddRule(RegisterStyleHint(colorRegistry, ArrayEndRule(), "color: #f7e2fa;"));
            lexer.AddRule(RegisterStyleHint(colorRegistry, EnumerationSeparatorRule(), "color: #faf0fa;"));
            lexer.AddRule(BasicTokensRules.CreateWhitespaceRule(), TokenAction.IgnoreToken);

            colorRegistry.Add(Lexer.ErrorTokenName, "background-color: #FF6060; color: #101010;");

            return (lexer, colorRegistry);
        }

        private static IRule RegisterStyleHint(Dictionary<string, string> colorRegistry, IRule rule, string color)
        {
            if (colorRegistry != null)
            {
                colorRegistry[rule.Name] = color;
            }
            return rule;
        }
    }
}
