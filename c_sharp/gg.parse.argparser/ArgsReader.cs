
using System.Reflection;
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
        }

        private static readonly List<PropertyArgs> _propertyAttributes = CreatePropertyArgList();
              
        private ParserBuilder _parserBuilder;
           
        
        public ArgsReader()
        {
            _parserBuilder = new ParserBuilder()
                            .From(File.ReadAllText("assets/args.tokens"), 
                            File.ReadAllText("assets/args.gramma")
                         );
        }


        public T Parse(string[] args) =>
            Parse(string.Join(" ", args));

        public T Parse(string args)
        {
            var (tokens, grammar) = _parserBuilder.Parse(args);

            var target = Activator.CreateInstance<T>();


            // xxx todo 

            return target;
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

        private void TrySetOptionalValue(T target, string text, Annotation annation)
        {
            var argSwitch = annation.FindByRuleName("switch");
            var name = annation.FindByRuleName("optionalArgName");
            var value = annation.FindByRuleName("argValue");

            // keep a list of which properties we've found values
            var targetValues = new bool[_propertyAttributes.Count];

            var errors = new List<string>();

            if (argSwitch.Children[0].Rule.Name == "shorthand")
            {

            }
            else
            {
                var propertyName = name.GetText(text);
                var argIndex = _propertyAttributes.FindIndex(attr => attr.MatchesFullName(propertyName));
                
                if (argIndex >= 0)
                { 
                    var propertyValue = value.GetText(text);
                    _propertyAttributes[argIndex].Info.SetValue(target, propertyValue);
                    targetValues[argIndex] = true;
                }
                else
                {
                    errors.Add("No property '{propertyName}' defined.");
                }
            }
        }
    }
}
