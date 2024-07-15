namespace Configurations.General.Settings
{
	public class AsyncClientSettings : IAsyncClientSettings
	{
		private const int DefaultBufferSize = 4096;
		private const int DefaultClientTimeout = 5;

		public int BufferSize { get; init; } = DefaultBufferSize;
		public int ClientTimeout { get; init; } = DefaultClientTimeout;

		public static AsyncClientSettings Default => new()
		{
			BufferSize = DefaultBufferSize,
			ClientTimeout = DefaultClientTimeout // Minutes
		};
	}

	public interface IAsyncClientSettings
	{
		int BufferSize { get; init; }
		int ClientTimeout { get; init; }
	}
}