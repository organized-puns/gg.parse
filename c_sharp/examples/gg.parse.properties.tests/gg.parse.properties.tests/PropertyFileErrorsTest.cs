// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

using System.Diagnostics;

using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace gg.parse.properties.tests
{
    /// <summary>
    /// Create errors either in the parse script or semantics, verify the errors
    /// are raised in a somewhat human understandable form.
    /// </summary>
    [TestClass]
    public class PropertyFileErrorsTest
    {
        [TestMethod]
        public void CreateScriptTokenError_Read_ExpectErrors()
        {
            try
            {
                // invalid token
                PropertyFile.Read("^ '1' @ '2' %");
                Fail();
            }
            catch (PropertiesException e)
            {
                IsTrue(e.InnerException != null);
                IsFalse(string.IsNullOrEmpty(e.ErrorReport));

                Debug.WriteLine(e.ErrorReport);
            }
        }

        [TestMethod]
        public void CreateScriptGrammarError_Read_ExpectError()
        {
            try
            {
                // invalid grammar
                PropertyFile.Read("[ var = 'boo', } }");
                Fail();
            }
            catch (PropertiesException e)
            {
                IsTrue(e.InnerException != null);
                IsFalse(string.IsNullOrEmpty(e.ErrorReport));

                Debug.WriteLine(e.ErrorReport);
            }
        }


        [TestMethod]
        public void CreateTypeError_Read_ExpectErrors()
        {
            try
            {
                // wrong type
                PropertyFile.Read<Dictionary<string, object>>("['this is an array']");
                Fail();
            }
            catch (PropertiesException e)
            {
                IsTrue(e.InnerException != null);
                IsFalse(string.IsNullOrEmpty(e.ErrorReport));

                Debug.WriteLine(e.ErrorReport);
            }
        }

        [TestMethod]
        public void CreateMultipleTypeErrors_Read_ExpectErrors()
        {
            try
            {
                // wrong types and a null as key
                PropertyFile.Read<Dictionary<string, int>>(
                      "{\n"
                    + " null: 'foo',\n"
                    + " 'foo': 1,\n"
                    + " 2: 'bar'\n" 
                    + "}");
                Fail();
            }
            catch (PropertiesException e)
            {
                IsTrue(e.InnerException != null);
                IsFalse(string.IsNullOrEmpty(e.ErrorReport));

                Debug.WriteLine(e.ErrorReport);
            }
        }
    }
}
