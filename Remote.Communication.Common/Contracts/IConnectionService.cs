namespace Remote.Communication.Common.Contracts
{
	public interface IConnectionService : IDisposable
	{
		event Action<string> ConnectionLost;
		event Action Reconnected;
		void Start();
		void Stop();
	}
}