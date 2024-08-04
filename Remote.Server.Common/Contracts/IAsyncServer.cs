using System.Net.Sockets;

namespace Remote.Server.Common.Contracts
{
	public interface IAsyncServer
	{
		event Action<KeyValuePair<string, TcpClient>> NewConnectionOccured;
		Task StartAsync();
		void Remove(string clientId);
	}
}