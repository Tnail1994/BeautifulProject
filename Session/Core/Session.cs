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

		private readonly SessionInfo _sessionInfo;

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
		public event EventHandler<string>? SessionStopped;

		public async void Start()
		{
			this.LogDebug($"Starting session {Id}", Id);
			_connectionService.ConnectionLost += OnConnectionLost;

			// From here the session can be used to communicate with the client.
			// All what happens here, should happen parallel to the main thread.
			// So beware of writing to the console or doing other blocking operations.
			// Need to define an own logging system for this session overall.

			try
			{
				_connectionService.Start();

				// todo; Check if the session is a pending session registered in the database
				// todo; if so, then reestablish the session with the provided context
				// otherwise we go the normal way

				SetState(SessionState.Authorizing);

				var authorizationInfo = await _authenticationService.Authorize(_communicationService);

				SetInfo(authorizationInfo);

				if (!authorizationInfo.IsAuthorized)
				{
					_sessionInfo.SetState(SessionState.FailedAuthorization);
					this.LogInfo("Retry authentication", Id);

					// todo: Retry authentication
					return;
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

		private void SetInfo(IAuthorizationInfo authorizationInfo)
		{
			_sessionInfo.SetUsername(authorizationInfo.Username);
			_sessionInfo.SetAuthorized(authorizationInfo.IsAuthorized);

			SaveSessionInfo();
		}

		private void SetState(SessionState state)
		{
			_sessionInfo.SetState(state);

			SaveSessionInfo();
		}

		private void Stop()
		{
			this.LogDebug($"Stopping session {Id}", Id);
			_connectionService.ConnectionLost -= OnConnectionLost;

			_sessionInfo.SetState(SessionState.Stopped);

			SaveSessionInfo();
		}

		private void SaveSessionInfo()
		{
			_sessionsService.SaveSessionInfo(_sessionInfo);
		}

		private void OnConnectionLost(string reason)
		{
			InvokeSessionOnHold(reason);
		}

		private void InvokeSessionOnHold(string reason)
		{
			SessionStopped?.Invoke(this, $"Connection lost: {reason}");
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