namespace gg.parse
{
    public interface IMetaRule : IRule
    {
        IRule? Subject { get; }

        IMetaRule CloneWithSubject(IRule subject);

        void MutateSubject(IRule subject);
    }
}
