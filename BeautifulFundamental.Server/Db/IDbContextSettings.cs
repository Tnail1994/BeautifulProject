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
}