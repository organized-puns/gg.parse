
using gg.parse.ebnf;

using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace gg.parse.tests.ebnf
{
    [TestClass]
    public class PipelineLogTests
    {
        [TestMethod]
        public void SetupPipelineLog_CreatePipeline_ExpectLogsToHoldData()
        {
            var result = new List<string>();
            
            var logger = new PipelineLog()
            {
                Out = (s) => result.Add(s),
            };

            // this should trigger a warning
            var pipeline = new ScriptPipeline("foo='bar';", "trigger_warning=;\ntrigger_warning_2=;", logger);

            IsTrue(result.Count == 2);
        }
    }
}
