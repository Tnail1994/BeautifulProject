using Configurations.General.Settings;
using Microsoft.Extensions.Options;
using NSubstitute;
using Remote.Core.Communication.Client;
using System.Net.Sockets;

namespace Tests.Remote.Core.Communication.Client
{
	public class AsyncClientTests
	{
		private readonly IAsyncClient _asyncClient;
		private readonly IClient _clientMock;

		public AsyncClientTests()
		{
			_clientMock = Substitute.For<IClient>();

			IOptions<AsyncClientSettings> optionsMock = Substitute.For<IOptions<AsyncClientSettings>>();
			optionsMock.Value.Returns(AsyncClientSettings.Default);
			_asyncClient = AsyncClient.Create(_clientMock, optionsMock);
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