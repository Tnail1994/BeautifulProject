using BeautifulFundamental.Server.Session.Implementations;

namespace BeautifulFundamental.Server.Session.Contracts.Context.Db
{
	public interface ISessionsDbContext;

	public interface ISessionDataProvider
	{
		bool TryGetSessionState(string sessionId, out SessionState sessionState);
	}
}