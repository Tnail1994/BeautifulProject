using System.Net.Sockets;

namespace Remote.Communication.Common.Client.Contracts
{
	public interface IClient : IDisposable
	{
		public bool Connected { get; }
		bool IsNotConnected { get; }
		Task<int> ReceiveAsync(byte[] buffer, SocketFlags socketFlags);

		Task<int> SendAsync(byte[] buffer, SocketFlags socketFlags);
		Task<bool> ConnectAsync(string ip, int port);
	}
}