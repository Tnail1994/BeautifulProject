namespace DbManagement.Common.Contracts
{
	public interface IDbContext
	{
		string Id { get; }
		string TypeNameOfCollectionEntries { get; }
		IEnumerable<object>? GetEntities();
	}
}