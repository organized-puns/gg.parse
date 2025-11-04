// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

using gg.parse.rules;
using gg.parse.script;
using System.Text;

namespace gg.parse.argparser
{
    public class PropertyFile
    {
        public static T? ReadFile<T>(string filename) where T : class
        {
            if (string.IsNullOrEmpty(filename))
            {
                throw new ArgumentException($"Filename is null or empty.");
            }

            if (!File.Exists(filename))
            {
                throw new ArgumentException($"Filename {filename} does not exist");
            }

            return Read<T>(File.ReadAllText(filename));
        }           

        public static T? Read<T>(string propertiesText) where T : class
        {
            if (string.IsNullOrEmpty(propertiesText))
            {
                return default;
            }

            var builder = new ParserBuilder();

            try
            {
                builder.FromFile("./assets/properties.tokens", "./assets/properties.grammar");

                var (tokens, syntaxTree) = builder.Parse(propertiesText);

                if (syntaxTree
                    && syntaxTree.Annotations != null
                    && syntaxTree.Annotations.Count >= 1
                    && tokens.Annotations != null)
                {
                    // validate this is a property file
                    if (syntaxTree.Annotations[0] == "properties")
                    {
                        if (syntaxTree.Annotations[0].Count > 0)
                        {
                            return (T)ParseInstance.OfValue(
                                typeof(T), 
                                syntaxTree.Annotations[0][0]!, 
                                tokens.Annotations, 
                                propertiesText
                            );
                        }

                        // empty property set
                        return Activator.CreateInstance<T>();
                    }
                }

                throw new ArgumentException($"Can't parse property text.");
            }
            catch (Exception ex)
            {
                var report = builder.GetReport(ex, LogLevel.Fatal | LogLevel.Error);
                throw new ArgumentException($"Failed to read properties.\n{report}", ex);
            }
        }

        public static string Write<T>(T obj) where T : class =>
            Write(obj, new PropertiesConfig());

        public static string Write<T>(T obj, in PropertiesConfig config) where T : class
        {
            if (obj == null)
            {
                return "null";
            }

            return new StringBuilder().AppendValue(obj, config).ToString();
        }
    }
}
