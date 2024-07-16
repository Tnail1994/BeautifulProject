using NSubstitute;
using Remote.Communication.Common.Contracts;
using Session.Common.Implementations;
using Session.Services;
using SharedBeautifulServices.Common;

namespace Tests.Session
{
	public class SessionFactoryTests
	{
		private readonly SessionFactory _sessionFactory = new();

		[Fact]
		public void WhenCreatingSession_ThenShouldCreateSessionWithCommunicationService()
		{
			var dummyCommunicationService = Substitute.For<ICommunicationService>();
			var sessionKeyMock = Substitute.For<ISessionKey>();
			var checkAliveServiceMock = Substitute.For<ICheckAliveService>();
			var session = _sessionFactory.Create(dummyCommunicationService, sessionKeyMock, checkAliveServiceMock);
			Assert.NotNull(session);
		}
	}
}