// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

using gg.parse.properties.tests.testclasses;

using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace gg.parse.properties.tests
{
    [TestClass]
    public class EnumPropertyTests
    {
        [TestMethod]
        public void CreateValidString_IsEnumProperty_ExpectTrue()
        {
            IsTrue(EnumProperty.IsEnum("enum.foo.bar"));
            IsTrue(EnumProperty.IsEnum("enum."));
        }

        [TestMethod]
        public void CreateInvalidString_IsEnumProperty_ExpectFalse()
        {
            IsFalse(EnumProperty.IsEnum("enumfoo.bar"));
            IsFalse(EnumProperty.IsEnum("enum foo"));
            IsFalse(EnumProperty.IsEnum(""));
            IsFalse(EnumProperty.IsEnum(null));
        }

        [TestMethod]
        public void SetupPropertyContext_ParseEnum_ExpectValidEnumValue()
        {
            var permissions = new TypePermissions(typeof(TestEnum));

            IsTrue(EnumProperty.Parse<TestEnum>($"enum.TestEnum.Foo", permissions) == TestEnum.Foo);
            IsTrue(EnumProperty.Parse<TestEnum>($"enum.TestEnum.Bar", permissions) == TestEnum.Bar);
        }

        [TestMethod]
        public void ReadWriteEnumProperty_ParseEnum_ExpectValidEnumValue()
        {
            var permissions = new TypePermissions(typeof(TestEnum));

            var text = EnumProperty.ToText(TestEnum.Foo);
            IsTrue(EnumProperty.Parse<TestEnum>(text, permissions) == TestEnum.Foo);

            text = EnumProperty.ToText(TestEnum.Bar);
            IsTrue(EnumProperty.Parse<TestEnum>(text, permissions) == TestEnum.Bar);
        }
    }
}

