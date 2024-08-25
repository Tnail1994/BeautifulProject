using BeautifulFundamental.Core.Identification;

namespace BeautifulFundamental.Server.Session.Contracts.Scope
{
	public interface IScopeManager
	{
		IScope Create();
		void Destroy(IIdentificationKey identificationKey);
	}
}