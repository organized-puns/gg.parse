
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

            // this should trigger a warning (because the rule is empty)
            var pipeline = new ScriptPipeline("foo='bar';", "trigger_warning=;\ntrigger_warning_2=;", logger);

            // logger should have written two warnings to the output
            IsTrue(result.Count == 2);
        }

        [TestMethod]
        public void SetupPipelineLog_CreatePipeline_ExpectLogsToContainTokenWarnings()
        {
            var result = new List<string>();

            var logger = new PipelineLog()
            {
                Out = (s) => result.Add(s),
            };

            // this should trigger a warning (because the rule is empty)
            var pipeline = new ScriptPipeline("\n\r\nfoo=;", null, logger);

            // logger should have written two warnings to the output
            IsTrue(result.Count == 1);
        }


        [TestMethod]
        [ExpectedException(typeof(EbnfException))]
        public void SetupPipelineLogWithFailOnWarning_CreatePipeline_ExpectException()
        {
            var result = new List<string>();

            var logger = new PipelineLog()
            {
                Out = (s) => result.Add(s),
                FailOnWarning = true
            };

            // this should trigger an exception
            new ScriptPipeline("foo='bar';", "trigger_warning=;", logger);
        }
    }
}
