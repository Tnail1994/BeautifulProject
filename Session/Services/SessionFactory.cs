using Remote.Communication.Common.Contracts;
using Session.Common.Contracts;
using Session.Common.Contracts.Services;

namespace Session.Services
{
	public class SessionFactory : ISessionFactory
	{
		public ISession Create(ICommunicationService communicationService)
		{
			return Session.Create(communicationService);
		}
	}
}