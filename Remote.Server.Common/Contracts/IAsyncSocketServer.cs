using System.Net.Sockets;

namespace Remote.Server.Common.Contracts
{
	public interface IAsyncSocketServer
	{
		event Action<TcpClient> NewConnectionOccured;
		Task StartAsync(int port = 8910, int maxListener = 100);
		void Stop();
	}
}