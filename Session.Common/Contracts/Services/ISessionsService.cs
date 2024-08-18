﻿using Session.Common.Contracts.Core;

namespace Session.Common.Contracts.Services
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