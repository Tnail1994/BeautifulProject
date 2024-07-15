using Configurations.General.Settings;
using System.Net.Sockets;

namespace Remote.Core.Communication.Client
{
	public interface IAsyncClientFactory
	{
		IAsyncClient Create(TcpClient client, IAsyncClientSettings settings);
	}

	public class AsyncClientFactory : IAsyncClientFactory
	{
		public IAsyncClient Create(TcpClient client, IAsyncClientSettings settings)
		{
			return AsyncClient.Create(ClientWrapper.Create(client), settings);
		}
	}
}