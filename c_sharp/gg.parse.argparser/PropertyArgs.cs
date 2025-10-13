using System.Reflection;

namespace gg.parse.argparser
{
    public partial class ArgsReader<T>
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

            public string KeyToString()
            {
                var key = PropertyInfoIndex >= 0 ? $"{PropertyInfoIndex}" : "";

                if (Attribute != null)
                {
                    key = !string.IsNullOrEmpty(Attribute.ShortName) ? $"{key}, -{Attribute.ShortName}" : key;
                    key = !string.IsNullOrEmpty(Attribute.FullName) ? $"{key}, --{Attribute.FullName}" : key;
                }
                return key;
            }

            public void SetValue(object target, object value)
            {
                Info.SetValue(target, value);
            }
        }

        
    }
}
