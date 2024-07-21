namespace Session.Common.Contracts
{
	public interface ISessionsService
	{
		void TryAdd(string sessionId, ISession session);
		bool TryRemove(string sessionId);
#if DEBUG
		IEnumerable<ISession> GetSessions();
#endif
		void SaveSessionInfo(ISessionInfo sessionInfo);
		bool TryMergeSession(string username, out ISessionInfo sessionInfo);
	}
}