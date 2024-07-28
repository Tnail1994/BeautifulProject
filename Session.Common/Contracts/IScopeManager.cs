using Session.Common.Implementations;

namespace Session.Common.Contracts
{
	public interface IScopeManager
	{
		IScope Create();
		void Destroy(ISessionKey sessionKey);
	}
}