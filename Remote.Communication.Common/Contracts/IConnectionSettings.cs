namespace Remote.Communication.Common.Contracts
{
	public interface IConnectionSettings
	{
		bool ReconnectActivated { get; init; }
		int ReconnectAttempts { get; init; }
		int ReconnectDelayInSeconds { get; init; }
	}
}