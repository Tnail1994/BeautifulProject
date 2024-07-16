namespace SharedBeautifulServices.Common
{
	public interface ICheckAliveSettings
	{
		bool Enabled { get; init; }
		int FrequencyInSeconds { get; init; }

		/// <summary>
		/// 0 - replies to check alive messages
		/// 1 - sends check alive messages
		/// </summary>
		int Mode { get; init; }
	}
}