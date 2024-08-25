namespace BeautifulFundamental.Server.Db
{
	public interface IDbSettings
	{
		int CachingTimeInSeconds { get; init; }
	}
}