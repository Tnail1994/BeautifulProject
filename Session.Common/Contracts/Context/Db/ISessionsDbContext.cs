using Session.Common.Implementations;

namespace Session.Common.Contracts.Context.Db
{
	public interface ISessionsDbContext
	{
	}

	public interface ISessionDataProvider
	{
		bool TryGetSessionState(string sessionId, out SessionState sessionState);
	}
}