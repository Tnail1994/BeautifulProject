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

		public string ServerAdresse { get; init; } = DefaultServerAdresse;
		public int Port { get; init; } = DefaultPort;
		public string DatabaseName { get; init; } = DefaultDatabaseName;
		public string UserId { get; init; } = DefaultUserId;
		public string Password { get; init; } = DefaultPassword;

		public static IDbContextSettings Default => new DbContextSettings()
		{
			ServerAdresse = DefaultServerAdresse,
			Port = DefaultPort,
			DatabaseName = DefaultDatabaseName,
			UserId = DefaultUserId,
			Password = DefaultPassword
		};
	}
}