﻿using BeautifulFundamental.Core.Communication;
using BeautifulFundamental.Core.Communication.Client;
using BeautifulFundamental.Core.Communication.Transformation;
using BeautifulFundamental.Core.Communication.Transformation.Implementations;
using BeautifulFundamental.Core.Identification;
using NSubstitute;
using Tests.TestObjects;

namespace Tests.Remote.Communication
{
	public class CommunicationServiceTests
	{
		private readonly ICommunicationService _communicationService;
		private readonly ITransformerService _transformerServiceMock;
		private readonly IAsyncClient _client;

		public CommunicationServiceTests()
		{
			_transformerServiceMock = Substitute.For<ITransformerService>();
			var sessionKeyMock = Substitute.For<IIdentificationKey>();
			_client = Substitute.For<IAsyncClient>();
			_communicationService = new CommunicationService(_client, _transformerServiceMock, sessionKeyMock);
		}

		[Fact]
		public void Start_WhenClientIsSet_ShouldStartReceivingOnClient()
		{
			StartCommunicationService();
			_client.Received(1).StartReceivingAsync();
		}

		[Fact]
		public void WhenMessagedReceivedEventOccurs_ThenTransform_OnTransformerServiceShouldGetCalled()
		{
			SetTransformerReturningValue();
			StartCommunicationService();
			RaiseMessageReceivedEvent();
			_transformerServiceMock.Received(1).Transform(Arg.Any<string>());
		}

		[Fact]
		public async void ReceiveAsync_WhenSuitableTransformedMessageIsAdded_ThenShouldReturnThis()
		{
			SetTransformerReturningValue();
			StartCommunicationService();
			RaiseMessageReceivedEvent();

			var result = await _communicationService.ReceiveAsync<TestMessage>();

			Assert.NotNull(result);
			Assert.Equal("MockMessage", result.TestObject.MockObj);
		}

		[Fact]
		public async void ReceiveAsync_WhenUnsuitableTransformedMessageIsAdded_ThenShouldWaitUntilSuitableMessageAdded()
		{
			SetTransformerReturningValue();
			StartCommunicationService();
			RaiseMessageReceivedEvent();

			var task = _communicationService.ReceiveAsync<OtherTestMessage>();

			await Task.Delay(10);
			SetTransformerReturningValue(OtherTestMessage.Create(), nameof(OtherTestMessage));
			RaiseMessageReceivedEvent(OtherTestMessage.CreateString());
			var result = await task;

			Assert.NotNull(result);
			Assert.Equal("MockMessage2", result.TestObject.MockObj);
		}

		[Fact]
		public void SendAsync_WhenCalled_ShouldSendStringToClient()
		{
			StartCommunicationService();
			_communicationService.SendAsync(TestMessage.Create());
			_client.Received(1).Send(Arg.Any<string>());
		}

		#region Helper

		private void SetTransformerReturningValue(object? testMessage = null, string? testMessageName = null)
		{
			testMessage ??= TestMessage.Create();
			testMessageName ??= nameof(TestMessage);

			_transformerServiceMock.Transform(Arg.Any<string>())
				.Returns(TransformedObject.Create(testMessage, testMessageName));
		}

		private void RaiseMessageReceivedEvent(string testMessageString = "")
		{
			if (string.IsNullOrEmpty(testMessageString))
				testMessageString = TestMessage.CreateString();

			_client.MessageReceived += Raise.Event<Action<string>>(testMessageString);
		}


		private void StartCommunicationService()
		{
			_communicationService.Start();
		}

		#endregion
	}
}