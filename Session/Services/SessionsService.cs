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
			public static SessionBundle Create(ISessionInfo sessionInfo)
			{
				return new SessionBundle
				{
					SessionInfo = sessionInfo,
				};
			}

			public static SessionBundle Create(ISession session, ISessionInfo sessionInfo)
			{
				return new SessionBundle
				{
					Session = session,
					SessionInfo = sessionInfo,
				};
			}

			public ISession? Session { get; set; }
			public ISessionInfo? SessionInfo { get; set; }
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
			var sessionInfosDto = GetSessionInfosDto();

			if (sessionInfosDto == null)
			{
				this.LogError("Cannot read session infos from database.");
				return;
			}

			foreach (var sessionInfoDto in sessionInfosDto)
			{
				TryAdd(sessionInfoDto.Id, SessionBundle.Create(Map(sessionInfoDto)));
			}
		}

		private IEnumerable<SessionInfoDto>? GetSessionInfosDto()
		{
			return _dbManager.GetEntities<SessionInfoDto>();
		}

		public void TryAdd(ISession session, ISessionInfo sessionInfo)
		{
			var sessionBundle = SessionBundle.Create(session, sessionInfo);
			TryAdd(session.Id, sessionBundle);
		}

		private void TryAdd(string sessionId, SessionBundle sessionBundle)
		{
			_sessionBundles.TryAdd(sessionId, sessionBundle);
		}

		public bool TryRemove(string sessionId)
		{
			return TryRemoveSessionBundle(sessionId);
		}

		private bool TryRemoveSessionBundle(string sessionId)
		{
			var tryRemoveResult = _sessionBundles.TryRemove(sessionId, out _);

			if (!tryRemoveResult)
			{
				this.LogError($"Cannot remove session with Id {sessionId} from dictionary.");
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

		public void SaveSessionInfo(ISessionInfo sessionInfo)
		{
			var sessionInfoDto = GetSessionInfosDto()?.FirstOrDefault(dto => dto.Id == sessionInfo.Id) ??
			                     Map(sessionInfo);

			UpdateSessionInfo(sessionInfo);
			UpdateSessionInfoDto(sessionInfoDto, sessionInfo);
			_dbManager.SaveChanges(sessionInfoDto);
		}

		private void UpdateSessionInfo(ISessionInfo sessionInfo)
		{
			if (_sessionBundles.TryGetValue(sessionInfo.Id, out var bundle))
			{
				var newBundle = SessionBundle.Create(sessionInfo);
				newBundle.Session = bundle.Session;
				_sessionBundles.TryUpdate(sessionInfo.Id, newBundle, bundle);
			}
			else
			{
				this.LogError($"Cannot update sessionInfo for id: {sessionInfo.Id}");
			}
		}

		private static void UpdateSessionInfoDto(SessionInfoDto sessionInfoDto, ISessionInfo sessionInfo)
		{
			sessionInfoDto.SessionState = (int)sessionInfo.SessionState;
			sessionInfoDto.Authorized = sessionInfo.Authorized;
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
				var foundSessionBundles = _sessionBundles.Values.Where(bundle =>
					FilterBundleByUsername(bundle, username) &&
					FilterBundleByState(bundle, SessionState.Stopped)).ToList();

				if (foundSessionBundles.Count > 1)
				{
					var logMessage =
						$"Found more than one session with username {username} and state {SessionState.Stopped}.";
					this.LogError(logMessage);
					throw new InvalidOperationException(logMessage);
				}

				var foundSessionBundle = foundSessionBundles.FirstOrDefault();

				sessionInfo = foundSessionBundle?.SessionInfo ?? SessionInfo.Empty;
				return !string.IsNullOrEmpty(sessionInfo.Id) && !string.IsNullOrEmpty(sessionInfo.Username) &&
				       foundSessionBundle != null;
			}

			sessionInfo = SessionInfo.Empty;
			return false;
		}

		public void UpdateSession(ISession session, ISessionInfo sessionInfo)
		{
			if (_sessionBundles.TryGetValue(sessionInfo.Id, out var bundle))
			{
				var newBundle = SessionBundle.Create(session, sessionInfo);
				_sessionBundles.TryUpdate(sessionInfo.Id, newBundle, bundle);
			}
			else
			{
				this.LogError($"Cannot update session for id: {sessionInfo.Id}");
			}
		}

		private static bool FilterBundleByState(SessionBundle bundle, SessionState state)
		{
			return bundle.SessionInfo?.SessionState == state;
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