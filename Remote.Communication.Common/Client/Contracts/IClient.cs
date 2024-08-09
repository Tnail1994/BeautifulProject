namespace Remote.Communication.Common.Client.Contracts
{
	public interface IClient : IDisposable
	{
		public bool Connected { get; }
		bool IsNotConnected { get; }
		Task<int> ReceiveAsync(byte[] buffer);

		Task<int> SendAsync(byte[] buffer);
		Task<bool> ConnectAsync(string ip, int port);
		void ResetSocket();
	}
}