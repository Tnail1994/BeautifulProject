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

		public async Task<bool> ConnectAsync(string ip, int port)
		{
			await _client.ConnectAsync(ip, port);
			return _client.Connected;
		}

		public void ResetSocket()
		{
			_client.Client = new TcpClient().Client;
		}

		public void Close()
		{
			_client.Close();
		}

		public void Dispose()
		{
			_client.Dispose();
		}
	}
}