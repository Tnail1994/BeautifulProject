using Remote.Communication.Client;
using Remote.Communication.Common.Client.Contracts;
using Remote.Server.Common.Contracts;

namespace Remote.Server
{
	public class AsyncServerSettings : IAsyncServerSettings
	{
		private const int DefaultPort = 8910;
		private const int DefaultMaxConnections = int.MaxValue;
		private const int DefaultMaxErrorCount = 1000;
		private const string DefaultIpAddress = "0.0.0.0";


		public int Port { get; init; } = DefaultPort;
		public int MaxConnections { get; init; } = DefaultMaxConnections;
		public int MaxErrorCount { get; init; } = DefaultMaxErrorCount;
		public string IpAddress { get; init; } = DefaultIpAddress;

		public ITlsSettings TlsSettingsObj { get; init; } = TlsSettings.Default;

		public static IAsyncServerSettings Default => new AsyncServerSettings();
	}
}