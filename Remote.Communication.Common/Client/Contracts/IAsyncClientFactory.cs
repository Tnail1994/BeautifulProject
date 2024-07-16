using System.Net.Sockets;

namespace Remote.Communication.Common.Client.Contracts
{
	public interface IAsyncClientFactory
	{
		IAsyncClient Create();
		IAsyncClient Create(TcpClient client);
	}
}