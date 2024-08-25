namespace BeautifulFundamental.Server.Db
{
	public interface IDbContextResolver
	{
		IEnumerable<IDbContext> Get();
	}
}