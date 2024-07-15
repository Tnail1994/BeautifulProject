using NSubstitute;
using Remote.Core.Communication.Client;
using System.Net.Sockets;

namespace Tests.Remote.Core.Communication
{
	public class AsyncClientTests
	{
		private readonly IAsyncClient _asyncClient;
		private readonly ISocket _socketMock;

		public AsyncClientTests()
		{
			_socketMock = Substitute.For<ISocket>();
			_asyncClient = AsyncClient.Create(_socketMock);
		}

		[Fact]
		public void StartReceivingAsync_WhenCalled_ShouldStartReceivingOnSocket()
		{
			_asyncClient.StartReceivingAsync();
			_socketMock.Received(1).ReceiveAsync(Arg.Any<byte[]>(), Arg.Any<SocketFlags>());
		}

		[Fact]
		public void SendingAString_ShouldCallSendMethodOnSocket()
		{
			_asyncClient.Send("MockMessage");
			_socketMock.Received(1).SendAsync(Arg.Any<byte[]>(), Arg.Any<SocketFlags>());
		}

		[Fact]
		public void Dispose_WhenCalled_ShouldDisposeSocket()
		{
			_asyncClient.Dispose();
			_socketMock.Received(1).Dispose();
		}
	}
}