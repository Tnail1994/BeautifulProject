using NSubstitute;
using Remote.Communication.Common.Contracts;
using Session.Common.Contracts;
using Session.Common.Implementations;

namespace Tests.Session
{
	public class SessionTests
	{
		private readonly ISession _session;
		private readonly ICommunicationService _communicationServiceMock;

		public SessionTests()
		{
			_communicationServiceMock = Substitute.For<ICommunicationService>();
			var sessionKeyMock = Substitute.For<ISessionKey>();
			_session = global::Session.Session.Create(_communicationServiceMock, sessionKeyMock);
		}

		[Fact]
		public void WhenStartingSession_ThenCommunicationServiceShouldStartAsWell()
		{
			_session.Start();
			_communicationServiceMock.Received(1).Start();
		}
	}
}