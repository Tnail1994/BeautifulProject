namespace Session.Common.Contracts
{
	public interface ISessionsService
	{
		void TryAdd(ISession session, ISessionInfo sessionInfo);
		bool TryRemove(string sessionId);
#if DEBUG
		IEnumerable<ISession> GetSessions();
#endif
		void SaveSessionInfo(ISessionInfo sessionInfo);
		bool TryGetSessionInfo(string username, out ISessionInfo sessionInfo);
		void UpdateSession(ISession session, ISessionInfo sessionInfo);
	}
}