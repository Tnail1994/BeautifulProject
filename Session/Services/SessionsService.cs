using DbManagement.Common.Contracts;
using DbManagement.Contexts;
using Session.Common.Contracts;
using System.Collections.Concurrent;
using Session.Common.Implementations;
using Session.Core;

namespace Session.Services
{
	public class SessionsService : ISessionsService
	{
		private readonly IDbManager _dbManager;

		private readonly ConcurrentDictionary<string, ISession> _sessions = new();

		public SessionsService(IDbManager dbManager)
		{
			_dbManager = dbManager;
		}

		public void TryAdd(string sessionId, ISession session)
		{
			_sessions.TryAdd(sessionId, session);
		}

		public bool TryRemove(string sessionId)
		{
			return _sessions.TryRemove(sessionId, out _);
		}

		public IEnumerable<ISession> GetSessions()
		{
			return _sessions.Values;
		}

		private IEnumerable<ISessionInfo>? GetSessionInfosDto()
		{
			return _dbManager.GetEntities<SessionInfoDto>()?.Select(Map);
		}

		public void SaveSessionInfo(ISessionInfo sessionInfo)
		{
			var dto = Map(sessionInfo);
			_dbManager.AddEntity(dto);
		}

		private ISessionInfo Map(SessionInfoDto dto)
		{
			return SessionInfo.Create(dto.Id, dto.Username, (SessionState)dto.SessionState);
		}

		private static SessionInfoDto Map(ISessionInfo sessionInfo)
		{
			return new SessionInfoDto(sessionInfo.Id, sessionInfo.Username, (int)sessionInfo.SessionState,
				sessionInfo.Authorized);
		}
	}
}