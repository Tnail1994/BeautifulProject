using NSubstitute;
using System.Net.Sockets;
using Remote.Communication.Client;
using Remote.Communication.Common.Client.Contracts;

namespace Tests.Remote.Communication.Client
{
	public class AsyncClientTests
	{
		private readonly IAsyncClient _asyncClient;
		private readonly IClient _clientMock;

		public AsyncClientTests()
		{
			_clientMock = Substitute.For<IClient>();

			_asyncClient = AsyncClient.Create(_clientMock, AsyncClientSettings.Default);
		}

		[Fact]
		public void StartReceivingAsync_WhenCalled_ShouldStartReceivingOnSocket()
		{
			_asyncClient.StartReceivingAsync();
			_clientMock.Received(1).ReceiveAsync(Arg.Any<byte[]>(), Arg.Any<SocketFlags>());
		}

		[Fact]
		public void SendingAString_ShouldCallSendMethodOnSocket()
		{
			_asyncClient.Send("MockMessage");
			_clientMock.Received(1).SendAsync(Arg.Any<byte[]>(), Arg.Any<SocketFlags>());
		}

		[Fact]
		public void Dispose_WhenCalled_ShouldDisposeSocket()
		{
			_asyncClient.Dispose();
			_clientMock.Received(1).Dispose();
		}
	}
}