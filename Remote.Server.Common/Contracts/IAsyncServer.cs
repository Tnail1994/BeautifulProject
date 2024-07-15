using System.Net.Sockets;

namespace Remote.Server.Common.Contracts
{
	public interface IAsyncServer
	{
		event Action<TcpClient> NewConnectionOccured;
		Task StartAsync();
		void Stop();
	}
}