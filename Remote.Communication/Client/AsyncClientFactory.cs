using Remote.Communication.Common.Client.Contracts;
using System.Net.Sockets;

namespace Remote.Communication.Client
{
	public class AsyncClientFactory : IAsyncClientFactory
	{
		private readonly IAsyncClientFactorySettings _factorySettings;
		private readonly IAsyncClientSettings _settings;
		private TcpClient? _client;

		public AsyncClientFactory(IAsyncClientFactorySettings factorySettings, IAsyncClientSettings settings)
		{
			_factorySettings = factorySettings;
			_settings = settings;
		}

		public void Init(TcpClient client)
		{
			if (_client != null)
				return;

			_client = client;
		}

		public IAsyncClient Create()
		{
			if (_client == null)
			{
				if (!_factorySettings.AutoInit)
					throw new InvalidOperationException("Client is not initialized");

				_client = new TcpClient();
			}

			return AsyncClient.Create(ClientWrapper.Create(_client), _settings);
		}
	}
}