namespace gg.parse
{

    /// <summary>
    /// Class representing the result of a parsing operation (see IRule.Parse).
    /// </summary>
    public class ParseResult
    {
        /// <summary>
        /// Code representing the outcome for the parsing operation 
        /// </summary>
        public enum Code
        {
            Success,
            Failure
        }

        /// <summary>
        /// Position in the text where the parsing started
        /// </summary>
        public int Offset { get; set; }

        /// <summary>
        /// Number of characters read when the outcome was reached
        /// </summary>
        public int Length { get; set; }

        /// <summary>
        /// Code representing the result of the parsing operation
        /// </summary>
        public Code ResultCode { get; set; }

        /// <summary>
        /// The rule that was used to parse the text
        /// </summary>
        public required IRule Rule { get; set; }

        /// <summary>
        /// In case of a composite rule, this is the sub-rule that was used to parse the text.
        /// </summary>
        public IRule? SubRule { get; set; }
    }
}
