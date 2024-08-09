namespace Remote.Communication.Common.Client.Contracts
{
	public interface IAsyncClientSettings
	{
		int BufferSize { get; init; }
		int ClientTimeout { get; init; }
		string IpAddress { get; init; }
		int Port { get; init; }

		/// <summary>
		/// Should this client operate on the server or client side?
		/// </summary>
		bool IsServerClient { get; init; }

		ITlsSettings TlsSettingsObj { get; init; }
	}
}