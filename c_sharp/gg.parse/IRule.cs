namespace gg.parse
{
    public interface IRule 
    {
        public enum Output
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

        int Id { get; set; }

        string Name { get; init; }
        
        int Precedence { get; init; }

        IRule.Output Production { get; init; }
    }
}
