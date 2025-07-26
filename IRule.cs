namespace gg.parse
{
    public interface IRule
    {
        /// <summary>
        /// Human readable name of the rule.
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Tries to parse the text starting at the specified offset.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="offset"></param>
        /// <returns>An object capturing the result of applying this rule</returns>
        ParseResult Parse(string text, int offset = 0);
    }
}
