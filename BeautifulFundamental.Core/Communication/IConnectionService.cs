namespace BeautifulFundamental.Core.Communication
{
	public interface IConnectionService
	{
		event Action ConnectionEstablished;
		event Action<string> ConnectionLost;
		event Action Reconnected;
		void Start();
		void Stop(bool force = false);
	}
}