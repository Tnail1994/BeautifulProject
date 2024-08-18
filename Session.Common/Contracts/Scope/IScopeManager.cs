using Session.Common.Implementations;

namespace Session.Common.Contracts.Scope
{
	public interface IScopeManager
	{
		IScope Create();
		void Destroy(ISessionKey sessionKey);
	}
}