using NSubstitute;
using Remote.Communication.Common.Contracts;
using Session.Common.Contracts;
using Session.Common.Implementations;

namespace Tests.Session
{
	public class SessionTests
	{
		private readonly ISession _session;
		private readonly IConnectionService _connectionServiceMock;

		public SessionTests()
		{
			var communicationServiceMock = Substitute.For<ICommunicationService>();
			_connectionServiceMock = Substitute.For<IConnectionService>();
			var sessionKeyMock = Substitute.For<ISessionKey>();
			_session = new global::Session.Core.Session(sessionKeyMock, _connectionServiceMock
#if DEBUG
				, communicationServiceMock
#endif
			);
		}

		[Fact]
		public void WhenStartingSession_ThenConnectionServiceShouldStartAsWellAndSubscribeToEvents()
		{
			_session.Start();
			_connectionServiceMock.Received(1).Start();
			_connectionServiceMock.Received(1).ConnectionLost += Arg.Any<Action<string>>();
		}
	}
}