// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

using System.Collections.Immutable;
using System.Reflection;
using System.Text;

using gg.parse.core;
using gg.parse.properties.tests.testclasses;
using gg.parse.script;
using gg.parse.tests;

using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

using Range = gg.parse.util.Range;

namespace gg.parse.properties.tests
{
    [TestClass]
    public class AnnotationToPropertyCompilerTests
    {
        private static readonly IRule identifierRule = new EmptyRule(PropertiesNames.Identifier);
        private static readonly IRule stringRule = new EmptyRule(PropertiesNames.String);
        private static readonly IRule intRule = new EmptyRule(PropertiesNames.Int);
        private static readonly IRule kvpRule = new EmptyRule(PropertiesNames.KvpPair);
        private static readonly IRule kvpListRule = new EmptyRule(PropertiesNames.KvpList);
        private static readonly IRule dictionaryRule = new EmptyRule(PropertiesNames.Dictionary);
        private static readonly IRule scopeStartRule = new EmptyRule(PropertiesNames.ScopeStart);
        private static readonly IRule scopeEndRule = new EmptyRule(PropertiesNames.ScopeEnd);
        private static readonly IRule boolRule = new EmptyRule(PropertiesNames.Boolean);
        private static readonly IRule arrayRule = new EmptyRule(PropertiesNames.Array);
        private static readonly IRule arrayStart = new EmptyRule(PropertiesNames.ArrayStart);
        private static readonly IRule arrayEnd = new EmptyRule(PropertiesNames.ArrayEnd);

        private static readonly ParserBuilder PropertyParser = 
            new ParserBuilder().FromFile("./assets/properties.tokens", "./assets/properties.grammar");

        [TestMethod]
        public void SetupIntAnnotation_CallInterpret_ExpectIntValue()
        {
            // setup
            var (intAnnotation, tokens, text) = SetupSingleTokenTest("123", PropertiesNames.Int);

            // act
            var result = new AnnotationToPropertyCompiler()
                            .Compile<int>(intAnnotation, new PropertyContext(text, tokens));

            // test
            IsTrue(result == 123);
        }

        [TestMethod]
        public void SetupStringAnnotation_CallInterpret_ExpectIntValue()
        {
            // setup
            var (stringAnnotation, tokens, text) = SetupSingleTokenTest("'foo'", PropertiesNames.String);

            // act
            var context = new PropertyContext(text, tokens, true);
            var result = new AnnotationToPropertyCompiler().Compile<string>(stringAnnotation, context);

            // test
            IsTrue(result == "foo");
        }

        [TestMethod]
        public void SetupKvpAnnotation_CallInterpret_ExpectStringObjectDictionary()
        {
            // setup
            var text = "foo: 'bar', count: 3";

            var tokens = ImmutableList<Annotation>
                .Empty
                .AddRange([
                    new Annotation(identifierRule, new Range(0, 3)),
                    new Annotation(stringRule, new Range(5, 5)),
                    new Annotation(identifierRule, new Range(12, 5)),
                    new Annotation(intRule, new Range(19, 1))
                ]);

            var kvPair1 = new Annotation(kvpRule, new Range(0, 2), [
                new Annotation(identifierRule, new Range(0, 1)),
                new Annotation(stringRule, new Range(1, 1)),
            ]);

            var kvPair2 = new Annotation(kvpRule, new Range(2, 2), [
                new Annotation(identifierRule, new Range(2, 1)),
                new Annotation(intRule, new Range(3, 1)),
            ]);

            var kvpList = new Annotation(kvpListRule, new Range(0, tokens.Count), [kvPair1, kvPair2]);

            // act
            var context = new PropertyContext(text, tokens, true);
            var result = new AnnotationToPropertyCompiler().Compile<Dictionary<string, object>>(kvpList, context);

            // test
            IsNotNull(result);
            IsTrue(((string)result["foo"]) == "bar");
            IsTrue(((int)result["count"]) == 3);
        }

        [TestMethod]
        public void SetupEmptyArray_CallInterpret_ExpectNull()
        {
            // setup
            var text = "[]";

            var tokens = ImmutableList<Annotation>
                .Empty
                .AddRange([
                    new Annotation(arrayStart, new Range(0, 1)),
                    new Annotation(arrayEnd, new Range(1, 1)),
                ]);

            var arrayAnnotation = new Annotation(arrayRule, new Range(0, 2), [
                new Annotation(arrayStart, new Range(0, 1)),
                new Annotation(arrayEnd, new Range(1, 1)),
            ]);

            // act
            var context = new PropertyContext(text, tokens, true);
            var result = new AnnotationToPropertyCompiler().Compile(null, arrayAnnotation, context);

            // test
            IsNull(result);
        }

