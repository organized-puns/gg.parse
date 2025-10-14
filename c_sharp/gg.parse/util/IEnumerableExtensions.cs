namespace gg.parse.util
{
    public static class IEnumerableExtensions
    {
        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> values, Action<T> action)
        {
            foreach (var item in values)
            {
                action(item);
            }

            return values;
        }
    }
}
