using NSubstitute;
using Remote.Communication.Common.Contracts;
using Session.Common.Implementations;
using Session.Services;

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
			var session = _sessionFactory.Create(dummyCommunicationService, sessionKeyMock);
			Assert.NotNull(session);
		}
	}
}