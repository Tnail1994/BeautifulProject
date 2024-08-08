using System.Net.Security;
using Remote.Communication.Common.Client.Contracts;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;

namespace Remote.Communication.Client
{
	public class AsyncClientFactory : IAsyncClientFactory
	{
		private readonly IAsyncClientFactorySettings _factorySettings;
		private readonly IAsyncClientSettings _settings;
		private TcpClient? _client;
		private SslStream? _sslStream;

		public AsyncClientFactory(IAsyncClientFactorySettings factorySettings, IAsyncClientSettings settings)
		{
			_factorySettings = factorySettings;
			_settings = settings;
		}

		public void Init(TcpClient client, SslStream sslStream)
		{
			if (_client != null && _sslStream != null)
				return;

			_sslStream ??= sslStream;
			_client ??= client;
		}

		public IAsyncClient Create()
		{
			if (_client == null)
			{
				if (!_factorySettings.AutoInit)
					throw new InvalidOperationException("Client is not initialized");

				_client = new TcpClient(_settings.IpAddress, _settings.Port);
			}

			if (_sslStream == null)
			{
				// Todo: Settings to define the validation. If server or client

				_sslStream = new SslStream(_client.GetStream(), false, App_CertificateValidation);
				_sslStream.AuthenticateAsClient("DESKTOP-BPFAUJ0", null, false);
			}

			return AsyncClient.Create(ClientWrapper.Create(_client, _sslStream), _settings);
		}

		private bool App_CertificateValidation(object sender, X509Certificate? certificate, X509Chain? chain,
			SslPolicyErrors sslpolicyerrors)
		{
			return true;
		}
	}
}