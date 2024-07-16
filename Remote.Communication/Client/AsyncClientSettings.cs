using Remote.Communication.Common.Client.Contracts;

namespace Remote.Communication.Client
{
	public class AsyncClientSettings : IAsyncClientSettings
	{
		private const int DefaultBufferSize = 4096;
		private const int DefaultClientTimeout = 5;
		private const string DefaultIpAddress = "0.0.0.0";
		private const int DefaultPort = 8910;

		public int BufferSize { get; init; } = DefaultBufferSize;
		public int ClientTimeout { get; init; } = DefaultClientTimeout;
		public string IpAddress { get; init; } = DefaultIpAddress;
		public int Port { get; init; } = DefaultPort;

		public static AsyncClientSettings Default => new()
		{
			BufferSize = DefaultBufferSize,
			ClientTimeout = DefaultClientTimeout, // Minutes
			IpAddress = DefaultIpAddress,
			Port = DefaultPort
		};
	}
}