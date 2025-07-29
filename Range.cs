namespace gg.parse
{
    public readonly struct Range(int start, int length) 
    {
        public int Start { get; init; } = start;
        
        public int Length { get; init; } = length;

        public override string ToString()
        {
            return $"Range(Start: {Start}, Length: {Length})";
        }
    }
}
