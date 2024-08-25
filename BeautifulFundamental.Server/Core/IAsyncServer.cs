using System.Net.Security;
using System.Net.Sockets;

namespace BeautifulFundamental.Server.Core
{
	public class ConnectionOccurObject
	{
		public string ClientId { get; }
		public TcpClient TcpClient { get; }
		public SslStream SslStream { get; }

		private ConnectionOccurObject(string clientId, TcpClient tcpClient, SslStream sslStream)
		{
			ClientId = clientId;
			TcpClient = tcpClient;
			SslStream = sslStream;
		}

		public static ConnectionOccurObject Create(string clientId, TcpClient client, SslStream sslStream)
		{
			return new ConnectionOccurObject(clientId, client, sslStream);
		}
	}

	public interface IAsyncServer
	{
		event Action<ConnectionOccurObject> NewConnectionOccured;
		Task StartAsync();
		void Remove(string clientId);
	}
}