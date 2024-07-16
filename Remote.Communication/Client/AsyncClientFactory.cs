using Remote.Communication.Common.Client.Contracts;
using Remote.Communication.Common.Client.Implementations;
using System.Net.Sockets;

namespace Remote.Communication.Client
{
	public class AsyncClientFactory : IAsyncClientFactory
	{
		private readonly AsyncClientSettings _settings;

		public AsyncClientFactory(AsyncClientSettings settings)
		{
			_settings = settings;
		}

		public IAsyncClient Create()
		{
			return AsyncClient.Create(ClientWrapper.Create(new TcpClient()), _settings);
		}

		public IAsyncClient Create(TcpClient client)
		{
			return AsyncClient.Create(ClientWrapper.Create(client), _settings);
		}
	}
}