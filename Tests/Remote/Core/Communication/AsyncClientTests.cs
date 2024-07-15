using Remote.Core.Communication.Client;
using System.Net.Sockets;

namespace Tests.Remote.Core.Communication
{
	public class TestableSocket(AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType)
		: Socket(addressFamily, socketType, protocolType), ITestableSocket;

	public interface ITestableSocket
	{
	}

	public class AsyncClientTests
	{
		private readonly Socket _dummySocket;
		private readonly IAsyncClient _asyncClient;

		public AsyncClientTests()
		{
			_dummySocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			_asyncClient = AsyncClient.Create(_dummySocket);
		}

		[Fact]
		public void StartReceivingAsync_WhenCalled_ShouldStartReceivingOnSocket()
		{
			// Arrange
			var message = "Hello, World!";
			var receivedMessage = string.Empty;

			_asyncClient.StartReceivingAsync();
		}
	}
}