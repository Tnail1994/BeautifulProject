namespace Remote.Server.Common.Settings
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

		public static AsyncServerSettings Default => new()
		{
			Port = DefaultPort,
			MaxConnections = DefaultMaxConnections,
			IpAddress = DefaultIpAddress,
			MaxErrorCount = DefaultMaxErrorCount
		};
	}

	public interface IAsyncServerSettings
	{
		int Port { get; init; }
		int MaxConnections { get; init; }
		int MaxErrorCount { get; init; }
		string IpAddress { get; init; }
	}
}