        [TestMethod]
        public void SetupEmptyDictionary_CallInterpret_ExpectNull()
        {
            // setup
            var tokens = ImmutableList<Annotation>
                .Empty
                .AddRange([
                    new Annotation(scopeStartRule, new Range(0, 1)),
                    new Annotation(scopeEndRule, new Range(1, 1)),
                ]);

            var dictionaryAnnotation = new Annotation(dictionaryRule, new Range(0, 2), [
                new Annotation(arrayStart, new Range(0, 1)),
                new Annotation(arrayEnd, new Range(1, 1)),
            ]);

            // act
            var context = new PropertyContext(null, tokens, true);
            var result = new AnnotationToPropertyCompiler().Compile(null, dictionaryAnnotation, context);

            // test
            IsNull(result);
        }

        [TestMethod]
        public void SetupMetaInformation_CallInterpret_ExpectDefaultObject()
        {
            // setup
            var text = MetaInformation.AppendMetaInformation(
                new StringBuilder(),
                new ComplexProperties(), 
                new PropertiesConfig(allowedTypes: new TypePermissions() {  AllowUnmanagedTypes = true })
            ).ToString();

            // too complex to build a manual token / syntax sets. So drop the unit
            // test constraints and make this an integration test
            var (tokens, syntaxTree) = PropertyParser.Parse(text, usingRule: "kvp_list");

            // act
            var context = new PropertyContext(text, tokens.Annotations, true);
            var result = new AnnotationToPropertyCompiler()
                .Compile<ComplexProperties>(syntaxTree.Annotations[0], context);

            // test
            IsNotNull(result);
        }


        [TestMethod]
        public void SetupDictionaryAnnotation_CallInterpret_ExpectStringObjectDictionary()
        {
            // setup
            var text = "{ 1: true, 2: false }";

            var tokens = ImmutableList<Annotation>
                .Empty
                .AddRange([
                    new Annotation(scopeStartRule, new Range(0, 1)),
                    new Annotation(intRule, new Range(2, 1)),
                    new Annotation(boolRule, new Range(5, 4)),
                    new Annotation(intRule, new Range(11, 1)),
                    new Annotation(boolRule, new Range(14, 5)),
                    new Annotation(scopeEndRule, new Range(20, 1))
                ]);

            var kvPair1 = new Annotation(kvpRule, new Range(1, 2), [
                new Annotation(intRule, new Range(1, 1)),
                new Annotation(boolRule, new Range(2, 1)),
            ]);

            var kvPair2 = new Annotation(kvpRule, new Range(3, 2), [
                new Annotation(intRule, new Range(3, 1)),
                new Annotation(boolRule, new Range(4, 1)),
            ]);

            var dictionary = new Annotation(
                dictionaryRule, 
                new Range(0, tokens.Count), 
                [
                    new Annotation(scopeStartRule, new Range(0, 1)),
                    kvPair1, 
                    kvPair2,
                    new Annotation(scopeEndRule, new Range(5, 1)),
                ]
            );

            // act
            var context = new PropertyContext(text, tokens, true);
            var result = new AnnotationToPropertyCompiler().Compile<Dictionary<int, bool>>(dictionary, context);

            // test
            IsNotNull(result);
            IsTrue(result[1] == true);
            IsTrue(result[2] == false);
        }

        [TestMethod]
        public void ParseDictionary_CallInterpret_ExpectStringObjectDictionary()
        {
            // setup
            var text = "{ 1: true, 2: false }";
            var (tokens, syntaxTree) = PropertyParser.Parse(text, usingRule: "dictionary");

            // act
            var context = new PropertyContext(text, tokens.Annotations, true)   ;
            var result = new AnnotationToPropertyCompiler().Compile<Dictionary<int, bool>>(syntaxTree[0], context);

            // test
            IsNotNull(result);
            IsTrue(result[1] == true);
            IsTrue(result[2] == false);
        }

