using BeautifulFundamental.Core.Communication;
using BeautifulFundamental.Server.Session.Context;
using BeautifulFundamental.Server.Session.Core;
using BeautifulFundamental.Server.Session.Services.Authorization;
using BeautifulFundamental.Server.Session.Services.Session;
using BeautifulFundamental.Server.Session.Services.UserRegistration;
using NSubstitute;

namespace Tests.Session.Core
{
	public class SessionTests
	{
		private readonly ISession _session;
		private readonly IConnectionService _connectionServiceMock;
		private readonly IAuthenticationService _authenticationServiceMock;
		private readonly ICommunicationService _communicationServiceMock;
		private readonly ISessionsService _sessionsServiceMock;

		public SessionTests()
		{
			_communicationServiceMock = Substitute.For<ICommunicationService>();
			_connectionServiceMock = Substitute.For<IConnectionService>();
			var sessionContextMock = Substitute.For<ISessionContext>();
			_authenticationServiceMock = Substitute.For<IAuthenticationService>();
			_sessionsServiceMock = Substitute.For<ISessionsService>();
			var sessionContextManagerMock = Substitute.For<ISessionContextManager>();

			var sessionLoop = Substitute.For<Lazy<ISessionLoop>>();
			var userRegistrationServiceMock = Substitute.For<IUserRegistrationService>();
			_session = new global::BeautifulFundamental.Server.Session.Core.Session(sessionLoop, sessionContextMock,
				sessionContextManagerMock,
				_connectionServiceMock,
				_authenticationServiceMock, _communicationServiceMock, _sessionsServiceMock,
				userRegistrationServiceMock);
		}

		[Fact]
		public void
			WhenStartingSessionAndConnectionEstablished_ThenConnectionServiceShouldStartAsWellAndSubscribeToEventsAndReceiveAuthorizeCall()
		{
			_authenticationServiceMock.Authorize(_communicationServiceMock)
				.Returns(Task.FromResult(AuthorizationInfo.Create("any")));

			_session.Start();
			_connectionServiceMock.ConnectionEstablished += Raise.Event<Action>();
			_connectionServiceMock.Received(1).Start();
			_connectionServiceMock.Received(1).ConnectionLost += Arg.Any<Action<string>>();
			_authenticationServiceMock.Received(1).Authorize(_communicationServiceMock);
		}

		[Fact]
		public void WhenStartingSession_ThenSessionAddsItselfToSessionsService_TryAddShouldCalled()
		{
			_session.Start();
			_sessionsServiceMock.Received(1).TryAdd(_session, Arg.Any<ISessionInfo>());
		}

		[Fact]
		public void WhenConnectionEstablished_ThenItTryToReestablishSessionByTryGettingSessionInfo()
		{
			_authenticationServiceMock.Authorize(_communicationServiceMock)
				.Returns(Task.FromResult(AuthorizationInfo.Create("mockName")));
			_session.Start();

			_connectionServiceMock.ConnectionEstablished += Raise.Event<Action>();
			_sessionsServiceMock.Received(1).TryGetPendingSessionInfo("mockName", out Arg.Any<ISessionInfo>());
		}

		[Fact]
		public void WhenStartingAndDisposing_ThenShouldCallTryRemoveAndSaveSessionInfoWhenUserIsAuthorized()
		{
			_authenticationServiceMock.Authorize(_communicationServiceMock)
				.Returns(Task.FromResult(AuthorizationInfo.Create("anyUser")));
			_session.Start();
			_connectionServiceMock.ConnectionEstablished += Raise.Event<Action>();
			((global::BeautifulFundamental.Server.Session.Core.Session)_session).Dispose();
			_sessionsServiceMock.Received().SaveSessionInfo(Arg.Any<SessionInfo>());
		}
	}
}