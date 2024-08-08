using DbManagement.Common.Contracts;

namespace DbManagement
{
	public class DbContextSettings : IDbContextSettings
	{
		private const int DefaultPort = 5432;
		private const string DefaultServerAdresse = "localhost";
		private const string DefaultDatabaseName = "BeautifulDb";
		private const string DefaultUserId = "someUserId";
		private const string DefaultPassword = "somePassword";
		private const int DefaultUpdateDbDelayInMs = 1000;
		private const bool DefaultAnalyzeUpdateSet = false;

		public string ServerAdresse { get; init; } = DefaultServerAdresse;
		public int Port { get; init; } = DefaultPort;
		public string DatabaseName { get; init; } = DefaultDatabaseName;
		public string UserId { get; init; } = DefaultUserId;
		public string Password { get; init; } = DefaultPassword;
		public int UpdateDbDelayInMs { get; init; } = DefaultUpdateDbDelayInMs;
		public bool AnalyzeUpdateSet { get; init; } = DefaultAnalyzeUpdateSet;

		public static IDbContextSettings Default => new DbContextSettings()
		{
			ServerAdresse = DefaultServerAdresse,
			Port = DefaultPort,
			DatabaseName = DefaultDatabaseName,
			UserId = DefaultUserId,
			Password = DefaultPassword,
			UpdateDbDelayInMs = DefaultUpdateDbDelayInMs,
			AnalyzeUpdateSet = DefaultAnalyzeUpdateSet
		};
	}
}