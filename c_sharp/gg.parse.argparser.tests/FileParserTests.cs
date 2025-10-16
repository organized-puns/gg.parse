using System.Diagnostics;

using gg.parse.script;
using gg.parse.script.pipeline;

using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace gg.parse.argparser.tests
{
    [TestClass]
    public class FileParserTests
    {
        static readonly string TokenFileName = "assets/filename.tokens";
        static readonly string GrammarFileName = "assets/filename.grammar";


        [TestMethod]
        public void SetupLoadTokensAndGrammar_ExpectNoExceptions()
        {
            var logger = new ScriptLogger()
            {
                Out = (level, message) => Debug.WriteLine(message)
            };
            var receivedLogs = logger.ReceivedLogs;
            var builder = new ParserBuilder();

            try
            {
                builder.FromFile(TokenFileName, GrammarFileName, logger);
            }
            catch (Exception)
            {
                // add a catch so we can inspect the logger's logs in case the grammar or tokenizers fails to 
                // parse
                Fail();
            }
        }

        [TestMethod]
        public void CreateAbsoluteWindowsDirectory_Parse_ExpectFullPathFound()
        { 
            var builder = new ParserBuilder().FromFile(TokenFileName, GrammarFileName);

            var filename = "c:\\my\\path.exe";
            var (tokensResult, syntaxTreeResult) = builder.Parse(filename);

            IsTrue(syntaxTreeResult);

            IsTrue(syntaxTreeResult[0] == "filename");
            IsTrue(syntaxTreeResult[0][0] == "drive");
            IsTrue(syntaxTreeResult[0][0][0] == "letter");

            var outputText = syntaxTreeResult[0][0].GetText(filename, tokensResult.Annotations);
            IsTrue(outputText == "c:\\");

            var path = syntaxTreeResult[0][1];

            IsTrue(path.Name == "path");
            IsTrue(path.GetText(filename, tokensResult.Annotations) == "my\\path.exe");            

            IsTrue(path[0].Name == "path_part");
            IsTrue(path[0].GetText(filename, tokensResult.Annotations) == "my");

            IsTrue(path[1].Name == "path_part");
            IsTrue(path[1].GetText(filename, tokensResult.Annotations) == "path.exe");
        }

        [TestMethod]
        public void CreateRelativeNixFile_Parse_ExpectFullPathFound()
        {
            var builder = new ParserBuilder().FromFile(TokenFileName, GrammarFileName);

            var filename = "../parent/file.txt";
            var (tokensResult, syntaxTreeResult) = builder.Parse(filename);

            IsTrue(syntaxTreeResult);

            IsTrue(syntaxTreeResult[0].Name == "filename");
            var path = syntaxTreeResult[0][0];

            IsTrue(path.Name == "path");
            
            IsTrue(path[0].Name == "path_part");
            IsTrue(path[0][0].Name == "parent_dir");
            IsTrue(path[0].GetText(filename, tokensResult.Annotations) == "..");

            IsTrue(path[1].Name == "path_part");
            IsTrue(path[1].GetText(filename, tokensResult.Annotations) == "parent");

            IsTrue(path[2].Name == "path_part");
            var partText = path[2].GetText(filename, tokensResult.Annotations);
            IsTrue(partText == "file.txt");            
        }
    }
}
