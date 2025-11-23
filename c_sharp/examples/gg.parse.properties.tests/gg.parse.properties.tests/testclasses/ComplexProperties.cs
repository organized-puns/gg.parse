// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

namespace gg.parse.properties.tests.testclasses
{
    public class ComplexProperties
    {
        public string Name { get; set; }

        public Dictionary<string, string> ExtendedProperties { get; set; }

        public int[] Arr { get; set; }

        public SingleProperty SingleProperty { get; set; }

        public List<bool> BoolList { get; set; }

        public HashSet<string> StringSet { get; set; }
    }
}
