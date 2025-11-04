// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

using System.Text;

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
                $"  }}" +
                $"}}");

            Assert.IsNotNull(properties);
            Assert.IsTrue(properties.Name == fooValue);
            Assert.IsTrue(properties.ExtendedProperties["key1"] == "value1");
            Assert.IsTrue(properties.ExtendedProperties["key2"] == "value2");
            Assert.IsTrue(properties.Arr.SequenceEqual([1, 2, 3]));
            Assert.IsTrue(properties.SingleProperty.Name == fooValue);
        }

        [TestMethod]
        public void WriteAndReadComplexProperties_ExpectPropertiesSet()
        {
            var complexProperties = new ComplexProperties()
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
                Arr = [ 1, 2, 3 ]
            };

            var complexPropertyString = PropertyFile.Write(complexProperties, indent: "  ");

            var properties = PropertyFile.Read<ComplexProperties>(complexPropertyString);
         
            Assert.IsNotNull(properties);
            Assert.IsTrue(properties.Name == "foo");
            Assert.IsTrue(properties.ExtendedProperties["key1"] == "value1");
            Assert.IsTrue(properties.ExtendedProperties["key2"] == "value2");
            Assert.IsTrue(properties.Arr.SequenceEqual([1, 2, 3]));
            Assert.IsTrue(properties.SingleProperty.Name == "foo");
        }
    }
}
