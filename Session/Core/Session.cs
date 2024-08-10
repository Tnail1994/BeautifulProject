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

		public void Start()
		{
			_sessionsService.TryAdd(this, _sessionInfo);

			_connectionService.ConnectionLost += OnConnectionLost;
			_connectionService.ConnectionEstablished += OnConnectionEstablished;

			try
			{
				SetState(SessionState.Connecting);
				_connectionService.Start();
			}
			catch (CheckAliveException checkAliveException)
			{
				this.LogError($"CheckAliveService failed to start. {checkAliveException.Message}", Id);
			}
			catch (Exception e)
			{
				this.LogFatal($"Error while Start inside session\n" +
				              $"Message: {e.Message}\n" +
				              $"Stacktrace: {e.StackTrace}\n", Id);
			}
		}

		private async void OnConnectionEstablished()
		{
			try
			{
				SetState(SessionState.Authorizing);

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
			catch (OperationCanceledException)
			{
				this.LogInfo("Authorization canceled.");
			}
			catch (Exception e)
			{
				this.LogFatal($"[Session] Error while OnConnectionEstablished event \n" +
				              $"Message: {e.Message}\n" +
				              $"Stacktrace: {e.StackTrace}\n", Id);
			}
		}

		private void ReestablishSession(ISessionInfo sessionInfo)
		{
			this.LogDebug($"Reestablishing session {Id}", Id);

			// Update internal session info data
			_sessionsService.UpdateSession(this, sessionInfo);
			_sessionsService.TryRemove(_sessionInfo.Id);
			_sessionInfo = (SessionInfo)sessionInfo;
			_sessionKey.UpdateId(sessionInfo.Id);

			// Grab session context data
		}

		private void RunSession()
		{
			this.LogDebug($"Running session {Id}", Id);

			// From here the session can be used to communicate with the client.
			// All what happens here, should happen parallel to the main thread.
			// So beware of writing to collections or doing other blocking operations.
		}

		private void SetInfo(IAuthorizationInfo authorizationInfo, bool save = true)
		{
			_sessionInfo.SetUsername(authorizationInfo.Username);
			_sessionInfo.SetAuthorized(authorizationInfo.IsAuthorized);

			if (save)
				SaveSession();
		}

		private void SetState(SessionState state)
		{
			_sessionInfo.SetState(state);

			if (state.Equals(SessionState.Running) || (state.Equals(SessionState.Stopped) && _sessionInfo.Authorized))
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
			//if (_sessionInfo.SessionState == SessionState.Stopped)
			//	return;

			_sessionsService.TryRemove(_sessionKey.SessionId);
		}

		private void SaveSession()
		{
			_sessionsService.SaveSessionInfo(_sessionInfo);
		}

		private void OnConnectionLost(string reason)
		{
			this.LogDebug($"[Session] Connection lost, invoke SessionStopped event. Id or reason:{reason}");
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