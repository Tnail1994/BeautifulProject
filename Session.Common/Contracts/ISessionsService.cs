namespace Session.Common.Contracts
{
	public interface ISessionsService
	{
		void TryAdd(string sessionId, ISession session);
		bool TryRemove(string sessionId);
		IEnumerable<ISession> GetSessions();
		void SaveSessionInfo(ISessionInfo sessionInfo);
	}
}