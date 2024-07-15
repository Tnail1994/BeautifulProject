using System.Net.Sockets;

namespace Remote.Core.Communication.Client
{
	public interface ISocket : IDisposable
	{
		Task<int> ReceiveAsync(byte[] buffer, SocketFlags socketFlags);

		Task<int> SendAsync(byte[] buffer, SocketFlags socketFlags);
	}

	public class ClientWrapper : ISocket
	{
		private readonly TcpClient _client;

		private ClientWrapper(TcpClient client)
		{
			_client = client;
		}

		public static ISocket Create(TcpClient client)
		{
			return new ClientWrapper(client);
		}

		public Task<int> ReceiveAsync(byte[] buffer, SocketFlags socketFlags)
		{
			return _client.Client.ReceiveAsync(buffer, socketFlags);
		}

		public Task<int> SendAsync(byte[] buffer, SocketFlags socketFlags)
		{
			return _client.Client.SendAsync(buffer, socketFlags);
		}

		public void Dispose()
		{
			_client.Dispose();
		}
	}
}