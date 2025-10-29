using gg.parse.script.common;
using gg.parse.script.parser;
using gg.parse.util;
using System.Diagnostics;
using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace gg.parse.argparser.tests
{
    [TestClass]
    public class ArgsReaderTests
    {
        public class SingleValueClass
        {
            public string Value { get; set; }
        }

        public class AttrClass
        {
            [Arg(FullName = "value")]
            public string FullValue { get; set; }

            [Arg(ShortName = "v")]
            public string ShortValue { get; set; }

            [Arg(DefaultValue = "???")]
            public string WithDefaultValue { get; set; }

            public string NoAttribute { get; set; }
        }

        [TestMethod]
        public void CreateReaderWithSingleValueClass_Parse_ExpectValueIsFoo()
        {
            var argReader = new ArgsReader<SingleValueClass>();

            var dummy = argReader.Parse("--Value=foo");

            IsTrue(dummy.Value == "foo");
        }

        [TestMethod]
        public void CreateReaderWithAttrClass_Parse_ExpectValuesSet()
        {
            var argReader = new ArgsReader<AttrClass>();

            var attrs = argReader.Parse("--value:foo -v:bar --NoAttribute=baz");

            IsTrue(attrs.FullValue == "foo");
            IsTrue(attrs.ShortValue == "bar");
            IsTrue(attrs.NoAttribute == "baz");
            IsTrue(attrs.WithDefaultValue == "???");
        }

        public class AttrClassWithArrayTypes
        {
            [Arg(ShortName = "b")]
            public bool[] Booleans { get; set; }

            [Arg(ShortName = "i")]
            public int[] Ints { get; set; }

            public float[][] FloatArrays { get; set; }
        }

        /// <summary>
        /// Simplified test with only one array type to verify float array parsing works
        /// as intended
        /// </summary>
        [TestMethod]
        public void CreateReaderWithAttrClassWithFloatArrayTypes_Parse_ExpectValuesSet()
        {
            var arrayTypes = new ArgsReader<AttrClassWithArrayTypes>().Parse("--FloatArrays:[[1, 2.0, 3.5], [4, -5.5, 6]]");

            IsTrue(arrayTypes.FloatArrays[0].SequenceEqual([1f, 2f, 3.5f]));
            IsTrue(arrayTypes.FloatArrays[1].SequenceEqual([4f, -5.5f, 6f]));
        }

        /// <summary>
        /// Simplified test with only one array type to verify bool array parsing works
        /// as intended
        /// </summary>
        [TestMethod]
        public void CreateReaderWithAttrClassWithBoolArrayTypes_Parse_ExpectValuesSet()
        {
            var arrayTypes = new ArgsReader<AttrClassWithArrayTypes>().Parse("-b:[true, true, false]");

            IsTrue(arrayTypes.Booleans.SequenceEqual([true, true, false]));
        }

        /// <summary>
        /// Full test with multiple array types to verify array parsing works as intended
        /// </summary>

        [TestMethod]
        public void CreateReaderWithAttrClassWithArrayTypes_Parse_ExpectValuesSet()
        {
            var argReader = new ArgsReader<AttrClassWithArrayTypes>();
            var arrayTypes = argReader.Parse("-b:[true, true, false] --i:[42, -3] --FloatArrays:[[1, 2.0, 3.5], [4, -5.5, 6]]");

            IsTrue(arrayTypes.Booleans.SequenceEqual([true, true, false]));
            IsTrue(arrayTypes.Ints.SequenceEqual([42, -3]));
            IsTrue(arrayTypes.FloatArrays[0].SequenceEqual([1f, 2f, 3.5f]));
            IsTrue(arrayTypes.FloatArrays[1].SequenceEqual([4f, -5.5f, 6f]));

            arrayTypes = argReader.Parse("-b:[]");
            IsTrue(arrayTypes.Booleans.Length == 0);
        }

        public class AttrClassWithDictionaryTypes
        {
            [Arg(FullName = "str_int_table")]
            public Dictionary<string, int> StringIntTable { get; set; }

            [Arg(FullName = "str_int_arr_table")]
            public Dictionary<string, int[]> StringIntArrayTable { get; set; }

            public Dictionary<int, Dictionary<bool, string>> IntDictTable { get; set; }
        }

        [TestMethod]
        public void CreateReaderWithAttrClassWithDictionaryTypes_Parse_ExpectValuesSet()
        {
            var argReader = new ArgsReader<AttrClassWithDictionaryTypes>();

            var dictTypes = argReader.Parse("--str_int_table:{ 'str1' : 1, 'str2': -42}");

            IsTrue(dictTypes.StringIntTable["str1"] == 1);
            IsTrue(dictTypes.StringIntTable["str2"] == -42);

            dictTypes = argReader.Parse("--str_int_arr_table:{ 'str1' : [1,2,3], 'str2': [-42]}");

            IsTrue(dictTypes.StringIntArrayTable["str1"].SequenceEqual([1, 2, 3]));
            IsTrue(dictTypes.StringIntArrayTable["str2"].SequenceEqual([-42]));

            dictTypes = argReader.Parse("--IntDictTable:{ 42 : {true: 'true', false: 'false'}, -1: {} }");

            IsTrue(dictTypes.IntDictTable[-1].Count == 0);
            IsTrue(dictTypes.IntDictTable[42][true] == "true");
        }

        public class AttrClassWithListTypes
        {
            public List<int> IntList { get; set; }

            public List<Dictionary<int, string>> DictList { get; set; }
        }

        [TestMethod]
        public void CreateReaderWithAttrClassWithListTypes_Parse_ExpectValuesSet()
        {
            var argReader = new ArgsReader<AttrClassWithListTypes>();

            var dictTypes = argReader.Parse("--IntList=[1, -2, 3]");

            IsTrue(dictTypes.IntList.SequenceEqual([1, -2, 3]));

            dictTypes = argReader.Parse("--DictList=[{1: 'one', -2: 'two'}, {3: 'three'}]");

            IsTrue(dictTypes.DictList[0][1] == "one");
            IsTrue(dictTypes.DictList[0][-2] == "two");
            IsTrue(dictTypes.DictList[1][3] == "three");
        }

        public class AttrClassWithObjectTypes
        {
            public string field = "";

            public SingleValueClass SingleValue { get; set; }

            public HashSet<int> SetValues { get; set; }
        }

        [TestMethod]
        public void CreateReaderWithAttrClassWithObjectTypes_Parse_ExpectValuesSet()
        {
            var argReader = new ArgsReader<AttrClassWithObjectTypes>();

            var objType = argReader.Parse("--SingleValue={Value: 'foo'}");

            IsTrue(objType.SingleValue.Value == "foo");

            objType = argReader.Parse("--SetValues=[1, -2, 3]");

            IsTrue(objType.SetValues.Contains(1));
            IsTrue(objType.SetValues.Contains(-2));
            IsTrue(objType.SetValues.Contains(3));

            objType = argReader.Parse("--field='bar'");

            IsTrue(objType.field == "bar");
        }

        public class AttrClassWithStructTypes
        {
            public struct Example
            {
                public float x;
                public int y;
            }

            [Arg(ShortName = "e")]
            public Example example;

            [Arg(Index = 0, IsRequired = true)]
            public Example required;
        }

        [TestMethod]
        public void CreateReaderWithAttrClassWithStructTypes_Parse_ExpectValuesSet()
        {
            var argReader = new ArgsReader<AttrClassWithStructTypes>();

            var structValue = argReader.Parse("-e={x: 0.42, y: -42} {x: 1.1, y: 0}");

            IsTrue(structValue.example.x == 0.42f);
            IsTrue(structValue.example.y == -42);

            IsTrue(structValue.required.x == 1.1f);
            IsTrue(structValue.required.y == 0);
        }

        /// <summary>
        /// Example of how to handle errors during parsing.
        /// </summary>
        [TestMethod]
        public void SetupParseError_Parse_ExpectHumanReadableError()
        {
            var argReader = new ArgsReader<AttrClassWithStructTypes>();

            try
            {
                // no closing '}'
                var structValue = argReader.Parse("-e={x: 0.42, y: -42  --Example=[1,2");

                Fail();
            }
            catch (ScriptException ex)
            {
                Debug.WriteLine(argReader.GetErrorReport(ex));
            }
        }

        /// <summary>
        /// </summary>
        [TestMethod]
        public void SetupSemanticError_Parse_ExpectHumanReadableError()
        {
            var argReader = new ArgsReader<AttrClassWithStructTypes>();

            try
            {
                // assign an int to a struct
                var structValue = argReader.Parse("-e=42 -e='foo' --privateExample=true");

                Fail();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(argReader.GetErrorReport(ex));
            }
        }
    }
}