using System.Diagnostics;

using gg.parse.script;
using gg.parse.script.parser;
using gg.parse.script.pipeline;

using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace gg.parse.argparser.tests
{
    
    /// <summary>
    /// Verify tokens are correctly parsed
    /// </summary>
    [TestClass]
    public class ArgsParserTest
    {
        static readonly string TokenFileName = "assets/args.tokens";
        static readonly string GrammarFileName = "assets/args.grammar";

        [TestMethod]
        public void SetupLoadTokensAndGrammar_ExpectNoExceptions()
        {
            var logger = new PipelineLogger()
            {
                Out = (level, message) => Debug.WriteLine(message)
            };
            
            var builder = new ParserBuilder();

            try
            {
                builder.FromFile(TokenFileName, GrammarFileName);
            }
            catch (Exception e)
            {
                // add a catch so we can inspect the logger's logs in case the grammar or tokenizers fails to 
                // parse
                Fail();
            }
        }


        [TestMethod]
        public void SetupSingleShorthand_Parse_ExpectMatch()
        {
            var builder = new ParserBuilder().FromFile(TokenFileName, GrammarFileName);
            var (tokens, syntaxTree) = builder.Parse("-v");

            IsTrue(tokens);
            IsTrue(tokens.Count == 1);
            IsTrue(tokens.MatchLength == 2);
            IsTrue(tokens[0].Rule.Name == "arg_key");

            IsTrue(syntaxTree);
            IsTrue(syntaxTree.Count == 1);
            IsTrue(syntaxTree.MatchLength == 1);

            IsTrue(syntaxTree[0].Name == "arg_option");
            IsTrue(syntaxTree[0][0].Name == "arg_key");

            var optionText = tokens[syntaxTree[0][0].Start][0].GetText("-v");

            IsTrue(optionText == "v");
        }

        [TestMethod]
        public void SetupSingleVerbose_Parse_ExpectMatch()
        {
            var builder = new ParserBuilder().FromFile(TokenFileName, GrammarFileName);
            var argText = "--value";
            var (tokens, syntaxTree) = builder.Parse(argText);

            IsTrue(tokens);
            IsTrue(tokens.Count == 1);
            IsTrue(tokens.MatchLength == argText.Length);
            IsTrue(tokens[0].Rule.Name == "arg_key");

            IsTrue(syntaxTree);
            IsTrue(syntaxTree.Count == 1);
            IsTrue(syntaxTree.MatchLength == 1);

            IsTrue(syntaxTree[0].Name == "arg_option");
            IsTrue(syntaxTree[0][0].Name == "arg_key");

            var optionText = tokens[syntaxTree[0][0].Start][0].GetText(argText);

            IsTrue(optionText == "value");
        }

        [TestMethod]
        public void SetupSingleConfigArg_ParseWithConfigArgRule_ExpectMatch()
        {
            var builder = new ParserBuilder().FromFile(TokenFileName, GrammarFileName);
            var argText = "--key=value";

            var (tokens, syntaxTree) = builder.Parse(argText);

            IsTrue(tokens);
            IsTrue(tokens[0].Name == "arg_key");
            IsTrue(tokens[1].Name == "kv_separator");
            IsTrue(tokens[2].Name == "arg_identifier");

            IsTrue(syntaxTree);
            IsTrue(syntaxTree[0].Rule.Name == "arg_option");
            IsTrue(syntaxTree[0][0].Rule.Name == "arg_key");
            IsTrue(syntaxTree[0][1].Rule.Name == "arg_value");

            var valueText = syntaxTree[0][1].GetText(argText, tokens);
            IsTrue(valueText == "value");
        }

        [TestMethod]
        public void SetupSingleRequired_Parse_ExpectMatch()
        {
            var builder = new ParserBuilder().FromFile(TokenFileName, GrammarFileName);
            var (tokens, syntaxTree) = builder.Parse("value");

            IsTrue(syntaxTree);
            IsTrue(syntaxTree[0].Rule.Name == "arg_value");
            IsTrue(syntaxTree[0][0].Rule.Name == "arg_identifier");

            var valueText = syntaxTree[0].GetText("value", tokens);
            IsTrue(valueText == "value");
        }

        [TestMethod]
        public void SetupOptionalAndRequired_Parse_ExpectMatch()
        {
            var builder = new ParserBuilder().FromFile(TokenFileName, GrammarFileName);

            var argText = "-s=123 --input=c:\\some\\file.txt command";
            var (tokens, syntaxTree) = builder.Parse(argText);

            IsTrue(tokens);
            IsTrue(tokens.Count == 10);

            IsTrue(syntaxTree);
            IsTrue(syntaxTree.Count == 3);
            IsTrue(syntaxTree.MatchLength == 10);

            IsTrue(syntaxTree[0].Name == "arg_option");
            IsTrue(syntaxTree[0][0].Name == "arg_key");
            IsTrue(syntaxTree[0][0].GetText(argText, tokens.Annotations) == "-s");
            IsTrue(syntaxTree[0][1].Name == "arg_value");
            IsTrue(syntaxTree[0][1][0].Name == "int");
            IsTrue(syntaxTree[0][1][0].GetText(argText, tokens.Annotations) == "123");

            IsTrue(syntaxTree[1].Name == "arg_option");
            IsTrue(syntaxTree[1][0].Name == "arg_key");
            IsTrue(syntaxTree[1][0].GetText(argText, tokens.Annotations) == "--input");
            IsTrue(syntaxTree[1][1].Name == "arg_value");
            IsTrue(syntaxTree[1][1][0].Name == "filename");
            IsTrue(syntaxTree[1][1][0].GetText(argText, tokens.Annotations) == "c:\\some\\file.txt");

            IsTrue(syntaxTree[2].Name == "arg_value");
            IsTrue(syntaxTree[2][0].Name == "arg_identifier");
            IsTrue(syntaxTree[2][0].GetText(argText, tokens.Annotations) == "command");
        }

        [TestMethod]
        public void SetupMultipleOptionalAndRequired_Parse_ExpectMatch()
        {
            var builder = new ParserBuilder().FromFile(TokenFileName, GrammarFileName);

            var argText = "-s=123 command1 command2 --next_option:-1.0 command3";
            var (tokens, syntaxTree) = builder.Parse(argText);

            IsTrue(syntaxTree);

            IsTrue(syntaxTree[0].Name == "arg_option");
            IsTrue(syntaxTree[0][0].GetText(argText, tokens.Annotations) == "-s");
            IsTrue(syntaxTree[0][1].GetText(argText, tokens.Annotations) == "123");

            IsTrue(syntaxTree[1].Name == "arg_value");
            IsTrue(syntaxTree[1].GetText(argText, tokens.Annotations) == "command1");

            IsTrue(syntaxTree[2].Name == "arg_value");
            IsTrue(syntaxTree[2].GetText(argText, tokens.Annotations) == "command2");

            IsTrue(syntaxTree[3].Name == "arg_option");
            IsTrue(syntaxTree[3][0].GetText(argText, tokens.Annotations) == "--next_option");
            IsTrue(syntaxTree[3][1].GetText(argText, tokens.Annotations) == "-1.0");

            IsTrue(syntaxTree[4].Name == "arg_value");
            IsTrue(syntaxTree[4].GetText(argText, tokens.Annotations) == "command3");
        }

        [TestMethod]
        public void SetupUnknownToken_Parse_ExpectScriptException()
        {
            var builder = new ParserBuilder().FromFile(TokenFileName, GrammarFileName);
            var argText = "|";


            builder.LogHandler.Out = (level, message) => Debug.WriteLine(message);

            try
            {
                builder.Parse(argText);
                Fail();
            }
            catch (ScriptException)
            {   
            }
        }

        [TestMethod]
        public void SetupUnknownGrammar_Parse_ExpectScriptException()
        {
            var builder = new ParserBuilder().FromFile(TokenFileName, GrammarFileName);
            var argText = ":boo";


            builder.LogHandler.Out = (level, message) => Debug.WriteLine(message);

            try
            {
                var (tokens, syntaxTree) = builder.Parse(argText);
                Fail();
            }
            catch (ScriptException)
            {
            }
        }
    }
}
