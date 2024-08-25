using BeautifulFundamental.Server.Session.Contracts.Core;

namespace BeautifulFundamental.Server.Session.Contracts.Services
{
	public interface ISessionsService
	{
		void TryAdd(ISession session, ISessionInfo sessionInfo);
		bool TryRemove(string sessionId);
#if DEBUG
		IEnumerable<ISession> GetSessions();
#endif
		void SaveSessionInfo(ISessionInfo sessionInfo);
		bool TryGetPendingSessionInfo(string username, out ISessionInfo sessionInfo);
		void UpdateSession(ISession session, ISessionInfo sessionInfo);
	}
}