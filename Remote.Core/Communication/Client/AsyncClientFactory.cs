using System.Net.Sockets;

namespace Remote.Core.Communication.Client
{
	public interface IAsyncClientFactory
	{
		IAsyncClient Create(TcpClient client, AsyncClientSettings settings);
	}

	public class AsyncClientFactory : IAsyncClientFactory
	{
		public IAsyncClient Create(TcpClient client, AsyncClientSettings settings)
		{
			return AsyncClient.Create(ClientWrapper.Create(client), settings);
		}
	}
}