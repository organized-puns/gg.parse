using gg.parse.ebnf;

using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace gg.parse.tests.ebnf
{
    [TestClass]
    public class ParseErrorExceptionTests
    {
        [TestMethod]
        public void TwoInvalidTokensInTokenizer_CreateParser_ExpectException()
        {
            try
            {
                // & and ^ are no valid tokens, so this should raise an exception
                // xxx no exceptions in the constructor
                var parser = new EbnfParser("& foo ^", null);
                Fail();
            }
            catch (EbnfException ebnfException)
            {
                var e = ebnfException.InnerException as TokenizeException;

                IsTrue(e.Errors != null);
                IsTrue(e.Errors.Count() == 2);
                IsTrue(e.Errors.ElementAt(0).Start == 0);
                IsTrue(e.Errors.ElementAt(0).Length == 2);
                IsTrue(e.Errors.ElementAt(1).Start == 6);
                IsTrue(e.Errors.ElementAt(1).Length == 1);
            }
        }

        [TestMethod]
        public void InvalidTokenGrammar_CreateParser_ExpectException()
        {
            try
            {
                // first rule has no terminating ;
                // second rule is fine
                // third rule has no assignment, =
                var parser = new EbnfParser("rule1 = 'foo' rule2 = 'bar'; rule3 'qaz';", null);
                Fail();
            }
            catch (EbnfException ebnfException)
            {
                var e = ebnfException.InnerException as ParseException;

                IsTrue(e.Errors != null);
                IsTrue(e.Errors.Count() == 2);
                IsTrue(e.Errors.ElementAt(0).Start == 3);
                IsTrue(e.Errors.ElementAt(0).Length == 0);
                IsTrue(e.Errors.ElementAt(1).Start == 7);
                IsTrue(e.Errors.ElementAt(1).Length == 3);
            }
        }

        [TestMethod]
        public void InvalidTokensInGrammar_CreateParser_ExpectException()
        {
            try
            {
                // & and : are no valid tokens, so this should raise an exception
                // xxx no exceptions in the constructor
                var parser = new EbnfParser("foo='string';", "^ bar=foo; :");
                Fail();
            }
            catch (EbnfException ebnfException)
            {
                var e = ebnfException.InnerException as TokenizeException;

                IsTrue(e.Errors != null);
                IsTrue(e.Errors.Count() == 2);
                IsTrue(e.Errors.ElementAt(0).Start == 0);
                IsTrue(e.Errors.ElementAt(0).Length == 2);
                IsTrue(e.Errors.ElementAt(1).Start == 11);
                IsTrue(e.Errors.ElementAt(1).Length == 1);
            }
        }

        [TestMethod]
        public void InvalidGrammarInGrammar_CreateParser_ExpectException()
        {
            try
            {
                var parser = new EbnfParser("foo='string';", "bar=foo xxx=foo; baz foo;");
                Fail();
            }
            catch (EbnfException ebnfException)
            {
                var e = ebnfException.InnerException as ParseException;

                IsTrue(e.Errors != null);
                IsTrue(e.Errors.Count() == 2);
                IsTrue(e.Errors.ElementAt(0).Start == 3);
                IsTrue(e.Errors.ElementAt(0).Length == 0);
                IsTrue(e.Errors.ElementAt(1).Start == 7);
                IsTrue(e.Errors.ElementAt(1).Length == 3);
            }           
        }
    }
}
