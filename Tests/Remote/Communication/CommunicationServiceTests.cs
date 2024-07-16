using NSubstitute;
using Remote.Communication;
using Remote.Communication.Common.Client.Contracts;
using Remote.Communication.Common.Contracts;
using Remote.Communication.Common.Transformation.Contracts;
using Remote.Communication.Common.Transformation.Implementations;
using Session.Common.Implementations;
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
			var sessionKeyMock = Substitute.For<ISessionKey>();
			_communicationService = new CommunicationService(_transformerServiceMock, sessionKeyMock);
			_client = Substitute.For<IAsyncClient>();
		}

		[Fact]
		public void SetClient_WhenClientIsNotSet_ShouldSetClient()
		{
			Assert.False(_communicationService.IsClientSet);
			SetClientToCommunicationService();
			Assert.True(_communicationService.IsClientSet);
		}

		[Fact]
		public void Start_WhenClientIsSet_ShouldStartReceivingOnClient()
		{
			SetClientToCommunicationService();
			StartCommunicationService();
			_client.Received(1).StartReceivingAsync();
		}

		[Fact]
		public void Start_WhenClientIsNotSet_ShouldThrowANullReferenceException()
		{
			Assert.ThrowsAsync<NullReferenceException>(() => _communicationService.Start());
		}

		[Fact]
		public void WhenMessagedReceivedEventOccurs_ThenTransform_OnTransformerServiceShouldGetCalled()
		{
			SetTransformerReturningValue();
			SetClientToCommunicationService();
			StartCommunicationService();
			RaiseMessageReceivedEvent();
			_transformerServiceMock.Received(1).Transform(Arg.Any<string>());
		}

		[Fact]
		public async void ReceiveAsync_WhenSuitableTransformedMessageIsAdded_ThenShouldReturnThis()
		{
			SetTransformerReturningValue();
			SetClientToCommunicationService();
			StartCommunicationService();
			RaiseMessageReceivedEvent();

			var result = await _communicationService.ReceiveAsync<TestMessage>();

			Assert.NotNull(result);
			Assert.Equal("MockMessage", result.MessageObject?.MockObj);
		}

		[Fact]
		public async void ReceiveAsync_WhenUnsuitableTransformedMessageIsAdded_ThenShouldWaitUntilSuitableMessageAdded()
		{
			SetTransformerReturningValue();
			SetClientToCommunicationService();
			StartCommunicationService();
			RaiseMessageReceivedEvent();

			var task = _communicationService.ReceiveAsync<OtherTestMessage>();

			await Task.Delay(10);
			SetTransformerReturningValue(OtherTestMessage.Create(), nameof(OtherTestMessage));
			RaiseMessageReceivedEvent(OtherTestMessage.CreateString());
			var result = await task;

			Assert.NotNull(result);
			Assert.Equal("MockMessage2", result.MessageObject?.MockObj);
		}

		[Fact]
		public void SendAsync_WhenCalled_ShouldSendStringToClient()
		{
			SetClientToCommunicationService();
			StartCommunicationService();
			_communicationService.SendAsync(TestMessage.Create());
			_client.Received(1).Send(Arg.Any<string>());
		}

		[Fact]
		public void SendAsync_WhenNoClientSet_ShouldRaiseNullReferenceException()
		{
			Assert.Throws<NullReferenceException>(() => _communicationService.SendAsync(TestMessage.Create()));
		}

		[Fact]
		public void Dispose_WhenCalled_ShouldNotDisposeClient()
		{
			SetClientToCommunicationService();
			StartCommunicationService();
			_communicationService.Dispose();
			_client.Received(1).Dispose();
		}

		[Fact]
		public void Dispose_WhenCalledAndNotClientSet_ShouldNotDisposeClient()
		{
			_communicationService.Dispose();
			_transformerServiceMock.DidNotReceive().Dispose();
		}

		#region Helper

		private void SetTransformerReturningValue(object? testMessage = null, string? testMessageName = null)
		{
			testMessage ??= TestMessage.Create();
			testMessageName ??= nameof(TestMessage);

			_transformerServiceMock.Transform(Arg.Any<string>())
				.Returns(TransformedObject.Create(testMessage, testMessageName));
		}

		private void SetClientToCommunicationService()
		{
			_communicationService.SetClient(_client);
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