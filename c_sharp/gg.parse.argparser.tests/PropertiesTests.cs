// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

namespace gg.parse.argparser.tests
{
    [TestClass]
    public class PropertiesTests
    {
        public class SingleProperty
        {
            public string Name { get; set; }
        }

        [TestMethod]
        public void ReadEmptyProperty_ExpectEmptyProperty()
        {
            var property = PropertyFile.Read<SingleProperty>($"{{}}");

            Assert.IsNotNull(property);
            Assert.IsTrue(property.Name == null);
        }

        [TestMethod]
        public void ReadSingleProperty_ExpectNameIsFoo()
        {
            var fooValue = "foo";
            var property = PropertyFile.Read<SingleProperty>($"{{ Name: '{fooValue}' }}");

            Assert.IsNotNull(property);
            Assert.IsTrue(property.Name == fooValue);
        }

        public class ComplexProperties
        {
            public string Name { get; set; }

            public Dictionary<string, string> ExtendedProperties { get; set; }

            public int[] Arr { get; set; }

            public SingleProperty SingleProperty { get; set; }

            public List<bool> BoolList { get; set; }

            public HashSet<string> StringSet { get; set; }
        }

        [TestMethod]
        public void ReadComplexProperties_ExpectPropertiesSet()
        {
            var fooValue = "foo";
            var properties = PropertyFile.Read<ComplexProperties>(
                $"{{" +
                $"  Name: '{fooValue}'," +
                $"  SingleProperty: {{" +
                $"      Name: '{fooValue}'" +
                $"  }}," +
                $"  Arr: [1, 2, 3]," +
                $"  ExtendedProperties: {{" +
                $"      'key1': 'value1'," +
                $"      'key2': 'value2'" +
                $"  }}," +
                $"  StringSet: ['foo', 'bar']" +
                $"}}");

            Assert.IsNotNull(properties);
            Assert.IsTrue(properties.Name == fooValue);
            Assert.IsTrue(properties.ExtendedProperties["key1"] == "value1");
            Assert.IsTrue(properties.ExtendedProperties["key2"] == "value2");
            Assert.IsTrue(properties.Arr.SequenceEqual([1, 2, 3]));
            Assert.IsTrue(properties.SingleProperty.Name == fooValue);
            Assert.IsTrue(properties.BoolList == null);
            Assert.IsTrue(properties.StringSet.SequenceEqual(["foo", "bar"]));
        }

        [TestMethod]
        public void ReadComplexPropertiesWithDefaultFormat_ExpectPropertiesSet()
        {
            var fooValue = "foo";
            var properties = PropertyFile.Read<ComplexProperties>(
                $"{{" +
                $"  Name: '{fooValue}'," +
                $"  SingleProperty: {{" +
                $"      Name: '{fooValue}'" +
                $"  }}," +
                $"  Arr: [1, 2, 3]," +
                $"  ExtendedProperties: {{" +
                $"      'key1': 'value1'," +
                $"      'key2': 'value2'" +
                $"  }}," +
                $"  StringSet: ['foo', 'bar']" +
                $"}}");

            Assert.IsNotNull(properties);
            Assert.IsTrue(properties.Name == fooValue);
            Assert.IsTrue(properties.ExtendedProperties["key1"] == "value1");
            Assert.IsTrue(properties.ExtendedProperties["key2"] == "value2");
            Assert.IsTrue(properties.Arr.SequenceEqual([1, 2, 3]));
            Assert.IsTrue(properties.SingleProperty.Name == fooValue);
            Assert.IsTrue(properties.BoolList == null);
            Assert.IsTrue(properties.StringSet.SequenceEqual(["foo", "bar"]));
        }

