using Remote.Communication.Common.Contracts;

namespace Session.Common.Contracts
{
	public interface IAuthenticationService
	{
		Task<bool> Authorize(ICommunicationService communicationService);
	}
}