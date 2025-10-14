namespace gg.parse
{
    public enum RuleOutput
    {
        /// <summary>
        /// Returns an annotation for the matched item.
        /// </summary>
        Self,

        /// <summary>
        /// Returns the output produced by any child rules.
        /// </summary>
        Children,

        /// <summary>
        /// Does not produce any output (regardless of whether or not it can find a match).
        /// </summary>
        Void
    }

    public static class RuleOutputExtensions
    {
        public static string ToString(this RuleOutput output)
        {
            switch (output)
            {
                case RuleOutput.Self:
                    return "self";
                case RuleOutput.Children:
                    return "children";
                case RuleOutput.Void:
                    return "void";
            }

            throw new NotImplementedException($"Missing ToString for case {output}.");
        }
    }


}
