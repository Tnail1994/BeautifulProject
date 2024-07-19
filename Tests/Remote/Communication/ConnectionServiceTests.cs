using NSubstitute;
using Remote.Communication;
using Remote.Communication.Client;
using Remote.Communication.Common.Client.Contracts;
using Remote.Communication.Common.Contracts;
using Session.Common.Implementations;
using SharedBeautifulServices.Common;

namespace Tests.Remote.Communication
{
	public class ConnectionServiceTests
	{
		private readonly ConnectionSettings _settingsMock = ConnectionSettings.Default;
		private readonly IAsyncClient _clientMock = Substitute.For<IAsyncClient>();
		private readonly ICommunicationService _communicationServiceMock = Substitute.For<ICommunicationService>();
		private readonly ICheckAliveService _checkAliveServiceMock = Substitute.For<ICheckAliveService>();
		private readonly ISessionKey _sessionKeyMock = Substitute.For<ISessionKey>();

		private IConnectionService? _connectionService;

		[Fact]
		public void
			WhenStartGetCalled_AndConnectionSuccessful_ThenStartFrom_CheckAlive_AndCommunicationService_ShouldBeCalledAsWell()
		{
			_connectionService = new ConnectionService(_clientMock, _communicationServiceMock, _checkAliveServiceMock,
				_sessionKeyMock, _settingsMock);

			_clientMock.ConnectAsync().Returns(Task.FromResult(true));

			_connectionService.Start();

			_communicationServiceMock.Received(1).Start();
			_checkAliveServiceMock.Received(1).Start();
		}

		[Fact]
		public void
			WhenStartGetCalled_AndConnectionFailed_ThenStartFrom_CheckAlive_AndCommunicationService_ShouldNotBeCalled()
		{
			_connectionService = new ConnectionService(_clientMock, _communicationServiceMock, _checkAliveServiceMock,
				_sessionKeyMock, _settingsMock);

			_clientMock.ConnectAsync().Returns(Task.FromResult(false));

			_connectionService.Start();

			_communicationServiceMock.DidNotReceive().Start();
			_checkAliveServiceMock.DidNotReceive().Start();
		}

		[Fact]
		public async void
			WhenConnectionLostEventOccurs_OfCheckAliveService_ThenConnectionServiceTriesToReconnect_WhenEnabled_ShouldReceiveConnectAsyncCallAsOftenAsReconnectAttemptsConfigurePlus1()
		{
			var connectionSettings = new ConnectionSettings
			{
				ReconnectActivated = true,
				ReconnectAttempts = 1,
				ReconnectDelayInSeconds = 0
			};

			_connectionService = new ConnectionService(_clientMock, _communicationServiceMock, _checkAliveServiceMock,
				_sessionKeyMock, connectionSettings);

			_clientMock.ConnectAsync().Returns(Task.FromResult(false));

			_connectionService.Start();

			_checkAliveServiceMock.ConnectionLost += Raise.Event<Action>();
			await _clientMock.Received(connectionSettings.ReconnectAttempts + 1).ConnectAsync();
		}

		[Fact]
		public async void
			WhenConnectionLostEventOccurs_OfCommunicationService_ThenConnectionServiceTriesToReconnect_WhenEnabled_ShouldReceiveConnectAsyncCallAsOftenAsReconnectAttemptsConfigurePlus1()
		{
			var connectionSettings = new ConnectionSettings
			{
				ReconnectActivated = true,
				ReconnectAttempts = 1,
				ReconnectDelayInSeconds = 0
			};

			_connectionService = new ConnectionService(_clientMock, _communicationServiceMock, _checkAliveServiceMock,
				_sessionKeyMock, connectionSettings);

			_clientMock.ConnectAsync().Returns(Task.FromResult(false));

			_connectionService.Start();

			_communicationServiceMock.ConnectionLost += Raise.Event<EventHandler<string>>(this, string.Empty);
			await _clientMock.Received(connectionSettings.ReconnectAttempts + 1).ConnectAsync();
		}
	}
}