        [TestMethod]
        public void ParseObjectWithMetaDataInDefaultFormat_CallInterpret_ExpectComplexObject()
        {
            // setup
            var text = "// autogenerated property file\r\n__meta_information: {\"ObjectType\": \"gg.parse.properties.tests.testclasses.ComplexProperties, gg.parse.properties.tests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null\"}\r\nName: \"foo\"\r\nExtendedProperties: {\r\n  \"key1\": \"value1\",\r\n  \"key2\": \"value2\"\r\n}\r\nArr: [1, 2, 3]\r\nSingleProperty: {\r\n  __meta_information: {\"ObjectType\": \"gg.parse.properties.tests.testclasses.SingleProperty, gg.parse.properties.tests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null\"}\r\n  Name: \"foo\"\r\n}\r\nBoolList: [true, false, true]\r\nStringSet: null";
            var (tokens, syntaxTree) = PropertyParser.Parse(text);
            var context = new PropertyContext(text, tokens.Annotations, true);

            // act
            var result = new AnnotationToPropertyCompiler().Compile<ComplexProperties>(syntaxTree[0][0], context);

            // test
            IsNotNull(result);
            // spot check
            IsTrue(result.Name == "foo");
            IsTrue(result.Arr.SequenceEqual([1,2,3]));
        }

        [TestMethod]
        public void ParseObjectWithMetaDataDontAllowUnmanagedTypes_CallInterpret_ExpectComplexArgumentExceptin()
        {
            // setup
            var text = "// autogenerated property file\r\n__meta_information: {\"ObjectType\": \"gg.parse.properties.tests.ComplexProperties, gg.parse.properties.tests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null\"}\r\nName: \"foo\"\r\nExtendedProperties: {\r\n  \"key1\": \"value1\",\r\n  \"key2\": \"value2\"\r\n}\r\nArr: [1, 2, 3]\r\nSingleProperty: {\r\n  __meta_information: {\"ObjectType\": \"gg.parse.properties.tests.testclasses.SingleProperty, gg.parse.properties.tests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null\"}\r\n  Name: \"foo\"\r\n}\r\nBoolList: [true, false, true]\r\nStringSet: null";
            var (tokens, syntaxTree) = PropertyParser.Parse(text);

            // context by default doesn't allow unmanaged types, ie types that are
            // not in the allowed list
            var context = new PropertyContext(text, tokens.Annotations);

            // act
            try
            {
                var result = new AnnotationToPropertyCompiler().Compile<ComplexProperties>(syntaxTree[0][0], context);
                Fail();
            }
            catch (PropertiesException)
            {
            }
        }

        [TestMethod]
        public void ParseObjectWithMetaDataSetAllowedTypes_CallInterpret_ExpectComplexArgumentExceptin()
        {
            // setup
            var text = "// autogenerated property file\r\n__meta_information: {\"ObjectType\": \"ComplexProperties\"}\r\nName: \"foo\"\r\nExtendedProperties: {\r\n  \"key1\": \"value1\",\r\n  \"key2\": \"value2\"\r\n}\r\nArr: [1, 2, 3]\r\nSingleProperty: {\r\n  __meta_information: {\"ObjectType\": \"SingleProperty\"}\r\n  Name: \"foo\"\r\n}\r\nBoolList: [true, false, true]\r\nStringSet: null";
            var (tokens, syntaxTree) = PropertyParser.Parse(text);

            // context by default doesn't allow unmanaged types, ie types that are
            // not in the allowed list. We need to explicitely allow specific types
            var context = new PropertyContext(text, tokens.Annotations)
                            .AllowTypes(typeof(ComplexProperties), typeof(SingleProperty));

            // act
            var result = new AnnotationToPropertyCompiler().Compile<ComplexProperties>(syntaxTree[0][0], context);

            // test
            IsNotNull(result);
            IsTrue(result.Name == "foo");
            IsTrue(result.Arr.SequenceEqual([1, 2, 3]));
        }

        [TestMethod]
        public void ParseObjectWithMetaDataSetAllowedTypesFromAssembly_CallInterpret_ExpectComplexArgumentExceptin()
        {
            // setup
            var text = "// autogenerated property file\r\n__meta_information: {\"ObjectType\": \"ComplexProperties\"}\r\nName: \"foo\"\r\nExtendedProperties: {\r\n  \"key1\": \"value1\",\r\n  \"key2\": \"value2\"\r\n}\r\nArr: [1, 2, 3]\r\nSingleProperty: {\r\n  __meta_information: {\"ObjectType\": \"SingleProperty\"}\r\n  Name: \"foo\"\r\n}\r\nBoolList: [true, false, true]\r\nStringSet: null";
            var (tokens, syntaxTree) = PropertyParser.Parse(text);

            // context by default doesn't allow unmanaged types, ie types that are
            // not in the allowed list. We need to explicitely allow specific types.
            // If there are a lot types, allowing (public) types by assembly / namespace may make it easier
            var context = new PropertyContext(text, tokens.Annotations)
                            .AllowTypes(Assembly.GetExecutingAssembly(), "gg.parse.properties.tests.testclasses");

            // act
            var result = new AnnotationToPropertyCompiler().Compile<ComplexProperties>(syntaxTree[0][0], context);

            // test
            IsNotNull(result);
            IsTrue(result.Name == "foo");
            IsTrue(result.Arr.SequenceEqual([1, 2, 3]));
        }

