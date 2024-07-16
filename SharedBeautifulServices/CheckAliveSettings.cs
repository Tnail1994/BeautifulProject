using SharedBeautifulServices.Common;

namespace SharedBeautifulServices
{
	public class CheckAliveSettings : ICheckAliveSettings
	{
		private const bool DefaultEnabled = true;
		private const int DefaultFrequencyInSeconds = 30;
		private const int DefaultMode = 1;

		public bool Enabled { get; init; } = DefaultEnabled;
		public int FrequencyInSeconds { get; init; } = DefaultFrequencyInSeconds;
		public int Mode { get; init; } = DefaultMode;

		public static CheckAliveSettings Default => new()
		{
			Enabled = DefaultEnabled,
			FrequencyInSeconds = DefaultFrequencyInSeconds,
			Mode = DefaultMode
		};
	}
}