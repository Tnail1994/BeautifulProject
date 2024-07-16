using System.Net.Sockets;
using Remote.Communication.Common.Client.Contracts;

namespace Remote.Communication.Client
{
	public class ClientWrapper : IClient
	{
		private readonly TcpClient _client;

		private ClientWrapper(TcpClient client)
		{
			_client = client;
		}

		public static IClient Create(TcpClient client)
		{
			return new ClientWrapper(client);
		}

		public bool Connected => _client.Connected;

		public bool IsNotConnected => !Connected;

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