namespace Remote.Communication.Common.Client.Contracts
{
	public interface IAsyncClientSettings
	{
		int BufferSize { get; init; }
		int ClientTimeout { get; init; }
		string IpAddress { get; init; }
		int Port { get; init; }
	}
}