﻿using BeautifulFundamental.Core.Communication.Client;
using NSubstitute;

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
			_clientMock.Received(1).ReceiveAsync(Arg.Any<byte[]>());
		}

		[Fact]
		public void SendingAString_ShouldCallSendMethodOnSocket()
		{
			_asyncClient.Send("MockMessage");
			_clientMock.Received(1).SendAsync(Arg.Any<byte[]>());
		}
	}
}