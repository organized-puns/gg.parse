using gg.parse.ebnf;

using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace gg.parse.tests.ebnf
{
    [TestClass]
    public class ParseErrorExceptionTests
    {
        [TestMethod]
        public void CreateInputWithTwoInvalidTokens_CreateParser_ExpectException()
        {
            try
            {
                // & and ^ are no valid tokens, so this should raise an exception
                // xxx no exceptions in the constructor
                var parser = new EbnfParser("& foo ^", null);
                Fail();
            }
            catch (TokenizeException e)
            {
                IsTrue(e.Errors != null);
                IsTrue(e.Errors.Count() == 2);
                IsTrue(e.Errors.ElementAt(0).Start == 0);
                IsTrue(e.Errors.ElementAt(0).Length == 2);
                IsTrue(e.Errors.ElementAt(1).Start == 6);
                IsTrue(e.Errors.ElementAt(1).Length == 1);
            }
        }

        [TestMethod]
        public void CreateInputWithInvalidGrammar_CreateParser_ExpectException()
        {
            try
            {
                // first rule has no terminating ;
                // second rule is fine
                // third rule has no assignment, =
                var parser = new EbnfParser("rule1 = 'foo' rule2 = 'bar'; rule3 'qaz';", null);
                Fail();
            }
            catch (ParseException e)
            {
                IsTrue(e.Errors != null);
                IsTrue(e.Errors.Count() == 2);
                IsTrue(e.Errors.ElementAt(0).Start == 0);
                IsTrue(e.Errors.ElementAt(0).Length == 3);
                IsTrue(e.Errors.ElementAt(1).Start == 7);
                IsTrue(e.Errors.ElementAt(1).Length == 3);
            }
        }
    }
}
