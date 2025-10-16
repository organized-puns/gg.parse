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

        // export the names so we address results by reference
        /*[TestMethod]
        public void ExportNames()
        {
            var builder = new ParserBuilder().FromFile(TokenFileName, GrammarFileName);

            var output = ScriptUtils.ExportNames(builder.TokenGraph, builder.GrammarGraph, "gg.parse.argparser", "ArgParserNames");

            File.WriteAllText("ArgParserNames.cs", output);
        }*/
            

        [TestMethod]
        public void SetupLoadTokensAndGrammar_ExpectNoExceptions()
        {
            var logger = new ScriptLogger()
            {
                Out = (level, message) => Debug.WriteLine(message)
            };
            
            var builder = new ParserBuilder();

            try
            {
                builder.FromFile(TokenFileName, GrammarFileName, logger: logger);
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
            IsTrue(tokens.Count == 2);
            IsTrue(tokens.MatchLength == 2);
            IsTrue(tokens[0].Rule.Name == ArgParserNames.ArgKeySwitch);
            IsTrue(tokens[1].Rule.Name == ArgParserNames.Identifier);

            IsTrue(syntaxTree);
            IsTrue(syntaxTree.Count == 1);
            IsTrue(syntaxTree.MatchLength == 2);

            IsTrue(syntaxTree[0].Name == ArgParserNames.ArgOption);
            IsTrue(syntaxTree[0][0].Name == ArgParserNames.ArgKey);
            var identifierNode = syntaxTree[0][0][1];
            IsTrue(identifierNode.Name == ArgParserNames.Identifier);

            var optionText = identifierNode.GetText("-v", tokens);

            IsTrue(optionText == "v");
        }

        [TestMethod]
        public void SetupSingleVerbose_Parse_ExpectMatch()
        {
            var builder = new ParserBuilder().FromFile(TokenFileName, GrammarFileName);
            var argText = "--value";
            var (tokens, syntaxTree) = builder.Parse(argText);

            IsTrue(tokens);
            IsTrue(tokens.Count == 2);
            IsTrue(tokens.MatchLength == argText.Length);

            IsTrue(syntaxTree);
            IsTrue(syntaxTree.Count == 1);
            IsTrue(syntaxTree.MatchLength == 2);

            IsTrue(syntaxTree[0].Name == ArgParserNames.ArgOption);
            IsTrue(syntaxTree[0][0].Name == ArgParserNames.ArgKey);
        }

        [TestMethod]
        public void SetupSingleConfigArg_ParseWithConfigArgRule_ExpectMatch()
        {
            var builder = new ParserBuilder().FromFile(TokenFileName, GrammarFileName);
            var argText = "--key=value";

            var (tokens, syntaxTree) = builder.Parse(argText);

            IsTrue(tokens);
            IsTrue(tokens[0].Name == ArgParserNames.ArgKeySwitch);
            IsTrue(tokens[1].Name == ArgParserNames.Identifier);
            IsTrue(tokens[2].Name == ArgParserNames.KvSeparator);
            IsTrue(tokens[3].Name == ArgParserNames.Identifier);

            IsTrue(syntaxTree);
            IsTrue(syntaxTree[0].Rule.Name == ArgParserNames.ArgOption);
            IsTrue(syntaxTree[0][0].Rule.Name == ArgParserNames.ArgKey);
            IsTrue(syntaxTree[0][1].Rule.Name == ArgParserNames.ArgValue);

            var valueText = syntaxTree[0][1].GetText(argText, tokens);
            IsTrue(valueText == "value");
        }

        [TestMethod]
        public void SetupSingleRequired_Parse_ExpectMatch()
        {
            var builder = new ParserBuilder().FromFile(TokenFileName, GrammarFileName);
            var (tokens, syntaxTree) = builder.Parse("value");

            IsTrue(syntaxTree);
            IsTrue(syntaxTree[0].Rule.Name == ArgParserNames.ArgValue);
            IsTrue(syntaxTree[0][0].Rule.Name == ArgParserNames.Identifier);

            var valueText = syntaxTree[0].GetText("value", tokens);
            IsTrue(valueText == "value");
        }

        [TestMethod]
        public void SetupOptionalAndRequired_Parse_ExpectMatch()
        {
            var builder = new ParserBuilder().FromFile(TokenFileName, GrammarFileName);

            var argText = "-s=123 --input=c:\\some\\file.txt command";
            //var argText = "--input=c:\\some\\file.txt";
            var (tokens, syntaxTree) = builder.Parse(argText);

            //var filenameRule = builder.GrammarGraph.FindRule("filename");
            //var res = filenameRule.Parse(tokens.SelectRuleIds(), 3);
            //var resText = res[0].GetText(argText, tokens);

            IsTrue(tokens);

            IsTrue(syntaxTree);
            IsTrue(syntaxTree.Count == 3);

            IsTrue(syntaxTree[0].Name == ArgParserNames.ArgOption);
            IsTrue(syntaxTree[0][0].Name == ArgParserNames.ArgKey);
            IsTrue(syntaxTree[0][0].GetText(argText, tokens.Annotations) == "-s");
            IsTrue(syntaxTree[0][1].Name == ArgParserNames.ArgValue);
            IsTrue(syntaxTree[0][1][0].Name == ArgParserNames.Int);
            IsTrue(syntaxTree[0][1][0].GetText(argText, tokens.Annotations) == "123");

            IsTrue(syntaxTree[1].Name == ArgParserNames.ArgOption);
            IsTrue(syntaxTree[1][0].Name == ArgParserNames.ArgKey);
            IsTrue(syntaxTree[1][0].GetText(argText, tokens.Annotations) == "--input");
            IsTrue(syntaxTree[1][1].Name == ArgParserNames.ArgValue);
            IsTrue(syntaxTree[1][1][0].Name == ArgParserNames.Filename);
            IsTrue(syntaxTree[1][1][0].GetText(argText, tokens.Annotations) == "c:\\some\\file.txt");

            IsTrue(syntaxTree[2].Name == ArgParserNames.ArgValue);
            IsTrue(syntaxTree[2][0].Name == ArgParserNames.Identifier);
            IsTrue(syntaxTree[2][0].GetText(argText, tokens.Annotations) == "command");
        }

        [TestMethod]
        public void SetupMultipleOptionalAndRequired_Parse_ExpectMatch()
        {
            var builder = new ParserBuilder().FromFile(TokenFileName, GrammarFileName);

            var argText = "-s=123 command1 command2 --next_option:-1.0 command3";
            var (tokens, syntaxTree) = builder.Parse(argText);

            IsTrue(syntaxTree);

            IsTrue(syntaxTree[0].Name == ArgParserNames.ArgOption);
            IsTrue(syntaxTree[0][0].GetText(argText, tokens.Annotations) == "-s");
            IsTrue(syntaxTree[0][1].GetText(argText, tokens.Annotations) == "123");

            IsTrue(syntaxTree[1].Name == ArgParserNames.ArgValue);
            IsTrue(syntaxTree[1].GetText(argText, tokens.Annotations) == "command1");

            IsTrue(syntaxTree[2].Name == ArgParserNames.ArgValue);
            IsTrue(syntaxTree[2].GetText(argText, tokens.Annotations) == "command2");

            IsTrue(syntaxTree[3].Name == ArgParserNames.ArgOption);
            IsTrue(syntaxTree[3][0].GetText(argText, tokens.Annotations) == "--next_option");
            IsTrue(syntaxTree[3][1].GetText(argText, tokens.Annotations) == "-1.0");

            IsTrue(syntaxTree[4].Name == ArgParserNames.ArgValue);
            IsTrue(syntaxTree[4].GetText(argText, tokens.Annotations) == "command3");
        }

        [TestMethod]
        public void SetupObject_Parse_ExpectMatch()
        {
            var builder = new ParserBuilder().FromFile(TokenFileName, GrammarFileName);

            var argText = "-s={}";
            var (tokens, syntaxTree) = builder.Parse(argText);

            IsTrue(syntaxTree);

            IsTrue(syntaxTree[0].Name == ArgParserNames.ArgOption);
            IsTrue(syntaxTree[0][0].GetText(argText, tokens.Annotations) == "-s");
            IsTrue(syntaxTree[0][1].GetText(argText, tokens.Annotations) == "{}");
            
            // without key - values we can't determine if it's an object or dictionary
            // since the dictionary goes before the object it will resolve to a dictionary
            IsTrue(syntaxTree[0][1][0] == ArgParserNames.Dictionary);
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
