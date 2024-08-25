namespace BeautifulFundamental.Server.Session.Contracts.Context.Db
{
	public interface IContextCollection
	{
		string TypeNameOfCollectionEntries { get; }
		IEntryDto? GetEntry(string sessionId);
	}
}