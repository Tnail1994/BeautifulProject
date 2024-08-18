namespace Session.Common.Contracts.Context.Db
{
    public interface IContextCollection
    {
        string TypeNameOfCollectionEntries { get; }
        IEntryDto? GetEntry(string sessionId);
    }
}