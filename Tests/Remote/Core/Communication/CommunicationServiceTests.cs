using Newtonsoft.Json;
using NSubstitute;
using Remote.Core.Communication;
using Remote.Core.Communication.Client;
using Remote.Core.Implementations;
using Remote.Core.Transformation;
using Xunit.Sdk;

namespace Tests.Remote.Core.Communication
{
	public class CommunicationServiceTests
	{
		private class Test
		{
			public string MockObj { get; set; }
		}

		private class TestMessage : BaseMessage<Test>
		{
		}

		private class OtherTestMessage : BaseMessage<Test>
		{
		}

		private readonly ICommunicationService _communicationService;
		private readonly ITransformerService _transformerServiceMock;
		private readonly IAsyncClient _client;

		public CommunicationServiceTests()
		{
			_transformerServiceMock = Substitute.For<ITransformerService>();
			_communicationService = new CommunicationService(_transformerServiceMock);
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
			Assert.Throws<NullReferenceException>(() => _communicationService.Start());
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
			SetTransformerReturningValue(CreateOtherTestMessage(), nameof(OtherTestMessage));
			RaiseMessageReceivedEvent(CreateOtherTestMessageString());
			var result = await task;

			Assert.NotNull(result);
			Assert.Equal("MockMessage2", result.MessageObject?.MockObj);
		}

		[Fact]
		public void SendAsync_WhenCalled_ShouldSendStringToClient()
		{
			SetClientToCommunicationService();
			StartCommunicationService();
			_communicationService.SendAsync(CreateTestMessage());
			_client.Received(1).Send(Arg.Any<string>());
		}

		[Fact]
		public void SendAsync_WhenNoClientSet_ShouldRaiseNullReferenceException()
		{
			Assert.Throws<NullReferenceException>(() => _communicationService.SendAsync(CreateTestMessage()));
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
			testMessage ??= CreateTestMessage();
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
				testMessageString = CreateTestMessageString();

			_client.MessageReceived += Raise.Event<Action<string>>(testMessageString);
		}

		private static string CreateTestMessageString()
		{
			return JsonConvert.SerializeObject(CreateTestMessage(), JsonConfig.Settings);
		}

		private static string CreateOtherTestMessageString()
		{
			return JsonConvert.SerializeObject(CreateOtherTestMessage(), JsonConfig.Settings);
		}

		private static TestMessage CreateTestMessage()
		{
			return new TestMessage
			{
				MessageObject = new Test { MockObj = "MockMessage" }
			};
		}

		private static OtherTestMessage CreateOtherTestMessage()
		{
			return new OtherTestMessage
			{
				MessageObject = new Test { MockObj = "MockMessage2" }
			};
		}


		private void StartCommunicationService()
		{
			_communicationService.Start();
		}

		#endregion
	}
}