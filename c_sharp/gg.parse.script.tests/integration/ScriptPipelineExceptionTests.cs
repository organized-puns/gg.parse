using gg.parse.rulefunctions;
using gg.parse.script.parser;
using gg.parse.script.pipeline;

using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace gg.parse.script.tests.integration
{
    [TestClass]
    public class ScriptPipelineExceptionTests
    {
        [TestMethod]
        public void TwoInvalidTokensInTokenizer_CreateParser_ExpectException()
        {
            var parser = new RuleGraphBuilder();

            try
            {
                // & and ^ are no valid tokens, so this should raise an exception
                parser.InitializeFromDefinition("& foo ^");
                Fail();
            }
            catch (ScriptPipelineException pipelineException)
            {
                var e = pipelineException.InnerException as TokenizeException;

                IsTrue(e.Errors != null);
                IsTrue(e.Errors.Count() == 2);
                IsTrue(e.Errors.ElementAt(0).Start == 0);
                IsTrue(e.Errors.ElementAt(0).Length == 2);
                IsTrue(e.Errors.ElementAt(1).Start == 6);
                IsTrue(e.Errors.ElementAt(1).Length == 1);

                IsTrue(parser
                        .LogHandler!
                        .ReceivedLogs!
                        .Where(entry => entry.level == LogLevel.Fatal)
                        .Count() == 1);

                IsTrue(parser
                        .LogHandler!
                        .ReceivedLogs!
                        .Where( entry => entry.level == LogLevel.Error)
                        .Count() == 2);
            }
        }

        [TestMethod]
        public void SetupValidTokenInTheWrongPlace_CreateParser_ExpectException()
        {
            var parser = new RuleGraphBuilder();

            try
            {
                // , is a valid token, just not a valid expression for the body
                parser.InitializeFromDefinition(" foo=,;");
                Fail();
            }
            catch (ScriptPipelineException pipelineException)
            {
                var e = pipelineException.InnerException as ParseException;

                IsTrue(e.Errors != null);
                IsTrue(e.Errors.Count() == 1);
                IsTrue(e.Errors.ElementAt(0).Start == 2);
                IsTrue(e.Errors.ElementAt(0).Length == 1);

                IsTrue(parser
                        .LogHandler!
                        .ReceivedLogs!
                        .Where(entry => entry.level == LogLevel.Fatal)
                        .Count() == 1);

                IsTrue(parser
                        .LogHandler!
                        .ReceivedLogs!
                        .Where(entry => entry.level == LogLevel.Error)
                        .Count() == 1);
            }
        }

        [TestMethod]
        public void SetupInvalidHeaderProduction_InitializeParser_ExpectException()
        {
            var parser = new RuleGraphBuilder();

            try
            {
                // . is a valid token, just not a valid expression in the header
                parser.InitializeFromDefinition(".foo='bar';");
                Fail();
            }
            catch (ScriptPipelineException pipelineException)
            {
                // at this point we don't have access to the parser used anymore. 
                // xxx this is not ideal because we're making assumptions about the rule
                // later on.
                var scriptParser = new ScriptParser();
                var e = pipelineException.InnerException as ParseException;

                IsTrue(e.Errors != null);
                IsTrue(e.Errors.Count() == 1);

                var error = e.Errors.ElementAt(0);

                IsTrue(error.Start == 0);
                IsTrue(error.Length == 1);
                IsTrue(error.Rule.Id == scriptParser.InvalidProductInHeaderError.Id);

                IsTrue(parser
                        .LogHandler!
                        .ReceivedLogs!
                        .Where(entry => entry.level == LogLevel.Fatal)
                        .Count() == 1);

                IsTrue(parser
                        .LogHandler!
                        .ReceivedLogs!
                        .Where(entry => entry.level == LogLevel.Error)
                        .Count() == 1);
            }
        }

        [TestMethod]
        public void SetupInvalidPrecedence_InitializeParser_ExpectException()
        {
            var parser = new RuleGraphBuilder();

            try
            {
                // . is a valid token, just not a valid expression in the header
                parser.InitializeFromDefinition("foo number ='bar';");
                Fail();
            }
            catch (ScriptPipelineException pipelineException)
            {
                // at this point we don't have access to the parser used anymore. 
                // xxx this is not ideal because we're making assumptions about the rule
                // later on.
                var scriptParser = new ScriptParser();
                var e = pipelineException.InnerException as ParseException;

                IsTrue(e.Errors != null);
                IsTrue(e.Errors.Count() == 1);

                var error = e.Errors.ElementAt(0);

                IsTrue(error.Start == 1);
                IsTrue(error.Length == 1);
                IsTrue(error.Rule.Id == scriptParser.InvalidPrecedenceError.Id);

                IsTrue(parser
                        .LogHandler!
                        .ReceivedLogs!
                        .Where(entry => entry.level == LogLevel.Fatal)
                        .Count() == 1);

                IsTrue(parser
                        .LogHandler!
                        .ReceivedLogs!
                        .Where(entry => entry.level == LogLevel.Error)
                        .Count() == 1);
            }
        }

        [TestMethod]
        public void InvalidTokenGrammar_CreateParser_ExpectException()
        {
            var parser = new RuleGraphBuilder();

            try
            {
                // first rule has no terminating ;
                // second rule is fine
                // third rule has no assignment, =
                parser.InitializeFromDefinition("rule1 = 'foo' rule2 = 'bar'; rule3 'qaz';");
                Fail();
            }
            catch (ScriptPipelineException pipelineException)
            {
                var e = pipelineException.InnerException as ParseException;

                IsTrue(e.Errors != null);
                IsTrue(e.Errors.Count() == 2);
                IsTrue(e.Errors.ElementAt(0).Start == 3);
                IsTrue(e.Errors.ElementAt(0).Length == 0);
                IsTrue(e.Errors.ElementAt(1).Start == 8);
                IsTrue(e.Errors.ElementAt(1).Length == 0);

                IsTrue(parser
                        .LogHandler!
                        .ReceivedLogs!
                        .Where(entry => entry.level == LogLevel.Fatal)
                        .Count() == 1);

                IsTrue(parser
                        .LogHandler!
                        .ReceivedLogs!
                        .Where(entry => entry.level == LogLevel.Error)
                        .Count() == 2);
            }
        }

        [TestMethod]
        public void InvalidTokensInGrammar_CreateParser_ExpectException()
        {
            var parser = new RuleGraphBuilder();

            try
            {
                // ^ and : are no valid tokens, so this should raise an exception
                // while tokenizing the grammar
                parser.InitializeFromDefinition("foo='string';", "^ bar=foo; :");
                Fail();
            }
            catch (ScriptPipelineException pipelineException)
            {
                var e = pipelineException.InnerException as TokenizeException;

                IsTrue(e.Errors != null);
                IsTrue(e.Errors.Count() == 2);
                IsTrue(e.Errors.ElementAt(0).Start == 0);
                IsTrue(e.Errors.ElementAt(0).Length == 2);
                IsTrue(e.Errors.ElementAt(1).Start == 11);
                IsTrue(e.Errors.ElementAt(1).Length == 1);

                IsTrue(parser
                        .LogHandler!
                        .ReceivedLogs!
                        .Where(entry => entry.level == LogLevel.Fatal)
                        .Count() == 1);

                IsTrue(parser
                        .LogHandler!
                        .ReceivedLogs!
                        .Where(entry => entry.level == LogLevel.Error)
                        .Count() == 2);
            }
        }

        [TestMethod]
        public void CreateSpecWithMissingRuleInGrammar_CreateParser_ExpectException()
        {
            var parser = new RuleGraphBuilder();

            try
            {
                parser.InitializeFromDefinition("foo='string';", "bar=foo");
                Fail();
            }
            catch (ScriptPipelineException pipelineException)
            {
                var e = pipelineException.InnerException as ParseException;

                IsTrue(e.Errors != null);
                IsTrue(e.Errors.Count() == 1);
                IsTrue(e.Errors.ElementAt(0).Start == 3);
                IsTrue(e.Errors.ElementAt(0).Length == 0);

                IsTrue(parser
                    .LogHandler!
                    .ReceivedLogs!
                    .Where(entry => entry.level == LogLevel.Fatal)
                    .Count() == 1);

                IsTrue(parser
                        .LogHandler!
                        .ReceivedLogs!
                        .Where(entry => entry.level == LogLevel.Error)
                        .Count() == 1);
            }
        }

        [TestMethod]
        public void InvalidGrammarInGrammar_CreateParser_ExpectException()
        {
            var parser = new RuleGraphBuilder();

            try
            {
                // missing ';' after 'bar=foo' and missing '=' after 'baz'
                parser.InitializeFromDefinition("foo='string';", "bar=foo xxx=foo; baz foo;");
                Fail();
            }
            catch (ScriptPipelineException pipelineException)
            {
                var e = pipelineException.InnerException as ParseException;

                IsTrue(e.Errors != null);
                IsTrue(e.Errors.Count() == 2);
                IsTrue(e.Errors.ElementAt(0).Start == 3);
                IsTrue(e.Errors.ElementAt(0).Length == 0);
                IsTrue(e.Errors.ElementAt(1).Start == 8);
                IsTrue(e.Errors.ElementAt(1).Length == 0);

                IsTrue(parser
                    .LogHandler!
                    .ReceivedLogs!
                    .Where(entry => entry.level == LogLevel.Fatal)
                    .Count() == 1);

                IsTrue(parser
                        .LogHandler!
                        .ReceivedLogs!
                        .Where(entry => entry.level == LogLevel.Error)
                        .Count() == 2);
            }           
        }

        [TestMethod]
        [ExpectedException(typeof(FatalConditionException<int>))]
        public void SetupRuleWithFatalWithCondition_CreateParserAndParseRule_ExpectException()
        {
            var parser = new RuleGraphBuilder()
                .InitializeFromDefinition("foo='trigger fatal';", "bar = fatal 'triggered a fatal condition' if foo;");

            // should trigger the fatal exception 
            parser.Parse("trigger fatal");
        }

        [TestMethod]
        [ExpectedException(typeof(FatalConditionException<int>))]
        public void SetupRuleWithFatalWithoutCondition_CreateParserAndParseRule_ExpectException()
        {
            var parser = new RuleGraphBuilder()
                .InitializeFromDefinition("foo='trigger fatal';", "bar = foo, fatal 'triggered a fatal condition';");

            // should trigger the fatal exception 
            parser.Parse("trigger fatal");
        }

        [TestMethod]
        public void MissingUnaryTermInput_CreateParser_ExpectException()
        {
            var parser = new RuleGraphBuilder();

            try
            {
                // first rule has no term after !;
                // second rule is fine
                // third rule has a ? and then nothing
                parser.InitializeFromDefinition("rule1 = !; rule2 = 'bar'; rule3=?");
                Fail();
            }
            catch (ScriptPipelineException pipelineException)
            {
                var e = pipelineException.InnerException as ParseException;

                IsTrue(e.Errors != null);
                IsTrue(e.Errors.Count() == 3);
                IsTrue(e.Errors.ElementAt(0).Start == 3);
                IsTrue(e.Errors.ElementAt(0).Length == 0);
                IsTrue(e.Errors.ElementAt(1).Start == 11);
                IsTrue(e.Errors.ElementAt(1).Length == 0);

                IsTrue(parser
                        .LogHandler!
                        .ReceivedLogs!
                        .Where(entry => entry.level == LogLevel.Fatal)
                        .Count() == 1);

                IsTrue(parser
                        .LogHandler!
                        .ReceivedLogs!
                        .Where(entry => entry.level == LogLevel.Error)
                        .Count() == 3);
            }
        }
    }
}
