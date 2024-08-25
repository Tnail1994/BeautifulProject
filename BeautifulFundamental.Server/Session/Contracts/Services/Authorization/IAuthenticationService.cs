using BeautifulFundamental.Core.Communication;

namespace BeautifulFundamental.Server.Session.Contracts.Services.Authorization
{
	public interface IAuthenticationService
	{
		Task<IAuthorizationInfo> Authorize(ICommunicationService communicationService);
		Task UnAuthorize(ICommunicationService communicationServicestring, string anyIdentifier);
	}
}