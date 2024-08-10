using DbManagement.Common.Contracts;
using DbManagement.Common.Implementations;

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
		private const int DefaultUpdateChangesFromDbThreshold = 3600;
		private const DbContextSyncMode DefaultSyncMode = DbContextSyncMode.LocalEntitiesOnly;

		public string ServerAdresse { get; init; } = DefaultServerAdresse;
		public int Port { get; init; } = DefaultPort;
		public string DatabaseName { get; init; } = DefaultDatabaseName;
		public string UserId { get; init; } = DefaultUserId;
		public string Password { get; init; } = DefaultPassword;
		public int UpdateDbDelayInMs { get; init; } = DefaultUpdateDbDelayInMs;
		public bool AnalyzeUpdateSet { get; init; }
		public DbContextSyncMode SyncMode { get; init; } = DefaultSyncMode;
		public int UpdateChangesFromDbThreshold { get; init; } = DefaultUpdateChangesFromDbThreshold;

		public static IDbContextSettings Default => new DbContextSettings();
	}
}