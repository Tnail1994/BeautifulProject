using Core.Extensions;
using Remote.Communication.Common.Contracts;
using Session.Common.Contracts;
using Session.Common.Implementations;
using SharedBeautifulData;
using SharedBeautifulServices.Common;

namespace Session
{
	internal class Session : ISession, IDisposable
	{
		private readonly ICommunicationService _communicationService;
		private readonly ISessionKey _sessionKey;
		private readonly ICheckAliveService _checkAliveService;

		private Session(ICommunicationService communicationService, ISessionKey sessionKey,
			ICheckAliveService checkAliveService)
		{
			_sessionKey = sessionKey;
			_checkAliveService = checkAliveService;
			_checkAliveService.ConnectionLost += OnCheckAliveServiceConnectionLost;
			_communicationService = communicationService;
			_communicationService.ConnectionLost += OnCommunicationServiceConnectionLost;
		}

		public string Id => _sessionKey.SessionId;
		public event EventHandler<string>? SessionOnHold;

		public void Start()
		{
			this.LogDebug($"Starting session {Id}", Id);

			// From here the session can be used to communicate with the client.
			// All what happens here, should happen parallel to the main thread.
			// So beware of writing to the console or doing other blocking operations.
			// Need to define an own logging system for this session overall.

			try
			{
				StartCommunicationService();
				StartKeepAliveService();
			}
			catch (CheckAliveException checkAliveException)
			{
				this.LogError($"CheckAliveService failed to start. {checkAliveException.Message}", Id);
			}
			catch (Exception e)
			{
				this.LogFatal($"!!! Unexpected error while Start inside Session event\n" +
				              $"Message: {e.Message}\n" +
				              $"Stacktrace: {e.StackTrace}\n", Id);
			}
		}


		public void Stop()
		{
			this.LogDebug($"Stopping session {Id}", Id);

			Dispose();
		}


		private void StartCommunicationService()
		{
			try
			{
				_communicationService.Start();
			}
			catch (NullReferenceException nullReferenceException)
			{
				this.LogError($"Cannot start communication for this session: {Id}" +
				              $"Possible no client is set to the communication service. Check <cs_setClient>. Result = {_communicationService.IsClientSet}" +
				              $"{nullReferenceException.Message}", Id);

				if (!_communicationService.IsClientSet)
				{
					// Todo retry to set a client
				}
			}
			catch (Exception ex)
			{
				this.LogFatal($"!!! Unexpected {Id}" +
				              $"{ex.Message}", Id);
			}
		}

		private void StartKeepAliveService()
		{
			_checkAliveService.Start();
		}

		#region Factory

		public static ISession Create(ICommunicationService communicationService, ISessionKey sessionKey,
			ICheckAliveService checkAliveService)
		{
			return new Session(communicationService, sessionKey, checkAliveService);
		}

		#endregion

		private void OnCheckAliveServiceConnectionLost()
		{
			InvokeSessionOnHold("Check alive did not receive answer.");
		}

		private void OnCommunicationServiceConnectionLost(object? sender, string reason)
		{
			InvokeSessionOnHold(reason);
		}

		private void InvokeSessionOnHold(string reason)
		{
			SessionOnHold?.Invoke(this, $"Connection lost: {reason}");
		}

		public void Dispose()
		{
			_communicationService.Dispose();
		}


#if DEBUG
		public void SendMessageToClient(object message)
		{
			_communicationService.SendAsync(message);
		}
#endif
	}
}