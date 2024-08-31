namespace BeautifulFundamental.Server.Db
{
	public interface IDbContextSettings
	{
		string ServerAdresse { get; init; }
		int Port { get; init; }
		string DatabaseName { get; init; }
		string UserId { get; init; }
		string Password { get; init; }

		/// <summary>
		/// How long the task wait to look up the updateQueue and do the tasks.
		/// </summary>
		int UpdateDbDelayInMs { get; init; }

		/// <summary>
		/// Maybe not the greatest name. It means: Check, if there is any redundant update for e.g.:
		/// 1) Adding and removing the same item -> Delete both entries. Obsolete
		/// 2) Changing and removing the same item -> Just remove the entries.
		/// 3) And any other combinations of 1 and 2 like Add, change, change, remove
		/// Hint: Setting th
		/// </summary>
		bool AnalyzeUpdateSet { get; init; }

		/// <summary>
		/// Whenever the update loop is running it counts ++.
		/// If the counter has reached this threshold, then it executes
		/// updating from database. Means: Do you have a delay timer of 1000ms (1s)
		/// and setting this value to 10, means it executes every 10 seconds.
		/// </summary>
		int UpdateChangesFromDbThreshold { get; init; }
	}

	public class DbContextSettings : IDbContextSettings
	{
		private const int DefaultPort = 5432;
		private const string DefaultServerAdresse = "localhost";
		private const string DefaultDatabaseName = "BeautifulDb";
		private const string DefaultUserId = "someUserId";
		private const string DefaultPassword = "somePassword";
		private const int DefaultUpdateDbDelayInMs = 1000;
		private const int DefaultUpdateChangesFromDbThreshold = 3600;

		public string ServerAdresse { get; init; } = DefaultServerAdresse;
		public int Port { get; init; } = DefaultPort;
		public string DatabaseName { get; init; } = DefaultDatabaseName;
		public string UserId { get; init; } = DefaultUserId;
		public string Password { get; init; } = DefaultPassword;
		public int UpdateDbDelayInMs { get; init; } = DefaultUpdateDbDelayInMs;
		public bool AnalyzeUpdateSet { get; init; }
		public int UpdateChangesFromDbThreshold { get; init; } = DefaultUpdateChangesFromDbThreshold;

		public static IDbContextSettings Default => new DbContextSettings();
	}
}