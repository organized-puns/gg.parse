// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

namespace gg.parse.properties
{
    public class PropertiesException : Exception
    {
        public string? ErrorReport
        {
            get;
            private set;
        }


        public PropertiesException() { }

        public PropertiesException(string message) : base(message) { }

        public PropertiesException(string message, Exception e) : base(message, e) { }

        public PropertiesException(string message, Exception e, string errorReport) : base(message, e) 
        {
            ErrorReport = errorReport;
        }
    }
}
