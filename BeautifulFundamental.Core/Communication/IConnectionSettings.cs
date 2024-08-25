namespace BeautifulFundamental.Core.Communication
{
	public interface IConnectionSettings
	{
		bool ReconnectActivated { get; init; }
		int ReconnectAttempts { get; init; }
		int ReconnectDelayInSeconds { get; init; }
	}
}