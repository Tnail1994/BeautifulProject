using Remote.Communication.Common.Contracts;

namespace Session.Common.Contracts.Services
{
	public interface ISessionFactory
	{
		ISession Create(ICommunicationService communicationService);
	}
}