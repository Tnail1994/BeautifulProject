using System.Net.Sockets;
using Remote.Communication.Common.Client.Implementations;

namespace Remote.Communication.Common.Client.Contracts
{
	public interface IAsyncClientFactory
	{
		IAsyncClient Create(TcpClient client, AsyncClientSettings settings);
	}
}