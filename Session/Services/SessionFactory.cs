using Remote.Communication.Common.Contracts;
using Session.Common.Contracts;
using Session.Common.Contracts.Services;
using Session.Common.Implementations;

namespace Session.Services
{
	public class SessionFactory : ISessionFactory
	{
		public ISession Create(ICommunicationService communicationService, ISessionKey sessionKey)
		{
			return Session.Create(communicationService, sessionKey);
		}
	}
}