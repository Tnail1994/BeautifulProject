using BeautifulServerApplication.Session;
using NSubstitute;
using Remote.Core.Communication;

namespace Tests.BeautifulServerApplication.Session
{
	public class SessionTests
	{
		private readonly ISession _session;
		private readonly ICommunicationService _communicationServiceMock;

		public SessionTests()
		{
			_communicationServiceMock = Substitute.For<ICommunicationService>();
			_session = global::BeautifulServerApplication.Session.Session.Create(_communicationServiceMock);
		}

		[Fact]
		public void WhenStartingSession_ThenCommunicationServiceShouldStartAsWell()
		{
			_session.Start();
			_communicationServiceMock.Received(1).Start();
		}
	}
}