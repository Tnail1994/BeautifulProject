using NSubstitute;
using Remote.Communication.Common.Contracts;
using Session.Common.Contracts;
using Session.Common.Implementations;
using Session.Services.Authorization;

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
			var sessionKeyMock = Substitute.For<ISessionKey>();
			_authenticationServiceMock = Substitute.For<IAuthenticationService>();
			_sessionsServiceMock = Substitute.For<ISessionsService>();

			_session = new global::Session.Core.Session(sessionKeyMock, _connectionServiceMock,
				_authenticationServiceMock, _communicationServiceMock, _sessionsServiceMock);
		}

		[Fact]
		public void WhenStartingSession_ThenConnectionServiceShouldStartAsWellAndSubscribeToEvents()
		{
			_authenticationServiceMock.Authorize(_communicationServiceMock)
				.Returns(Task.FromResult(AuthorizationInfo.Create("any")));

			_session.Start();
			_connectionServiceMock.Received(1).Start();
			_connectionServiceMock.Received(1).ConnectionLost += Arg.Any<Action<string>>();
		}
	}
}