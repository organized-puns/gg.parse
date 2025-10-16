using gg.parse.script.parser;

namespace gg.parse.script.tests.parserbuilder
{
    [TestClass]
    public class ExportScriptNames
    {
        // xxx apply to common tokenizer / parser
        // export the names so we address results by reference
        // only turn this on when needed
        /*[TestMethod]
        public void ExportNames()
        {
            File.WriteAllText("ScriptParserNames.cs",
                ScriptUtils.ExportNames(
                    new ScriptTokenizer(), 
                    new ScriptParser(), 
                    "gg.parse.script.parser", 
                    "ScriptParserNames"
                )
            );
        }*/
    }
}
