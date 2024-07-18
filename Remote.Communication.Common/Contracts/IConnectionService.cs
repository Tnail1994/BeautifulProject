namespace Remote.Communication.Common.Contracts
{
	public interface IConnectionService
	{
		event Action<string> ConnectionLost;
		event Action Reconnected;
		void Start();
	}
}