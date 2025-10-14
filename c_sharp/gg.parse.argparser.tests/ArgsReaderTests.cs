using gg.parse.script.common;
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


        [TestMethod]
        public void CreateReaderWithAttrClassWithArrayTypes_Parse_ExpectValuesSet()
        {
            var argReader = new ArgsReader<AttrClassWithArrayTypes>();

            var arrayRule = argReader.Parser.GrammarGraph.FindRule("array");
            var tokens = argReader.Parser.TokenGraph.TokenizeText("[true, true, false]");
            var syntaxTree = arrayRule.Parse(tokens);

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

            IsTrue(dictTypes.StringIntArrayTable["str1"].SequenceEqual([1,2,3]));
            IsTrue(dictTypes.StringIntArrayTable["str2"].SequenceEqual([-42]));

            dictTypes = argReader.Parse("--IntDictTable:{ 42 : {true: 'true', false: 'false'}, -1: {} }");
            
            IsTrue(dictTypes.IntDictTable[-1].Count == 0);
            IsTrue(dictTypes.IntDictTable[42][true] == "true");
        }
    }
}
