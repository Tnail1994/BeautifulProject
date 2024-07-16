using Remote.Communication.Common.Contracts;
using Session.Common.Implementations;
using SharedBeautifulServices.Common;

namespace Session.Common.Contracts.Services
{
	public interface ISessionFactory
	{
		ISession Create(ICommunicationService communicationService, ISessionKey sessionKey,
			ICheckAliveService checkAliveService);
	}
}