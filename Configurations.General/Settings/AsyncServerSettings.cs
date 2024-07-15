namespace Configurations.General.Settings
{
	public class AsyncServerSettings
	{
		private const int DefaultPort = 8910;
		private const int DefaultMaxConnections = int.MaxValue;
		private const string DefaultIpAddress = "0.0.0.0";


		public int Port { get; init; } = DefaultPort;
		public int MaxConnections { get; init; } = DefaultMaxConnections;
		public string IpAddress { get; init; } = DefaultIpAddress;

		public static AsyncServerSettings Default => new()
		{
			Port = DefaultPort,
			MaxConnections = DefaultMaxConnections,
			IpAddress = DefaultIpAddress
		};
	}
}