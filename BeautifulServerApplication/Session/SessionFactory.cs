using Remote.Core.Communication;

namespace BeautifulServerApplication.Session
{
	public interface ISessionFactory
	{
		ISession Create(ICommunicationService communicationService);
	}

	internal class SessionFactory : ISessionFactory
	{
		public ISession Create(ICommunicationService communicationService)
		{
			return Session.Create(communicationService);
		}
	}
}