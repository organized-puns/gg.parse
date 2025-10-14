using System.Reflection;

using gg.parse.script;
using gg.parse.util;

namespace gg.parse.argparser
{
    public partial class ArgsReader<T>
    {
        private static readonly List<PropertyArgs> _propertyDescriptors = CreatePropertyArgList();
              
        private ParserBuilder _parserBuilder;

        public ParserBuilder Parser => _parserBuilder;

        
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
                        if ((property = AssignOption(target, node, args, tokens, errors)) != null)
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
                errors.Add($"No argument provided for _required_ arg {arg.Info.Name}(${arg.KeyToString()}).");
            }

            return errors.Count > 0 
                    ? throw new ArgumentException("Failed to read arguments:\n" + string.Join("\n", errors))
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
                try
                {
                    property.SetValue(
                        target!,
                        ParseInstance.OfValue<T>(property!.Info.PropertyType, node, tokens.Annotations!, args)
                    );
                }
                catch (Exception ex)
                {
                    errors.Add(ex.Message);
                }
            }

            return property;
        }

        private (string? key, PropertyArgs? property) FindKeyProperty(Annotation keyNode, string text, ParseResult tokens)
        {
            var key = keyNode!.GetText(text, tokens).Replace("-", "");

            return (key, _propertyDescriptors
                .FirstOrDefault(propertyArg =>
                    propertyArg.MatchesShortName(key) || propertyArg.MatchesFullName(key)
                ));
        }

        private PropertyArgs? AssignOption(T target, Annotation node, string args, ParseResult tokens, List<string> errors)
        {
            var (key, property) = FindKeyProperty(node[0]!, args, tokens);

            if (property == null)
            {
                errors.Add($"Can't match '{key}' to any known option.");
            }
            else
            {
                try
                {
                    object value = node.Count == 2
                        ? ParseInstance.OfValue<T>(property.Info.PropertyType, node[1][0], tokens.Annotations, args)
                        // assume its a bool
                        : true;

                    property.SetValue(target!, value);

                    return property;
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
