﻿using BeautifulFundamental.Core.Communication;
using BeautifulFundamental.Core.MessageHandling;
using NSubstitute;
using Tests.TestObjects;

namespace Tests.Remote.Communication.MessageHandling
{
	public class AutoSynchronizedMessageHandlerTests
	{
		private readonly ICommunicationService _communicationServiceMock;
		private readonly AutoSynchronizedMessageHandler _autoSynchronizedMessageHandler;

		public AutoSynchronizedMessageHandlerTests()
		{
			_communicationServiceMock = Substitute.For<ICommunicationService>();
			_autoSynchronizedMessageHandler = new AutoSynchronizedMessageHandler(_communicationServiceMock);
		}

		[Fact]
		public void WhenReplyMessageActionIsNull_WhenSubscribing_ThenShouldThrowException()
		{
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
			Assert.Throws<InvalidOperationException>(() =>
				_autoSynchronizedMessageHandler.Subscribe<TestRequestMessage>(null));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
		}

		[Fact]
		public async void WhenCommunicationServiceReceivesRequestedMessageType_ThenShouldFireEvent()
		{
			var testRequestMessage = TestRequestMessage.Create();
			_communicationServiceMock.ReceiveAsync<TestRequestMessage>()
				.Returns(Task.FromResult(testRequestMessage));

			var eventFired = false;
			var id = _autoSynchronizedMessageHandler.Subscribe<TestRequestMessage>(message =>
			{
				eventFired = true;
				Assert.Equal(message, testRequestMessage);
				return TestReplyMessage.Create();
			});

			await Task.Delay(2);
			_autoSynchronizedMessageHandler.Unsubscribe(id);
			Assert.True(eventFired);
		}
	}
}