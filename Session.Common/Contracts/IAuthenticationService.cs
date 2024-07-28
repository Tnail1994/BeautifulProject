using Remote.Communication.Common.Contracts;

namespace Session.Common.Contracts
{
	public interface IAuthenticationService
	{
		Task<IAuthorizationInfo> Authorize(ICommunicationService communicationService);
		Task UnAuthorize(ICommunicationService communicationServicestring, string username);
	}
}