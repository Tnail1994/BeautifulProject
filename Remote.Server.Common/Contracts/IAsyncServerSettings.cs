using Remote.Communication.Common.Client.Contracts;

namespace Remote.Server.Common.Contracts
{
	public interface IAsyncServerSettings
	{
		int Port { get; init; }
		int MaxConnections { get; init; }
		int MaxErrorCount { get; init; }
		string IpAddress { get; init; }
		ITlsSettings TlsSettingsObj { get; init; }
	}
}