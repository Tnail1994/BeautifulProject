using BeautifulFundamental.Core.Communication;
using BeautifulFundamental.Core.Identification;
using BeautifulFundamental.Core.Messages.CheckAlive;
using BeautifulFundamental.Core.Services.CheckAlive;
using NSubstitute;

namespace Tests.SharedBeautifulServices.CheckAliveService
{
	public class CheckAliveServiceTests
	{
		private readonly ICommunicationService _communicationServiceMock = Substitute.For<ICommunicationService>();
		private readonly IIdentificationKey _identificationKeyMock = Substitute.For<IIdentificationKey>();


		[Fact]
		public void
			WhenStartingCheckAliveServiceWithWrongModeSetting_ThenCheckAliveService_ShouldThrowCheckAliveException()
		{
			var settings = new CheckAliveSettings
			{
				Enabled = true,
				Mode = -1,
				FrequencyInSeconds = 1000
			};

			var checkAliveService = CreateCheckAliveService(settings);
			Assert.Throws<CheckAliveException>(() => checkAliveService.Start());
		}

		private global::BeautifulFundamental.Core.Services.CheckAlive.CheckAliveService CreateCheckAliveService(
			CheckAliveSettings settings)
		{
			var checkAliveService =
				new global::BeautifulFundamental.Core.Services.CheckAlive.CheckAliveService(settings,
					_communicationServiceMock,
					_identificationKeyMock);
			return checkAliveService;
		}

		[Fact]
		public void
			WhenStartingCheckAliveServiceWithFrequencyLessThan20_ThenCheckAliveService_ShouldThrowCheckAliveException()
		{
			var settings = new CheckAliveSettings
			{
				Enabled = true,
				Mode = 0,
				FrequencyInSeconds = 10
			};

			var checkAliveService = CreateCheckAliveService(settings);
			Assert.Throws<CheckAliveException>(() => checkAliveService.Start());
		}

		[Fact]
		public async void
			WhenStartingCheckAliveServiceWithMode0_ThenCheckAliveService_ShouldReceiveCheckAliveMessageAndSendReplyMessage()
		{
			var settings = new CheckAliveSettings
			{
				Enabled = true,
				Mode = 0,
				FrequencyInSeconds = 1000
			};

			var checkAliveService = CreateCheckAliveService(settings);

			_communicationServiceMock.ReceiveAsync<CheckAliveRequest>()
				.Returns(Task.FromResult(new CheckAliveRequest()));

			// For the moq framework, interesting!
			//_communicationServiceMock.Setup(m => m.ReceiveAsync<CheckAliveMessage>(It.IsAny<CancellationToken>()))
			//	.Returns(Task.FromResult(new CheckAliveMessage()));

			checkAliveService.Start();
			await Task.Delay(1);
			checkAliveService.Stop();

			await _communicationServiceMock.Received().ReceiveAndSendAsync<CheckAliveRequest>(Arg.Any<object>());

			// For the moq framework, interesting!
			//_communicationServiceMock.Verify(
			//	mock => mock.ReceiveAsync<CheckAliveMessage>(It.IsAny<CancellationToken>()));
			//_communicationServiceMock.Verify(mock => mock.SendAsync(It.IsAny<CheckAliveReplyMessage>()));
		}

		[Fact]
		public async void
			WhenStartingCheckAliveServiceWithMode1_ThenCheckAliveService_ShouldSendCheckAliveMessageAndReceiveReplyMessage()
		{
			var settings = new CheckAliveSettings
			{
				Enabled = true,
				Mode = 1,
				FrequencyInSeconds = 20
			};

			var checkAliveService = CreateCheckAliveService(settings);


			_communicationServiceMock.ReceiveAsync<CheckAliveReply>()
				.Returns(Task.FromResult(new CheckAliveReply { Success = true }));

			// For the moq framework, interesting!
			//_communicationServiceMock.Setup(m => m.SendAsync(It.IsAny<CheckAliveMessage>()));
			//_communicationServiceMock.Setup(m => m.ReceiveAsync<CheckAliveReplyMessage>(It.IsAny<CancellationToken>()))
			//	.Returns(Task.FromResult(new CheckAliveReplyMessage()));

			checkAliveService.Start();
			await Task.Delay(1);
			checkAliveService.Stop();

			await _communicationServiceMock.Received()
				.SendAndReceiveAsync<CheckAliveReply>(Arg.Any<object>());
		}
	}
}