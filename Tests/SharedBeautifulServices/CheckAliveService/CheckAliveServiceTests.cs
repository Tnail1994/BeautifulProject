using NSubstitute;
using Remote.Communication.Common.Contracts;
using Session.Common.Implementations;
using SharedBeautifulData;
using SharedBeautifulServices;

namespace Tests.SharedBeautifulServices.CheckAliveService
{
	public class CheckAliveServiceTests
	{
		private readonly ICommunicationService _communicationServiceMock = Substitute.For<ICommunicationService>();
		private readonly ISessionKey _sessionKeyMock = Substitute.For<ISessionKey>();


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

		private global::SharedBeautifulServices.CheckAliveService CreateCheckAliveService(CheckAliveSettings settings)
		{
			var checkAliveService =
				new global::SharedBeautifulServices.CheckAliveService(settings, _communicationServiceMock,
					_sessionKeyMock);
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

			_communicationServiceMock.ReceiveAsync<CheckAliveMessage>(Arg.Any<CancellationToken>())
				.Returns(Task.FromResult(new CheckAliveMessage()));

			// For the moq framework, interesting!
			//_communicationServiceMock.Setup(m => m.ReceiveAsync<CheckAliveMessage>(It.IsAny<CancellationToken>()))
			//	.Returns(Task.FromResult(new CheckAliveMessage()));

			checkAliveService.Start();
			await Task.Delay(1);
			checkAliveService.Stop();

			await _communicationServiceMock.Received().ReceiveAsync<CheckAliveMessage>(Arg.Any<CancellationToken>());
			_communicationServiceMock.Received().SendAsync(Arg.Any<CheckAliveReplyMessage>());

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


			_communicationServiceMock.ReceiveAsync<CheckAliveReplyMessage>(Arg.Any<CancellationToken>())
				.Returns(Task.FromResult(new CheckAliveReplyMessage()));

			// For the moq framework, interesting!
			//_communicationServiceMock.Setup(m => m.SendAsync(It.IsAny<CheckAliveMessage>()));
			//_communicationServiceMock.Setup(m => m.ReceiveAsync<CheckAliveReplyMessage>(It.IsAny<CancellationToken>()))
			//	.Returns(Task.FromResult(new CheckAliveReplyMessage()));

			checkAliveService.Start();
			await Task.Delay(1);
			checkAliveService.Stop();

			await _communicationServiceMock.Received()
				.ReceiveAsync<CheckAliveReplyMessage>(Arg.Any<CancellationToken>());
			_communicationServiceMock.Received().SendAsync(Arg.Any<CheckAliveMessage>());
		}
	}
}