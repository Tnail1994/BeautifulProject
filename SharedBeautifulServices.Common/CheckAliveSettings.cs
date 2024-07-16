namespace SharedBeautifulServices.Common
{
	public class CheckAliveSettings : ICheckAliveSettings
	{
		private const bool DefaultEnabled = true;
		private const int DefaultFrequencyInSeconds = 30;

		public bool Enabled { get; init; } = DefaultEnabled;
		public int FrequencyInSeconds { get; init; } = DefaultFrequencyInSeconds;

		public static CheckAliveSettings Default => new()
		{
			Enabled = DefaultEnabled,
			FrequencyInSeconds = DefaultFrequencyInSeconds,
		};
	}

	public interface ICheckAliveSettings
	{
		bool Enabled { get; init; }
		int FrequencyInSeconds { get; init; }
	}
}