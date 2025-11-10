// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

using gg.parse.properties.tests.testclasses;
using gg.parse.script;

using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace gg.parse.properties.tests
{
    [TestClass]
    public class TypeToPropertyTests
    {
        private static readonly ParserBuilder PropertyParser =
            new ParserBuilder().FromFile("./assets/properties.tokens", "./assets/properties.grammar");

        [TestMethod]
        public void SetupEnumAnnotationAsIdentifier_Parse_ExpectValidEnumValue()
        {
            // setup
            var (enumAnnotation, tokens, text) = PropertyCompilerTestsHelpers.SetupSingleTokenTest("enum.TestEnum.Foo", PropertiesNames.Identifier);
            var allowedTypes = new TypePermissions(typeof(TestEnum));
            var context = new PropertyContext(text, tokens, null, allowedTypes);

            // act
            var result = new TypeToPropertyCompiler().Compile<TestEnum>(enumAnnotation, context);

            // test
            IsTrue(result == TestEnum.Foo);
        }

        [TestMethod]
        public void SetupEnumAnnotationAsString_Parse_ExpectValidEnumValue()
        {
            // setup
            var (enumAnnotation, tokens, text) = PropertyCompilerTestsHelpers.SetupSingleTokenTest("\"enum.TestEnum.Foo\"", PropertiesNames.String);
            var allowedTypes = new TypePermissions(typeof(TestEnum));
            var context = new PropertyContext(text, tokens, null, allowedTypes);

            // act
            var result = new TypeToPropertyCompiler().Compile<TestEnum>(enumAnnotation, context);

            // test
            IsTrue(result == TestEnum.Foo);
        }

        [TestMethod]
        public void ParseDictionary_CallInterpret_ExpectStringEnumDictionary()
        {
            // setup
            var text = "{ 1: enum.TestEnum.Foo, 2: enum.TestEnum.Bar }";
            var (tokens, syntaxTree) = PropertyParser.Parse(text, usingRule: "dictionary");

            // act
            var permissions = new TypePermissions(typeof(TestEnum));
            var context = new PropertyContext(text, tokens.Annotations, permissions);
            var result = new TypeToPropertyCompiler().Compile<Dictionary<int, TestEnum>>(syntaxTree[0], context);

            // test
            IsNotNull(result);
            IsTrue(result[1] == TestEnum.Foo);
            IsTrue(result[2] == TestEnum.Bar);
        }

        [TestMethod]
        public void ParseDictionaryWithEnumStrings_CallInterpret_ExpectStringEnumDictionary()
        {
            // setup
            var text = "{ 1: \"enum.TestEnum.Foo\", 2: \"enum.TestEnum.Bar\" }";
            var (tokens, syntaxTree) = PropertyParser.Parse(text, usingRule: "dictionary");

            // act
            var permissions = new TypePermissions(typeof(TestEnum));
            var context = new PropertyContext(text, tokens.Annotations, permissions);
            var result = new TypeToPropertyCompiler().Compile<Dictionary<int, TestEnum>>(syntaxTree[0], context);

            // test
            IsNotNull(result);
            IsTrue(result[1] == TestEnum.Foo);
            IsTrue(result[2] == TestEnum.Bar);
        }
    }
}
