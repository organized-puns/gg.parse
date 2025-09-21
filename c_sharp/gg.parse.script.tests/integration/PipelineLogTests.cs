#nullable disable

using gg.parse.script.pipeline;

using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace gg.parse.script.tests.integration
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
                Out = (l, s) => result.Add(s),
            };

            // this should trigger a warning (because the rule is empty)
            var pipeline = new RuleGraphBuilder()
                            .InitializeFromDefinition("foo='bar';", "trigger_warning=;\ntrigger_warning_2=;", logger);

            // logger should have written two warnings to the output
            IsTrue(result.Count == 2);
        }

        [TestMethod]
        public void SetupPipelineLog_CreatePipeline_ExpectLogsToContainTokenWarnings()
        {
            var result = new List<string>();

            var logger = new PipelineLog()
            {
                Out = (level, message) => result.Add(message),
            };

            // this should trigger a warning (because the rule is empty)
            var pipeline = new RuleGraphBuilder()
                            .InitializeFromDefinition("\n\r\nfoo=;", null, logger);

            // logger should have written two warnings to the output
            IsTrue(result.Count == 1);
        }


        [TestMethod]
        [ExpectedException(typeof(ScriptPipelineException))]
        public void SetupPipelineLogWithFailOnWarning_CreatePipeline_ExpectException()
        {
            var result = new List<string>();

            var logger = new PipelineLog()
            {
                Out = (level, message) => result.Add(message),
                FailOnWarning = true
            };

            // this should trigger an exception
            new RuleGraphBuilder()
                    .InitializeFromDefinition("foo='bar';", "trigger_warning=;", logger);
        }
    }
}
