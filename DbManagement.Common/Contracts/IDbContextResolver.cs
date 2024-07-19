namespace DbManagement.Common.Contracts
{
	public interface IDbContextResolver
	{
		IEnumerable<IDbContext> Get();
	}
}