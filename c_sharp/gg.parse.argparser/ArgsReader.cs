
using System.Globalization;
using System.Reflection;
using gg.parse.argparser;
using gg.parse.script;
using gg.parse.script.common;

namespace gg.parse.json
{
    public class ArgsReader<T>
    {
        private class PropertyArgs
        {
            public PropertyInfo Info { get; init; }

            public int PropertyInfoIndex { get; init; }

            public ArgAttribute? Attribute { get; init; }
            
            public bool MatchesFullName(string key) =>
                Attribute == null
                    ? Info.Name == key
                    : Attribute.FullName == key;

            public bool MatchesShortName(string key) =>
                Attribute != null && Attribute.ShortName == key;

            public bool MatchesIndex(int index) =>
                Attribute == null
                    ? PropertyInfoIndex == index
                    : Attribute.Index == index;

            public bool IsRequired =>
                Attribute != null && Attribute.IsRequired;

            public void SetValue(object target, object value)
            {
                Info.SetValue(target, value);
            }
        }

        private static readonly List<PropertyArgs> _propertyAttributes = CreatePropertyArgList();
              
        private ParserBuilder _parserBuilder;
        
        public ArgsReader()
        {
            _parserBuilder = new ParserBuilder().FromFile("assets/args.tokens", "assets/args.grammar");
        }

        public T Parse(string[] args) =>
            Parse(string.Join(" ", args));

        public T Parse(string args)
        {
            var (tokens, syntaxTree) = _parserBuilder.Parse(args);
            var target = Activator.CreateInstance<T>();
            var index = 0;
            var errors = new List<string>();
            var requiredArgs = new HashSet<PropertyArgs>(_propertyAttributes.Where(attr => attr.IsRequired));

            foreach (var attr in _propertyAttributes)
            {
                if (attr.Attribute != null && attr.Attribute.DefaultValue != null)
                {
                    attr.SetValue(target, attr.Attribute.DefaultValue);
                    requiredArgs.Remove(attr);
                }
            }

            foreach (var node in syntaxTree)
            {
                if (node == ArgParserNames.ArgOption)
                {
                    var key = node[0]!.GetText(args, tokens).Replace("-", "");

                    var property = _propertyAttributes
                        .FirstOrDefault(propertyArg => 
                            propertyArg.MatchesShortName(key) || propertyArg.MatchesFullName(key)
                        );

                    if (property == null)
                    {
                        errors.Add($"Can't '{key}' to any known option.");
                    }
                    else
                    {
                        try
                        {
                            object value = node.Count == 2 
                                ? ParseValue(property.Info.PropertyType, node[1], tokens.Annotations, args) 
                                // assume its a bool
                                : true;
                            property.SetValue(target!, value);

                            requiredArgs.Remove(property);
                        }
                        catch (Exception ex)
                        {
                            errors.Add(ex.Message);
                        }
                    }
                }
                else if (node == ArgParserNames.ArgValue)
                {
                    var property = _propertyAttributes
                        .FirstOrDefault(propertyArg => propertyArg.MatchesIndex(index));

                    if (property == null)
                    {
                        errors.Add($"Can't '{index}' to any known option.");
                    }
                    try
                    {
                        object value = ParseValue(property.Info.PropertyType, node, tokens.Annotations, args);
                        property.SetValue(target!, value);
                        requiredArgs.Remove(property);
                        index++;
                    }
                    catch (Exception ex)
                    {
                        errors.Add(ex.Message);
                    }
                }
            }

            foreach (var arg in requiredArgs)
            {
                var key = arg.PropertyInfoIndex >= 0 ? $"{arg.PropertyInfoIndex}" : "";

                if (arg.Attribute != null)
                {
                    key = !string.IsNullOrEmpty(arg.Attribute.ShortName) ? $"{key}, -{arg.Attribute.ShortName}" : key;
                    key = !string.IsNullOrEmpty(arg.Attribute.FullName) ? $"{key}, --{arg.Attribute.FullName}" : key;
                }

                errors.Add($"No argument provided for _required_ arg {arg.Info.Name}(${key}).");
            }

            if (errors.Count > 0)
            {
                throw new ArgumentException("Failed to read arguments:\n" + string.Join("\n", errors));
            }

            return target;
        }

        private object ParseValue(Type targetType, Annotation annotation, List<Annotation> tokenList, string text)
        {
            if (targetType.IsArray)
            {
                var arrayType = targetType.GetElementType();

                if (annotation[0] == ArgParserNames.Array)
                {
                    var result = Array.CreateInstance(arrayType, annotation[0].Count);
                    var index = 0;

                    foreach (var element in annotation[0])
                    {
                        result.SetValue( ParseValue(arrayType, element, tokenList, text), index);
                        index++;
                    }

                    return result;
                }
                throw new ArgumentException($"Request Array<{arrayType}> but provided value is not a valid array.");
            }
            else
            {
                return ParseBasicValue(targetType, annotation.GetText(text, tokenList));
            }
        }

        private object ParseBasicValue(Type targetType, string text)
        {
            if (targetType == typeof(bool))
            {
                return bool.Parse(text);    
            }
            else if (targetType == typeof(string))
            {
                return text;
            }
            else if (targetType == typeof(int))
            {
                return int.Parse(text);
            }
            else if (targetType == typeof(double))
            {
                return double.Parse(text, CultureInfo.InvariantCulture);
            }
            else if (targetType == typeof(float))
            {
                return float.Parse(text, CultureInfo.InvariantCulture);
            }
            else
            {
                throw new NotImplementedException($"No backing implementation to parse type {targetType}.");
            }

        }

        private static List<PropertyArgs> CreatePropertyArgList()
        {
            var result = new List<PropertyArgs>();
            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetProperty);

            for (int i = 0; i < properties.Length; i++)
            {
                result.Add(new PropertyArgs()
                {
                    Info = properties[i],
                    PropertyInfoIndex = i,
                    Attribute = (ArgAttribute) Attribute.GetCustomAttribute(properties[i], typeof(ArgAttribute)),
                });
            }

            return result;
        }        

        
    }
}
