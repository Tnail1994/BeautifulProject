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

			if (_sslStream != null)
				return CreateAsyncClient();

			if (!_settings.IsServerClient)
			{
				var clientCertificate = new X509Certificate2(_settings.TlsSettingsObj.CertificatePath);
				var clientCertificateCollection =
					new X509CertificateCollection(new X509Certificate[] { clientCertificate });

				_sslStream = new SslStream(_client.GetStream(), _settings.TlsSettingsObj.LeaveInnerStreamOpen,
					ValidateAsClient);
				_sslStream.AuthenticateAsClient(_settings.TlsSettingsObj.TargetHost, clientCertificateCollection,
					_settings.TlsSettingsObj.CheckCertificateRevocation);
			}

			return CreateAsyncClient();
		}

		/// <summary>
		/// Exceptions:
		/// - InvalidOperationException
		/// </summary>
		/// <returns></returns>
		/// <exception cref="InvalidOperationException"></exception>
		private IAsyncClient CreateAsyncClient()
		{
			if (_client == null || _sslStream == null)
				throw new InvalidOperationException("Cannot create AsyncClient without _client and _sslStream set.");

			return AsyncClient.Create(ClientWrapper.Create(_client, _sslStream), _settings);
		}

		private bool ValidateAsClient(object sender, X509Certificate? certificate, X509Chain? chain,
			SslPolicyErrors sslPolicyErrors)
		{
			return sslPolicyErrors == SslPolicyErrors.None ||
			       (_settings.TlsSettingsObj.AllowRemoteCertificateChainErrors &&
			        sslPolicyErrors == SslPolicyErrors.RemoteCertificateChainErrors);
		}
	}
}