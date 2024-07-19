using DbManagement.Common.Contracts;

namespace DbManagement
{
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