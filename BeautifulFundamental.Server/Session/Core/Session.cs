﻿using BeautifulFundamental.Core.Communication;
using BeautifulFundamental.Core.Extensions;
using BeautifulFundamental.Core.Services.CheckAlive;
using BeautifulFundamental.Server.Session.Context;
using BeautifulFundamental.Server.Session.Implementations;
using BeautifulFundamental.Server.Session.Services.Authorization;
using BeautifulFundamental.Server.Session.Services.Session;
using BeautifulFundamental.Server.Session.Services.UserRegistration;

namespace BeautifulFundamental.Server.Session.Core
{
	public interface ISession
	{
		event EventHandler<SessionStoppedEventArgs>? SessionStopped;
		string Id { get; }

		void Start();

#if DEBUG
		void SendMessageToClient(object message);
#endif
	}

	public class Session : ISession, IDisposable
	{
		private readonly Lazy<ISessionLoop> _sessionLoop;
		private readonly ISessionContext _sessionContext;
		private readonly ISessionContextManager _sessionContextManager;
		private readonly IConnectionService _connectionService;
		private readonly IAuthenticationService _authenticationService;

		private readonly ICommunicationService _communicationService;
		private readonly ISessionsService _sessionsService;
		private readonly IUserRegistrationService _userRegistrationService;

		private SessionInfo _sessionInfo;

		public Session(Lazy<ISessionLoop> sessionLoop, ISessionContext sessionContext,
			ISessionContextManager sessionContextManager,
			IConnectionService connectionService,
			IAuthenticationService authenticationService, ICommunicationService communicationService,
			ISessionsService sessionsService, IUserRegistrationService userRegistrationService)
		{
			_sessionLoop = sessionLoop;
			_sessionContext = sessionContext;
			_sessionContextManager = sessionContextManager;
			_connectionService = connectionService;
			_authenticationService = authenticationService;
			_communicationService = communicationService;
			_sessionsService = sessionsService;
			_userRegistrationService = userRegistrationService;

			_sessionInfo = SessionInfo.Create(Id, string.Empty);
		}

		public string Id => _sessionContext.SessionId;
		public event EventHandler<SessionStoppedEventArgs>? SessionStopped;
		private ISessionLoop SessionLoop => _sessionLoop.Value;


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
				_userRegistrationService.Start();

				SetState(SessionState.Authorizing);

				var authorizationInfo = await _authenticationService.Authorize(_communicationService);

				SetInfo(authorizationInfo);

				if (!authorizationInfo.IsAuthorized)
				{
					SetState(SessionState.FailedAuthorization);
					InvokeSessionOnHold("Authorization failed");

					return;
				}

				if (_sessionsService.TryGetPendingSessionInfo(authorizationInfo.Username, out ISessionInfo sessionInfo))
				{
					ReestablishSessionContext(sessionInfo);
				}

				RunSession();

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

		private void ReestablishSessionContext(ISessionInfo sessionInfo)
		{
			this.LogDebug($"Reestablishing session {Id}", Id);

			UpdateInternalSessionInfoData(sessionInfo);

			// Grab session context data
			if (!_sessionContextManager.TryFillSessionContext(_sessionContext))
			{
				this.LogWarning($"Could not fill session context for {Id}. Try to reestablished failed.", Id);
			}

			RunSession();
		}

		private void UpdateInternalSessionInfoData(ISessionInfo sessionInfo)
		{
			_sessionsService.UpdateSession(this, sessionInfo);
			_sessionsService.TryRemove(_sessionInfo.Id);
			_sessionInfo = (SessionInfo)sessionInfo;
			_sessionContext.IdentificationKey.UpdateId(sessionInfo.Id);
			_sessionInfo.SetAuthorized(true);
		}

		private void RunSession()
		{
			this.LogDebug($"Running session {Id}", Id);

			// From here the session can be used to communicate with the client.
			// All what happens here, should happen parallel to the main thread.
			// So beware of writing to collections or doing other blocking operations.

			SessionLoop.Start();
		}

		private void SetInfo(IAuthorizationInfo authorizationInfo)
		{
			_sessionInfo.SetUsername(authorizationInfo.Username);
			_sessionInfo.SetAuthorized(authorizationInfo.IsAuthorized);
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
			_connectionService.ConnectionEstablished -= OnConnectionEstablished;

			SetState(SessionState.Stopped);

			_authenticationService.UnAuthorize(_communicationService, _sessionInfo.Username);

			TryRemoveSession();
		}

		private void TryRemoveSession()
		{
			if (_sessionInfo.SessionState == SessionState.Stopped)
				return;

			_sessionsService.TryRemove(_sessionContext.SessionId);
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
			SessionStopped?.Invoke(this,
				SessionStoppedEventArgs.Create(_sessionContext.IdentificationKey, $"Connection lost: {reason}"));
		}

		public void Dispose()
		{
			Stop();
		}


#if DEBUG
		public void SendMessageToClient(object message)
		{
			try
			{
				_communicationService.SendAsync(message);
			}
			catch (Exception)
			{
				// ignored
			}
		}
#endif
	}
}