using NSubstitute;
using Remote.Communication.Common.Contracts;
using Session.Common.Contracts;
using Session.Common.Implementations;
using SharedBeautifulServices.Common;

namespace Tests.Session
{
	public class SessionTests
	{
		private readonly ISession _session;
		private readonly ICommunicationService _communicationServiceMock;
		private readonly ICheckAliveService _checkAliveService;

		public SessionTests()
		{
			_communicationServiceMock = Substitute.For<ICommunicationService>();
			var sessionKeyMock = Substitute.For<ISessionKey>();
			_checkAliveService = Substitute.For<ICheckAliveService>();
			_session = global::Session.Session.Create(_communicationServiceMock, sessionKeyMock, _checkAliveService);
		}

		[Fact]
		public void WhenStartingSession_ThenCommunicationServiceShouldStartAsWell()
		{
			_session.Start();
			_communicationServiceMock.Received(1).Start();
			_checkAliveService.Received(1).Start();
		}
	}
}