namespace Session.Common.Contracts
{
	public interface IContextCollection
	{
		string TypeNameOfCollectionEntries { get; }
		IEntryDto? GetEntry(string sessionId);
	}
}