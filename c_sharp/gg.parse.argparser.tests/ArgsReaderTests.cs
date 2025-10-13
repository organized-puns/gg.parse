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

            var arrayTypes = argReader.Parse("-b:[true, true, false] --i:[42, -3] --FloatArrays:[[1, 2.0, 3.5], [4, -5.5, 6]]");
            
            IsTrue(arrayTypes.Booleans.SequenceEqual([true, true, false]));
            IsTrue(arrayTypes.Ints.SequenceEqual([42, -3]));
            IsTrue(arrayTypes.FloatArrays[0].SequenceEqual([1f, 2f, 3.5f]));
            IsTrue(arrayTypes.FloatArrays[1].SequenceEqual([4f, -5.5f, 6f]));
        }

        
        public class AttrClassWithDictionaryTypes
        {
            [Arg(FullName = "table1")]
            public Dictionary<string, int> StringIntTable { get; set; }
        }

        [TestMethod]
        public void CreateReaderWithAttrClassWithDictionaryTypes_Parse_ExpectValuesSet()
        {
            var argReader = new ArgsReader<AttrClassWithDictionaryTypes>();

            var dictTypes = argReader.Parse("--table1:{ 'str1' : 1, 'str2': -42}");

            IsTrue(dictTypes.StringIntTable["str1"] == 1);
            IsTrue(dictTypes.StringIntTable["str2"] == -42);
        }
    }
}
