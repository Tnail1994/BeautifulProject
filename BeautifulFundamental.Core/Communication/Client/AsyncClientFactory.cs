using System.Net.Security;
using System.Net.Sockets;

namespace BeautifulFundamental.Core.Communication.Client
{
	public interface IAsyncClientFactory
	{
		void Init(TcpClient? client = null, SslStream? sslStream = null, bool isServerClient = true);

		IAsyncClient Create();
	}

	public class AsyncClientFactory : IAsyncClientFactory
	{
		private readonly IAsyncClientSettings _settings;
		private readonly ITlsSettings _tlsSettings;

		private TcpClient? _client;
		private SslStream? _sslStream;
		private bool _isServerClient;

		public AsyncClientFactory(IAsyncClientSettings settings,
			ITlsSettings tlsSettings)
		{
			_settings = settings;
			_tlsSettings = tlsSettings;
		}

		public void Init(TcpClient? client = null, SslStream? sslStream = null, bool isServerClient = true)
		{
			if (_client != null && _sslStream != null)
				return;

			_sslStream ??= sslStream;
			_client ??= client;
			_isServerClient = isServerClient;
		}

		/// <summary>
		/// Creating a 
		/// </summary>
		/// <returns></returns>
		/// <exception cref="InvalidOperationException">When isServerClient, but _client and _sslStream null</exception>
		public IAsyncClient Create()
		{
			return _isServerClient ? CreateAsyncServerClient() : CreateAsyncClient();
		}

		private IAsyncClient CreateAsyncClient()
		{
			return AsyncClient.Create(TlsClient.CreateClient(_settings.IpAddress, _settings.Port, _tlsSettings),
				_settings);
		}

		/// <summary>
		/// </summary>
		/// <returns></returns>
		/// <exception cref="InvalidOperationException"></exception>
		private IAsyncClient CreateAsyncServerClient()
		{
			if (_client == null || _sslStream == null)
				throw new InvalidOperationException("Cannot create AsyncClient without _client and _sslStream set.");

			return AsyncClient.Create(TlsClient.CreateServerClient(_client, _sslStream, _tlsSettings), _settings);
		}
	}
}