        [TestMethod]
        public void ReadComplexPropertiesWithDefaultFormatNoItemSeparators_ExpectPropertiesSet()
        {
            var fooValue = "foo";
            var properties = PropertyFile.Read<ComplexProperties>(
                $"{{" +
                $"  Name: '{fooValue}'" +
                $"  SingleProperty: {{" +
                $"      Name: '{fooValue}'" +
                $"  }}" +
                $"  Arr: [1 2 3]" +
                $"  ExtendedProperties: {{" +
                $"      'key1': 'value1'" +
                $"      'key2': 'value2'" +
                $"  }}" +
                $"  StringSet: ['foo' 'bar']" +
                $"}}");

            Assert.IsNotNull(properties);
            Assert.IsTrue(properties.Name == fooValue);
            Assert.IsTrue(properties.ExtendedProperties["key1"] == "value1");
            Assert.IsTrue(properties.ExtendedProperties["key2"] == "value2");
            Assert.IsTrue(properties.Arr.SequenceEqual([1, 2, 3]));
            Assert.IsTrue(properties.SingleProperty.Name == fooValue);
            Assert.IsTrue(properties.BoolList == null);
            Assert.IsTrue(properties.StringSet.SequenceEqual(["foo", "bar"]));
        }

        [TestMethod]
        public void ReadComplexPropertiesWithDefaultFormatNoItemSeparatorsKvpOnly_ExpectPropertiesSet()
        {
            var fooValue = "foo";
            var properties = PropertyFile.Read<ComplexProperties>(
                $"// header comments\n" +
                $"Name: '{fooValue}'" +
                $"SingleProperty: {{" +
                $"    Name: '{fooValue}'" +
                $"}}" +
                // mix in item separators to see if it works
                $"Arr: [1, 2, 3]" +
                $"ExtendedProperties: {{" +
                $"    'key1': 'value1'" +
                $"    'key2': 'value2'" +
                $"}}" +
                $"StringSet: ['foo' 'bar']" +
                $"/* footer comments */");

            Assert.IsNotNull(properties);
            Assert.IsTrue(properties.Name == fooValue);
            Assert.IsTrue(properties.ExtendedProperties["key1"] == "value1");
            Assert.IsTrue(properties.ExtendedProperties["key2"] == "value2");
            Assert.IsTrue(properties.Arr.SequenceEqual([1, 2, 3]));
            Assert.IsTrue(properties.SingleProperty.Name == fooValue);
            Assert.IsTrue(properties.BoolList == null);
            Assert.IsTrue(properties.StringSet.SequenceEqual(["foo", "bar"]));
        }

        [TestMethod]
        public void WriteAndReadComplexProperties_ExpectPropertiesSet()
        {
            var complexProperties = CreateTestObject();
            
            var complexDefaultPropertyString = PropertyFile.Write(complexProperties,
                new PropertiesConfig(format: PropertiesFormat.Default, indent: "  "));

            ValidateTestObject(complexProperties, complexDefaultPropertyString);

            var complexDefaultPropertyWithMetaString = PropertyFile.Write(complexProperties,
                new PropertiesConfig(format: PropertiesFormat.Default, indent: "  ", addMetaInfo: true));

            ValidateTestObject(complexProperties, complexDefaultPropertyWithMetaString);

            var complexJsonPropertyString = PropertyFile.Write(complexProperties,
                new PropertiesConfig(format: PropertiesFormat.Json, indent: "  ", addMetaInfo: true));

            ValidateTestObject(complexProperties, complexJsonPropertyString);
        }

        private static ComplexProperties CreateTestObject() =>
            new ()
            {
                Name = "foo",
                ExtendedProperties = new Dictionary<string, string>()
                    {
                        { "key1", "value1" },
                        { "key2", "value2" },
                    },
                SingleProperty = new SingleProperty()
                {
                    Name = "foo"
                },
                Arr = [1, 2, 3],
                BoolList = [true, false, true]
            };

        private static void ValidateTestObject(ComplexProperties original, string complexPropertyString)
        {
            var properties = PropertyFile.Read<ComplexProperties>(complexPropertyString);

            Assert.IsNotNull(properties);
            Assert.IsTrue(properties.Name == "foo");
            Assert.IsTrue(properties.ExtendedProperties["key1"] == "value1");
            Assert.IsTrue(properties.ExtendedProperties["key2"] == "value2");
            Assert.IsTrue(properties.Arr.SequenceEqual([1, 2, 3]));
            Assert.IsTrue(properties.SingleProperty.Name == "foo");
            Assert.IsTrue(properties.BoolList.SequenceEqual(original.BoolList));
            Assert.IsTrue(properties.StringSet == null);
        }

        /*[TestMethod]
        public void ExportNames()
        {
            PropertyFile.ExportNames();
        }*/
    }
}
