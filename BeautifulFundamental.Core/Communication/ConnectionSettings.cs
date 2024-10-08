﻿namespace BeautifulFundamental.Core.Communication
{
	public interface IConnectionSettings
	{
		bool ReconnectActivated { get; init; }
		int ReconnectAttempts { get; init; }
		int ReconnectDelayInSeconds { get; init; }
	}

	public class ConnectionSettings : IConnectionSettings
	{
		private const bool DefaultReconnectActivated = false;
		private const int DefaultReconnectAttempts = 5;
		private const int DefaultReconnectDelayInSeconds = 15;

		public bool ReconnectActivated { get; init; } = DefaultReconnectActivated;
		public int ReconnectAttempts { get; init; } = DefaultReconnectAttempts;
		public int ReconnectDelayInSeconds { get; init; } = DefaultReconnectDelayInSeconds;

		public static ConnectionSettings Default => new()
		{
			ReconnectActivated = DefaultReconnectActivated,
			ReconnectAttempts = DefaultReconnectAttempts,
			ReconnectDelayInSeconds = DefaultReconnectDelayInSeconds
		};
	}
}