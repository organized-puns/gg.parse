namespace gg.parse.script.compiler
{
    public class RuleReferenceException : Exception
    {
        public int RuleId { get; init; }

        public string RuleName { get; init; }

        public RuleReferenceException(string message, int id)
            : base(message)
        {
            RuleId = id;
            RuleName = string.Empty;
        }

        public RuleReferenceException(int id) 
            : base($"No able to find rule id ({id}).")
        {
            RuleId = id;
            RuleName = string.Empty;
        }

        public RuleReferenceException(string name)
            : base($"No able to find rule name ({name}).")
        {
            RuleId = -1;
            RuleName = name;
        }

        public RuleReferenceException(string message, string name)
            : base(message)
        {
            RuleId = -1;
            RuleName = name;
        }
    }
}
