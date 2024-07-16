using System.Net.Sockets;
using Remote.Communication.Common.Client.Contracts;
using Remote.Communication.Common.Client.Implementations;

namespace Remote.Communication.Client
{
	public class AsyncClientFactory : IAsyncClientFactory
	{
		public IAsyncClient Create(TcpClient client, AsyncClientSettings settings)
		{
			return AsyncClient.Create(ClientWrapper.Create(client), settings);
		}
	}
}