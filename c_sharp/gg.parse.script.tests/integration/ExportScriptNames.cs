using gg.parse.script.parser;

namespace gg.parse.script.tests.integration
{
    [TestClass]
    public class ExportScriptNames
    {
        // export the names so we address results by reference
        [TestMethod]
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
        }
    }
}
