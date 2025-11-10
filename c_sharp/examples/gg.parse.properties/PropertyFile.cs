// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

using System.Text;

using gg.parse.rules;
using gg.parse.script;
using gg.parse.script.parser;

namespace gg.parse.properties
{
    public class PropertyFile
    {


        /// <summary>
        /// Reads a property - or json file and returns an object of type T
        /// with the properties set to the values in said file.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filename"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static T? ReadFile<T>(string filename, TypePermissions? allowedTypes = null) where T : class
        {
            if (string.IsNullOrEmpty(filename))
            {
                throw new ArgumentException($"Filename is null or empty.");
            }

            if (!File.Exists(filename))
            {
                throw new ArgumentException($"Filename {filename} does not exist");
            }

            return Read<T>(File.ReadAllText(filename), allowedTypes);
        }

        /// <summary>
        /// Reads a property - or json text and returns an object of type T
        /// with the properties set to the values in said text.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filename"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static T? Read<T>(string propertiesText, TypePermissions? allowedTypes = null) where T : class
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
                    if (syntaxTree.Annotations[0] == PropertiesNames.Properties)
                    {
                        if (syntaxTree.Annotations[0].Count > 0)
                        {
                            var context = new PropertyContext(
                                propertiesText,
                                tokens.Annotations,
                                syntaxTree.Annotations,
                                allowedTypes ?? new TypePermissions()
                            );

                            var annotationCompiler = new AnnotationToPropertyCompiler();
                            var typeCompiler = new TypeToPropertyCompiler();

                            annotationCompiler.TypeBasedCompiler = typeCompiler;
                            typeCompiler.AnnotationCompiler = annotationCompiler;

                            return typeCompiler.Compile<T?>(syntaxTree.Annotations[0][0]!, context);
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

        /// <summary>
        /// Write an object to a property file using a default PropertiesConfig
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        /// <see cref="PropertiesConfig"/>
        public static string Write<T>(T obj, TypePermissions? allowedTypes = null) where T : class =>
            Write(obj, new PropertiesConfig(allowedTypes: allowedTypes));

        /// <summary>
        /// Write an object to a property string using the given config
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        public static string Write<T>(T obj, in PropertiesConfig config) where T : class
        {
            if (obj == null)
            {
                return "null";
            }

            var builder = new StringBuilder();

            if (config.Format == PropertiesFormat.Default)
            {
                var targetType = obj.GetType();

                if (targetType.IsDictionary() || targetType.IsClass() || targetType.IsStruct())
                {
                    builder.AppendAsKeyValuePairs(obj, config);
                }
                else
                {
                    builder.AppendValue(obj, config);
                }
            }
            else
            {
                builder.AppendValue(obj, config);
            }

            return builder.ToString();
        }

        /// <summary>
        /// Write a property file using the given object to the given path 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <param name="obj"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        public static string WriteFile<T>(string path, T obj, in PropertiesConfig config) where T : class
        {
            var result = Write(obj, config);

            File.WriteAllText(path, result);

            return result;
        }

        /// <summary>
        /// Utility to write all the tokens and ast nodes names to a filename.
        /// (Needs to be updated if the grammar / tokens change).
        /// </summary>
        /// <param name="targetNameSpace"></param>
        /// <param name="filenamePrefix"></param>
        public static void ExportNames(string targetNameSpace = "gg.parse.properties", string filenamePrefix = "Properties")
        {
            var builder = new ParserBuilder()
                            .FromFile("./assets/properties.tokens", "./assets/properties.grammar");

            var tokens = ScriptUtils.ExportTokens(builder.TokenGraph, targetNameSpace, $"{filenamePrefix}Tokens");

            File.WriteAllText($"{filenamePrefix}Tokens.cs", tokens);

            var names = ScriptUtils.ExportNames(builder.TokenGraph, builder.GrammarGraph, targetNameSpace, $"{filenamePrefix}Names");

            File.WriteAllText($"{filenamePrefix}Names.cs", names);
        }
    }
}
