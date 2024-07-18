namespace Remote.Communication.Common.Client.Contracts
{
	public interface IAsyncClient : IDisposable
	{
		string Id { get; }
		bool IsNotConnected { get; }
		bool IsConnected { get; }
		event Action<string> MessageReceived;
		event EventHandler<string> ConnectionLost;

		Task<bool> ConnectAsync();
		void StartReceivingAsync();
		void Send(string message);
	}
}