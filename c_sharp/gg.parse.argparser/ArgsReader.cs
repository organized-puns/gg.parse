// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

using System.Reflection;
using gg.parse.core;
using gg.parse.script;
using gg.parse.util;

namespace gg.parse.argparser
{
    public partial class ArgsReader<T>
    {
        private static readonly List<PropertyArgs> _propertyDescriptors = CreatePropertyArgList();
              
        private readonly ParserBuilder _parserBuilder;

        public ParserBuilder Parser => _parserBuilder;

        
        public ArgsReader()
        {
            _parserBuilder = new ParserBuilder().FromFile("assets/args.tokens", "assets/args.grammar");
        }

        public string GetErrorReport(Exception e)
        {
            return Parser.GetReport(e, rules.LogLevel.Error | rules.LogLevel.Fatal);
            /*var parserErrors = Parser.LogHandler?.ReceivedLogs?.Where(l => l.level == rules.LogLevel.Error);

            return parserErrors != null && parserErrors.Any()
                ? string.Join("\n", parserErrors)
                : e.Message;*/
        }
        
        public T Parse(string[] args) =>
            Parse(string.Join(" ", args));        

        public T Parse(string args)
        {
            var (tokens, syntaxTree) = _parserBuilder.Parse(args);
            var target = Activator.CreateInstance<T>();
            var index = 0;
            var errors = new List<string>();
            var requiredArgs = new HashSet<PropertyArgs>(_propertyDescriptors.Where(attr => attr.IsRequired));

            // set all default values
            _propertyDescriptors
                .Where(descriptor => descriptor.Attribute != null && descriptor.Attribute.DefaultValue != null)
                .ForEach(descriptor =>
                {
                    descriptor.SetValue(target!, descriptor.Attribute!.DefaultValue!);
                    requiredArgs.Remove(descriptor);
                });

            // assign args/options to properties 
            foreach (var node in syntaxTree)
            {
                PropertyArgs? property = null;

                switch (node)
                {
                    case ArgParserNames.ArgOption:
                        if ((property = ArgsReader<T>.AssignOption(target, node, args, tokens, errors)) != null)
                        {
                            requiredArgs.Remove(property);
                        }
                        break;
                    case ArgParserNames.ArgValue:
                        if ((property = ArgsReader<T>.AssignValue(target, node, index, args, tokens, errors)) != null)
                        {
                            requiredArgs.Remove(property);
                            index++;
                        }
                        break;
                }                
            }

            // validate if all required args were provided
            foreach (var arg in requiredArgs)
            {
                errors.Add($"No argument provided for _required_ arg '{arg.ArgName}'({arg.KeyToString()}).");
            }

            return errors.Count > 0 
                    ? throw new ArgumentException("Failed to read arguments:\n - " + string.Join("\n - ", errors))
                    : target;
        }

        private static PropertyArgs? AssignValue(
            T target, 
            Annotation node, 
            int index, 
            string args, 
            ParseResult tokens, 
            List<string> errors
        )
        {
            var property = _propertyDescriptors.FirstOrDefault(propertyArg => propertyArg.MatchesIndex(index));

            if (property == null)
            {
                errors.Add($"Can't match the arg at position '{index}' to any known option.");
            }
            else
            {
                Assertions.Requires(node.Count >= 1);
                Assertions.RequiresNotNull(node[0]);
                try
                {
                    property.SetValue(
                        target!,
                        PropertyReader.OfValue(property!.ArgType, node[0]!, tokens.Annotations!, args)!
                    );
                }
                catch (Exception ex)
                {
                    errors.Add(ex.Message);
                }
            }

            return property;
        }

        private static (string? key, PropertyArgs? property) FindKeyProperty(Annotation keyNode, string text, ParseResult tokens)
        {
            var key = keyNode!.GetText(text, tokens).Replace("-", "");

            return (key, _propertyDescriptors
                .FirstOrDefault(propertyArg =>
                    propertyArg.MatchesShortName(key) || propertyArg.MatchesFullName(key)
                ));
        }

        private static PropertyArgs? AssignOption(T target, Annotation node, string args, ParseResult tokens, List<string> errors)
        {
            // validate all the required node elements are present
            Assertions.RequiresNotNull(tokens.Annotations);
            Assertions.RequiresNotNull(node);
            Assertions.RequiresNotNull(node[0]);
            Assertions.RequiresNotNull(node[1]);
            Assertions.RequiresNotNull(node[1]![0]);

            var (key, argType) = FindKeyProperty(node[0]!, args, tokens);

            if (argType == null)
            {
                errors.Add($"Can't match '{key}' to any known option.");
            }
            else
            {
                try
                {
                    object? value = node.Count == 2
                        // assumed structure is -a=b where node[0] is a key, node[1] = value and node[1][0]
                        // the actual value type
                        ? PropertyReader.OfValue(argType.ArgType, node[1]![0]!, tokens.Annotations, args)
                        // assume its a bool
                        : true;

                    argType.SetValue(target!, value!);

                    return argType;
                }
                catch (Exception ex)
                {
                    errors.Add(ex.Message);
                }
            }

            return null;
        }

        private static List<PropertyArgs> CreatePropertyArgList()
        {
            var result = new List<PropertyArgs>();
            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetProperty);
            var fields = typeof(T).GetFields(BindingFlags.Public | BindingFlags.Instance);
            var argAttributeType = typeof(ArgAttribute);

            for (int i = 0; i < properties.Length; i++)
            {
                var attribute = Attribute.GetCustomAttribute(properties[i], argAttributeType) as ArgAttribute;
                result.Add(new PropertyArgs(properties[i], null, i, attribute));
            }

            for (int i = 0; i < fields.Length; i++)
            {
                var attribute = Attribute.GetCustomAttribute(fields[i], argAttributeType) as ArgAttribute;
                result.Add(new PropertyArgs(null, fields[i], i, attribute));
            }

            return result;
        }               
    }
}
