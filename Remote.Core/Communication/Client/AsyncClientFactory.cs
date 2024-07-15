using System.Net.Sockets;

namespace Remote.Core.Communication.Client
{
	public interface IAsyncClientFactory
	{
		IAsyncClient Create(TcpClient client);
	}

	public class AsyncClientFactory : IAsyncClientFactory
	{
		public IAsyncClient Create(TcpClient client)
		{
			return AsyncClient.Create(ClientWrapper.Create(client));
		}
	}
}