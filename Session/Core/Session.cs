using Core.Extensions;
using Remote.Communication.Common.Contracts;
using Session.Common.Contracts;
using Session.Common.Implementations;
using SharedBeautifulData.Exceptions;

namespace Session.Core
{
	public class Session : ISession, IDisposable
	{
		private readonly ISessionKey _sessionKey;
		private readonly IConnectionService _connectionService;
		private readonly IAuthenticationService _authenticationService;

		private readonly ICommunicationService _communicationService;
		private readonly ISessionsService _sessionsService;

		private SessionInfo _sessionInfo;

		public Session(ISessionKey sessionKey, IConnectionService connectionService,
			IAuthenticationService authenticationService, ICommunicationService communicationService,
			ISessionsService sessionsService)
		{
			_sessionKey = sessionKey;
			_connectionService = connectionService;
			_authenticationService = authenticationService;
			_communicationService = communicationService;
			_sessionsService = sessionsService;

			_sessionInfo = SessionInfo.Create(_sessionKey.SessionId, string.Empty);
		}

		public string Id => _sessionKey.SessionId;
		public event EventHandler<SessionStoppedEventArgs>? SessionStopped;

		public async void Start()
		{
			_sessionsService.TryAdd(this, _sessionInfo);

			_connectionService.ConnectionLost += OnConnectionLost;

			try
			{
				_connectionService.Start();

				SetState(SessionState.Authorizing, false);

				var authorizationInfo = await _authenticationService.Authorize(_communicationService);

				SetInfo(authorizationInfo, false);

				if (!authorizationInfo.IsAuthorized)
				{
					SetState(SessionState.FailedAuthorization);
					return;
				}

				if (_sessionsService.TryGetSessionInfo(authorizationInfo.Username, out ISessionInfo sessionInfo))
				{
					ReestablishSession(sessionInfo);
				}
				else
				{
					RunSession();
				}

				SetState(SessionState.Running);
			}
			catch (CheckAliveException checkAliveException)
			{
				this.LogError($"CheckAliveService failed to start. {checkAliveException.Message}", Id);
			}
			catch (OperationCanceledException)
			{
				this.LogInfo("Session was stopped", Id);
			}
			catch (Exception e)
			{
				this.LogFatal($"!!! Unexpected error while Start inside Session event\n" +
				              $"Message: {e.Message}\n" +
				              $"Stacktrace: {e.StackTrace}\n", Id);
			}
		}

		private void ReestablishSession(ISessionInfo sessionInfo)
		{
			this.LogDebug($"Reestablishing session {Id}", Id);

			SetState(SessionState.Down);
			_sessionsService.UpdateSession(this, sessionInfo);
			_sessionsService.TryRemove(_sessionInfo.Id);
			_sessionInfo = (SessionInfo)sessionInfo;
		}

		private void RunSession()
		{
			this.LogDebug($"Running session {Id}", Id);

			// From here the session can be used to communicate with the client.
			// All what happens here, should happen parallel to the main thread.
			// So beware of writing to the console or doing other blocking operations.
			// Need to define an own logging system for this session overall.
		}

		private void SetInfo(IAuthorizationInfo authorizationInfo, bool save = true)
		{
			_sessionInfo.SetUsername(authorizationInfo.Username);
			_sessionInfo.SetAuthorized(authorizationInfo.IsAuthorized);

			if (save)
				SaveSession();
		}

		private void SetState(SessionState state, bool save = true)
		{
			_sessionInfo.SetState(state);

			if (save)
				SaveSession();
		}

		private void Stop()
		{
			this.LogDebug($"Stopping session {Id}", Id);
			_connectionService.ConnectionLost -= OnConnectionLost;

			SetState(SessionState.Stopped);

			_authenticationService.UnAuthorize(_communicationService, _sessionInfo.Username);

			TryRemoveSession();
		}

		private void TryRemoveSession()
		{
			if (_sessionInfo.SessionState == SessionState.Stopped)
				return;

			_sessionsService.TryRemove(_sessionKey.InstantiatedSessionId);
		}

		private void SaveSession()
		{
			_sessionsService.SaveSessionInfo(_sessionInfo);
		}

		private void OnConnectionLost(string reason)
		{
			InvokeSessionOnHold(reason);
		}

		private void InvokeSessionOnHold(string reason)
		{
			SessionStopped?.Invoke(this, SessionStoppedEventArgs.Create(_sessionKey, $"Connection lost: {reason}"));
		}

		public void Dispose()
		{
			Stop();
		}


#if DEBUG
		public void SendMessageToClient(object message)
		{
			_communicationService.SendAsync(message);
		}
#endif
	}
}