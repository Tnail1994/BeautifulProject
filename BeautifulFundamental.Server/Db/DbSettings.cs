namespace BeautifulFundamental.Server.Db
{
	public interface IDbSettings
	{
		int CachingTimeInSeconds { get; init; }
	}

	public class DbSettings : IDbSettings
	{
		private const int DefaultCachingTimeInSeconds = 60;

		public int CachingTimeInSeconds { get; init; } = DefaultCachingTimeInSeconds;

		public static IDbSettings Default => new DbSettings()
		{
			CachingTimeInSeconds = DefaultCachingTimeInSeconds,
		};
	}
}