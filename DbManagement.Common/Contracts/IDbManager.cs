using Session.Common.Implementations;
using SharedBeautifulData;

namespace DbManagement.Common.Contracts
{
	public interface IDbManager
	{
		IEnumerable<T>? GetEntities<T>(ISessionKey? sessionKey = null) where T : Entity, new();
	}
}