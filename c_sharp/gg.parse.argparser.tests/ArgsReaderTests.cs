using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace gg.parse.json.tests
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

            var dummy = argReader.Parse("--Value foo");

            IsTrue(dummy.Value == "foo");
        }

        [TestMethod]
        public void CreateReaderWithSingleValueAttrClass_Parse_ExpectValuesSet()
        {
            var argReader = new ArgsReader<AttrClass>();

            var attrs = argReader.Parse("--value foo -v bar --NoAttribute baz");

            IsTrue(attrs.FullValue == "foo");
            IsTrue(attrs.ShortValue == "bar");
            IsTrue(attrs.NoAttribute == "baz");
            IsTrue(attrs.WithDefaultValue == "???");
        }

    }
}
