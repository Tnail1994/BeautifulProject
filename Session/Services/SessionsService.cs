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

			public static SessionBundle Create(ISessionInfo sessionInfo, SessionInfoDto sessionInfoDto)
			{
				return new SessionBundle
				{
					SessionInfo = sessionInfo,
					SessionInfoDto = sessionInfoDto
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

			ReadSessionsInfo();
		}

		private void ReadSessionsInfo()
		{
			var sessionInfosDto = _dbManager.GetEntities<SessionInfoDto>();

			if (sessionInfosDto == null)
			{
				this.LogError("Cannot read session infos from database.", "server");
				return;
			}

			foreach (var sessionInfoDto in sessionInfosDto)
			{
				_sessionBundles.TryAdd(sessionInfoDto.Id, SessionBundle.Create(Map(sessionInfoDto), sessionInfoDto));
			}
		}

		public void TryAdd(ISession session)
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

#if DEBUG
		public IEnumerable<ISession> GetSessions()
		{
			return _sessionBundles.Values.Select(bundle => bundle.Session).OfType<ISession>();
		}
#endif

		private bool FilterBundleByUsername(SessionBundle bundle, string username)
		{
			// When empty, then we do not filter by username
			if (string.IsNullOrEmpty(username))
				return true;

			return bundle.SessionInfo?.Username == username;
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
				UpdateSessionInfo(bundle, sessionInfo);
				UpdateSessionInfoDto(bundle, sessionInfo);
			}

			_dbManager.SaveChanges(dto);
		}

		public bool HasPendingSession(string username)
		{
			return _sessionBundles.Values.Any(bundle =>
				bundle.SessionInfo?.Username == username && bundle.SessionInfo?.SessionState == SessionState.Stopped);
		}

		public bool TryGetSessionInfo(string username, out ISessionInfo sessionInfo)
		{
			if (HasPendingSession(username))
			{
				var foundSessionInfos = _sessionBundles.Values.Where(bundle =>
					FilterBundleByUsername(bundle, username) &&
					FilterBundleByState(bundle, SessionState.Stopped)).ToList();

				if (foundSessionInfos.Count > 1)
				{
					var logMessage =
						$"Found more than one session with username {username} and state {SessionState.Stopped}.";
					this.LogError(logMessage, "server");
					throw new InvalidOperationException(logMessage);
				}

				var foundSessionInfo = foundSessionInfos.FirstOrDefault();

				sessionInfo = foundSessionInfo?.SessionInfo ?? SessionInfo.Empty;
				return foundSessionInfo != null;
			}

			sessionInfo = SessionInfo.Empty;
			return false;
		}

		private static bool FilterBundleByState(SessionBundle bundle, SessionState state)
		{
			return bundle.SessionInfo?.SessionState == state;
		}

		private static void UpdateSessionInfo(SessionBundle bundle, ISessionInfo sessionInfo)
		{
			bundle.SessionInfo = sessionInfo;
		}

		private static void UpdateSessionInfoDto(SessionBundle bundle, ISessionInfo sessionInfo)
		{
			if (bundle.SessionInfoDto == null)
				bundle.SessionInfoDto = Map(sessionInfo);
			else
			{
				bundle.SessionInfoDto.Username = sessionInfo.Username;
				bundle.SessionInfoDto.SessionState = (int)sessionInfo.SessionState;
				bundle.SessionInfoDto.Authorized = sessionInfo.Authorized;
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