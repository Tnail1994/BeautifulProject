using NSubstitute;
using Remote.Communication.Common.Contracts;
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
			var session = _sessionFactory.Create(dummyCommunicationService);
			Assert.NotNull(session);
		}
	}
}