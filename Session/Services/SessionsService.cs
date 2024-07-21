using DbManagement.Common.Contracts;
using DbManagement.Contexts;
using Session.Common.Contracts;
using System.Collections.Concurrent;
using Core.Extensions;
using Session.Common.Implementations;
using Session.Core;

namespace Session.Services
{
	public class SessionsService : ISessionsService
	{
		private class SessionBundle
		{
			public static SessionBundle Create(ISession session)
			{
				return new SessionBundle
				{
					Session = session
				};
			}

			public ISession? Session { get; set; }
			public ISessionInfo? SessionInfo { get; set; }
			public SessionInfoDto? SessionInfoDto { get; set; }
		}

		private readonly IDbManager _dbManager;

		private readonly ConcurrentDictionary<string, SessionBundle> _sessionBundles = new();

		public SessionsService(IDbManager dbManager)
		{
			_dbManager = dbManager;
		}

		public void TryAdd(string sessionId, ISession session)
		{
			_sessionBundles.TryAdd(session.Id, SessionBundle.Create(session));
		}

		public bool TryRemove(string sessionId)
		{
			var tryRemoveResult = _sessionBundles.TryRemove(sessionId, out _);

			if (!tryRemoveResult)
			{
				this.LogError($"Cannot remove session with Id {sessionId} from dictionary.", "server");
			}

			return tryRemoveResult;
		}

		public IEnumerable<ISession> GetSessions()
		{
			return _sessionBundles.Values.Select(bundle => bundle.Session).OfType<ISession>();
		}

		private IEnumerable<ISessionInfo>? GetSessionInfosDto()
		{
			return _dbManager.GetEntities<SessionInfoDto>()?.Select(Map);
		}

		public void SaveSessionInfo(ISessionInfo sessionInfo)
		{
			var dto = Map(sessionInfo);

			if (_sessionBundles.TryGetValue(sessionInfo.Id, out var bundle))
			{
				UpdateSessionInfo(sessionInfo, bundle);
				UpdateSessionInfoDto(bundle, dto);
			}

			_dbManager.SaveChanges(dto);
		}

		private static void UpdateSessionInfo(ISessionInfo sessionInfo, SessionBundle bundle)
		{
			bundle.SessionInfo = sessionInfo;
		}

		private static void UpdateSessionInfoDto(SessionBundle bundle, SessionInfoDto dto)
		{
			if (bundle.SessionInfoDto == null)
				bundle.SessionInfoDto = dto;
			else
			{
				bundle.SessionInfoDto.Username = dto.Username;
				bundle.SessionInfoDto.SessionState = dto.SessionState;
				bundle.SessionInfoDto.Authorized = dto.Authorized;
			}
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