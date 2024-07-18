using Remote.Communication.Common.Client.Contracts;

namespace Remote.Communication.Client;

public class AsyncClientFactorySettings : IAsyncClientFactorySettings
{
	private const bool DefaultAutoInit = false;
	public bool AutoInit { get; init; } = DefaultAutoInit;

	public static AsyncClientFactorySettings Default => new()
	{
		AutoInit = DefaultAutoInit
	};
}