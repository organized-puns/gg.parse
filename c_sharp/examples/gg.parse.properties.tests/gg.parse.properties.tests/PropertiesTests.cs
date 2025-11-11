// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

using gg.parse.properties.tests.testclasses;

using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace gg.parse.properties.tests
{
    [TestClass]
    public partial class PropertiesTests
    {

        [TestMethod]
        public void ReadEmptyProperty_ExpectEmptyProperty()
        {
            var property = PropertyFile.Read<SingleProperty>($"{{}}");

            IsNotNull(property);
            IsTrue(property.Name == null);
        }

        [TestMethod]
        public void ReadSingleProperty_ExpectNameIsFoo()
        {
            var fooValue = "foo";
            var property = PropertyFile.Read<SingleProperty>($"{{ Name: '{fooValue}' }}");

            IsNotNull(property);
            IsTrue(property.Name == fooValue);
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

            IsNotNull(properties);
            IsTrue(properties.Name == fooValue);
            IsTrue(properties.ExtendedProperties["key1"] == "value1");
            IsTrue(properties.ExtendedProperties["key2"] == "value2");
            IsTrue(properties.Arr.SequenceEqual([1, 2, 3]));
            IsTrue(properties.SingleProperty.Name == fooValue);
            IsTrue(properties.BoolList == null);
            IsTrue(properties.StringSet.SequenceEqual(["foo", "bar"]));
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

            IsNotNull(properties);
            IsTrue(properties.Name == fooValue);
            IsTrue(properties.ExtendedProperties["key1"] == "value1");
            IsTrue(properties.ExtendedProperties["key2"] == "value2");
            IsTrue(properties.Arr.SequenceEqual([1, 2, 3]));
            IsTrue(properties.SingleProperty.Name == fooValue);
            IsTrue(properties.BoolList == null);
            IsTrue(properties.StringSet.SequenceEqual(["foo", "bar"]));
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

            IsNotNull(properties);
            IsTrue(properties.Name == fooValue);
            IsTrue(properties.ExtendedProperties["key1"] == "value1");
            IsTrue(properties.ExtendedProperties["key2"] == "value2");
            IsTrue(properties.Arr.SequenceEqual([1, 2, 3]));
            IsTrue(properties.SingleProperty.Name == fooValue);
            IsTrue(properties.BoolList == null);
            IsTrue(properties.StringSet.SequenceEqual(["foo", "bar"]));
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

            IsNotNull(properties);
            IsTrue(properties.Name == fooValue);
            IsTrue(properties.ExtendedProperties["key1"] == "value1");
            IsTrue(properties.ExtendedProperties["key2"] == "value2");
            IsTrue(properties.Arr.SequenceEqual([1, 2, 3]));
            IsTrue(properties.SingleProperty.Name == fooValue);
            IsTrue(properties.BoolList == null);
            IsTrue(properties.StringSet.SequenceEqual(["foo", "bar"]));
        }

        [TestMethod]
        public void ReadUnspecifiedObject_ExpectStringObjectDictionary()
        {
            var fooValue = "foo";
            var properties = PropertyFile.Read(
                $"name: '{fooValue}'" +
                $"arr: [1, 2, 3]" +
                $"extendedProperties: {{" +
                $"    'key1': 'value1'" +
                $"    'key2': 'value2'" +
                $"}}" +
                $"stringSet: ['foo' 'bar']");

            IsNotNull(properties);

            var resultDict = properties as Dictionary<string, object>;

            IsNotNull(resultDict);

            IsTrue(((string)resultDict["name"]) == fooValue);

            var extendedProperties = resultDict["extendedProperties"] as Dictionary<string, string>;

            IsNotNull(extendedProperties);

            IsTrue(extendedProperties["key1"] == "value1");
            IsTrue(extendedProperties["key2"] == "value2");

            var arr = resultDict["arr"] as int[];

            IsNotNull(arr);
            IsTrue(arr.SequenceEqual([1, 2, 3]));

            var stringSet = resultDict["stringSet"] as string[];

            IsTrue(stringSet.SequenceEqual(["foo", "bar"]));
        }

        [TestMethod]
        public void WriteAndReadComplexPropertiesWithUnmanagedTypes_ExpectPropertiesSet()
        {
            var complexProperties = CreateTestObject();
            var allowedTypes = new TypePermissions() { AllowUnmanagedTypes = true };
            var complexDefaultPropertyString = PropertyFile.Write(complexProperties,
                new PropertiesConfig(format: PropertiesFormat.Default, indent: "  ", allowedTypes: allowedTypes));

            ValidateTestObject(complexProperties, complexDefaultPropertyString, allowedTypes);

            var complexDefaultPropertyWithMetaString = PropertyFile.Write(complexProperties,
                new PropertiesConfig(format: PropertiesFormat.Default, indent: "  ", addMetaInfo: true, allowedTypes: allowedTypes));

            ValidateTestObject(complexProperties, complexDefaultPropertyWithMetaString, allowedTypes);

            var complexJsonPropertyString = PropertyFile.Write(complexProperties,
                new PropertiesConfig(format: PropertiesFormat.Json, indent: "  ", addMetaInfo: true, allowedTypes: allowedTypes));

            ValidateTestObject(complexProperties, complexJsonPropertyString, allowedTypes);
        }

        [TestMethod]
        public void WriteAndReadMixedTypeDictionary_Compile_ExpectSameDictionary()
        {
            // setup
            var mixedDictionary = new Dictionary<object, object>()
            {
                { "str", "lorem" },
                { 2, "enum.TestEnum.Foo" },
                { TestEnum.Bar, new float[] { 42.0f, -1f } },
                { 3.0f, null },
            };

            var permissions = new TypePermissions(typeof(TestEnum));
            var config = new PropertiesConfig(allowedTypes: permissions);

            // act
            var dictString = PropertyFile.Write(mixedDictionary, config);
            var compiledDict = PropertyFile
                                .Read<Dictionary<object, object>>(
                                    dictString,
                                    permissions
                                );

            IsTrue(compiledDict.Count == mixedDictionary.Count);
            
            IsTrue(((string)mixedDictionary["str"]) == (string)compiledDict["str"]);

            var mixedValue = (TestEnum) EnumProperty.Parse((string) mixedDictionary[2], permissions);

            IsTrue(mixedValue == (TestEnum) compiledDict[2]);
            IsTrue(((float[])mixedDictionary[TestEnum.Bar])
                        .SequenceEqual(((float[])compiledDict[TestEnum.Bar])));
        }


        [TestMethod]
        public void WriteAndReadMixedTypeList_Compile_ExpectSameList()
        {
            // setup
            var mixedList = new List<object>()
            {
                "str",
                2, 
                "enum.TestEnum.Foo",
                TestEnum.Bar, 
                new float[] { 42.0f, -1f }
            };

            var permissions = new TypePermissions(typeof(TestEnum));
            var config = new PropertiesConfig(allowedTypes: permissions);

            // act
            var listString = PropertyFile.Write(mixedList, config);
            var compiledList = PropertyFile
                                .Read<List<object>>(
                                    listString,
                                    permissions
                                );

            // test
            IsTrue(compiledList.Count == mixedList.Count);

            IsTrue(((string)mixedList[0]) == (string)compiledList[0]);
            IsTrue(((int)mixedList[1]) == (int)compiledList[1]);

            var mixedValue = (TestEnum)EnumProperty.Parse((string)mixedList[2], permissions);
            IsTrue(mixedValue == (TestEnum)compiledList[2]);

            IsTrue(((TestEnum)mixedList[3]) == (TestEnum)compiledList[3]);

            IsTrue(((float[])mixedList[4])
                        .SequenceEqual(((float[])compiledList[4])));
        }

        [TestMethod]
        public void WriteAndReadMixedTypeArray_Compile_ExpectSameArray()
        {
            // setup
            var mixedArray = new object[]
            {
                "str",
                2,
                "enum.TestEnum.Foo",
                TestEnum.Bar,
                new float[] { 42.0f, -1f }
            };

            var permissions = new TypePermissions(typeof(TestEnum));
            var config = new PropertiesConfig(allowedTypes: permissions);

            // act
            var arrayString = PropertyFile.Write(mixedArray, config);
            var compiledArray = PropertyFile
                                .Read<object[]>(
                                    arrayString,
                                    permissions
                                );

            // test
            IsTrue(compiledArray.Length == mixedArray.Length);

            IsTrue(((string)mixedArray[0]) == (string)compiledArray[0]);
            IsTrue(((int)mixedArray[1]) == (int)compiledArray[1]);

            var mixedValue = (TestEnum)EnumProperty.Parse((string)mixedArray[2], permissions);
            IsTrue(mixedValue == (TestEnum)compiledArray[2]);

            IsTrue(((TestEnum)mixedArray[3]) == (TestEnum)compiledArray[3]);

            IsTrue(((float[])mixedArray[4])
                        .SequenceEqual(((float[])compiledArray[4])));
        }

        [TestMethod]
        public void WriteAndReadComplexPropertiesWithManagedTypes_ExpectPropertiesSet()
        {
            var complexProperties = CreateTestObject();
            var allowedTypes = new TypePermissions().AllowTypes(typeof(ComplexProperties), typeof(SingleProperty));
            var complexDefaultPropertyString = PropertyFile.Write(complexProperties,
                new PropertiesConfig(format: PropertiesFormat.Default, indent: "  ", allowedTypes: allowedTypes));

            ValidateTestObject(complexProperties, complexDefaultPropertyString, allowedTypes);

            var complexDefaultPropertyWithMetaString = PropertyFile.Write(complexProperties,
                new PropertiesConfig(format: PropertiesFormat.Default, indent: "  ", addMetaInfo: true, allowedTypes: allowedTypes));

            ValidateTestObject(complexProperties, complexDefaultPropertyWithMetaString, allowedTypes);

            var complexJsonPropertyString = PropertyFile.Write(complexProperties,
                new PropertiesConfig(format: PropertiesFormat.Json, indent: "  ", addMetaInfo: true, allowedTypes: allowedTypes));

            ValidateTestObject(complexProperties, complexJsonPropertyString, allowedTypes);
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

        private static void ValidateTestObject(
            ComplexProperties original, 
            string complexPropertyString, 
            TypePermissions permissions
        )
        {
            var properties = PropertyFile
                                .Read<ComplexProperties>(
                                    complexPropertyString, 
                                    permissions 
                                );

            IsNotNull(properties);
            IsTrue(properties.Name == "foo");
            IsTrue(properties.ExtendedProperties["key1"] == "value1");
            IsTrue(properties.ExtendedProperties["key2"] == "value2");
            IsTrue(properties.Arr.SequenceEqual([1, 2, 3]));
            IsTrue(properties.SingleProperty.Name == "foo");
            IsTrue(properties.BoolList.SequenceEqual(original.BoolList));
            IsTrue(properties.StringSet == null);
        }

        /*[TestMethod]
        public void ExportNames()
        {
            PropertyFile.ExportNames();
        }*/
    }
}