        [TestMethod]
        public void ParseObjectWithMetaDataInJsonFormat_CallInterpret_ExpectComplexObject()
        {
            // setup
            var text = "{\r\n  \"__meta_information\": {\"ObjectType\": \"gg.parse.properties.tests.testclasses.ComplexProperties, gg.parse.properties.tests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null\"},\r\n  \"Name\": \"foo\",\r\n  \"ExtendedProperties\": {\r\n    \"key1\": \"value1\",\r\n    \"key2\": \"value2\"\r\n  },\r\n  \"Arr\": [1, 2, 3],\r\n  \"SingleProperty\": {\r\n    \"__meta_information\": {\"ObjectType\": \"gg.parse.properties.tests.testclasses.SingleProperty, gg.parse.properties.tests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null\"},\r\n    \"Name\": \"foo\"\r\n  },\r\n  \"BoolList\": [true, false, true],\r\n  \"StringSet\": null\r\n}";
            var (tokens, syntaxTree) = PropertyParser.Parse(text);
            var context = new PropertyContext(text, tokens.Annotations, true);

            // act
            var result = new AnnotationToPropertyCompiler().Compile<ComplexProperties>(syntaxTree[0][0], context);

            // test
            IsNotNull(result);
            // spot check
            IsTrue(result.Name == "foo");
            IsTrue(result.Arr.SequenceEqual([1, 2, 3]));
        }

        [TestMethod]
        public void ParseObjectWithoutMetaDataInJsonFormat_CallInterpret_ExpectObjectDictionary()
        {
            // setup
            var text = "{\r\n  \"Name\": \"foo\",\r\n  \"ExtendedProperties\": {\r\n    \"key1\": \"value1\",\r\n    \"key2\": \"value2\"\r\n  },\r\n  \"Arr\": [1, 2, 3],\r\n  \"SingleProperty\": {\r\n    \"Name\": \"foo\"\r\n  },\r\n  \"BoolList\": [true, false, true],\r\n  \"StringSet\": null\r\n}";
            var (tokens, syntaxTree) = PropertyParser.Parse(text);
            var context = new PropertyContext(text, tokens.Annotations, true);

            // act
            var result = new AnnotationToPropertyCompiler().Compile<Dictionary<string, object>>(syntaxTree[0][0], context);

            // test
            IsNotNull(result);
            // spot check
            IsTrue(((string)result["Name"]) == "foo");
            IsTrue(((Dictionary<string, string>)result["ExtendedProperties"]).Count == 2);
            IsTrue(((int[])result["Arr"]).SequenceEqual([1, 2, 3]));
        }

        [TestMethod]
        public void ParseObjectWithoutMetaDataInDefaultFormat_CallInterpret_ExpectObjectDictionary()
        {
            // setup
            var text = "// autogenerated property file\r\nName: \"foo\"\r\nExtendedProperties: {\r\n  \"key1\": \"value1\",\r\n  \"key2\": \"value2\"\r\n}\r\nArr: [1, 2, 3]\r\nSingleProperty: {\r\n  Name: \"foo\"\r\n}\r\nBoolList: [true, false, true]\r\nStringSet: null";
            var (tokens, syntaxTree) = PropertyParser.Parse(text);
            var context = new PropertyContext(text, tokens.Annotations, true);

            // act
            var result = new AnnotationToPropertyCompiler().Compile<Dictionary<string, object>>(syntaxTree[0][0], context);

            // test
            IsNotNull(result);
            // spot check
            IsTrue(((string)result["Name"]) == "foo");
            IsTrue(((Dictionary<string, string>)result["ExtendedProperties"]).Count == 2);
            IsTrue(((int[])result["Arr"]).SequenceEqual([1, 2, 3]));
        }

        // --- Private / util methods ---------------------------------------------------------------------------------

        private static (Annotation grammarAnnotation, ImmutableList<Annotation> tokens, string text)
            SetupSingleTokenTest(string text, string tokenName)
        {
            var token = new EmptyRule(tokenName);
            var tokens = ImmutableList<Annotation>.Empty.Add(new Annotation(token, new Range(0, text.Length)));
            var grammarAnnotation = new Annotation(new EmptyRule(0, tokenName), new Range(0, 1));

            return (grammarAnnotation, tokens, text);
        }
    }
}
