using System.Net.Sockets;

namespace Remote.Core.Communication.Client
{
	public interface ISocket : IDisposable
	{
		Task<int> ReceiveAsync(byte[] buffer, SocketFlags socketFlags);

		Task<int> SendAsync(byte[] buffer, SocketFlags socketFlags);
	}

	public class SocketWrapper : ISocket
	{
		private readonly Socket _socket;

		private SocketWrapper(Socket socket)
		{
			_socket = socket;
		}

		public static ISocket Create(Socket socket)
		{
			return new SocketWrapper(socket);
		}

		public Task<int> ReceiveAsync(byte[] buffer, SocketFlags socketFlags)
		{
			return _socket.ReceiveAsync(buffer, socketFlags);
		}

		public Task<int> SendAsync(byte[] buffer, SocketFlags socketFlags)
		{
			return _socket.SendAsync(buffer, socketFlags);
		}

		public void Dispose()
		{
			_socket.Dispose();
		}
	}
}