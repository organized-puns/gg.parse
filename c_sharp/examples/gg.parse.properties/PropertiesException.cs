// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

namespace gg.parse.properties
{
    public class PropertiesException : Exception
    {
        public PropertiesException() { }

        public PropertiesException(string message) : base(message) { }
    }
}
