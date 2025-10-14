namespace gg.parse
{
    public partial interface IRule 
    {
        int Id { get; set; }

        string Name { get; init; }
        
        int Precedence { get; init; }

        RuleOutput Output { get; init; }
    }
}
