namespace Remote.Communication.Common.Client.Contracts
{
	public interface IAsyncClient : IDisposable
	{
		string Id { get; }
		event Action<string> MessageReceived;
		event EventHandler<string> ConnectionLost;
		void StartReceivingAsync();
		void Send(string message);
	}
}