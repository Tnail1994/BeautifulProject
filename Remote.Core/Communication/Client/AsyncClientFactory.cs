using Configurations.General.Settings;
using Microsoft.Extensions.Options;
using System.Net.Sockets;

namespace Remote.Core.Communication.Client
{
	public interface IAsyncClientFactory
	{
		IAsyncClient Create(TcpClient client, IOptions<AsyncClientSettings> options);
	}

	public class AsyncClientFactory : IAsyncClientFactory
	{
		public IAsyncClient Create(TcpClient client, IOptions<AsyncClientSettings> options)
		{
			return AsyncClient.Create(ClientWrapper.Create(client), options);
		}
	}
}