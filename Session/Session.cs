using Core.Extensions;
using Remote.Communication.Common.Contracts;
using Session.Common.Contracts;
using Session.Common.Implementations;
using SharedBeautifulData;

namespace Session
{
	public class Session : ISession, IDisposable
	{
		private readonly ISessionKey _sessionKey;
		private readonly IConnectionService _connectionService;

#if DEBUG
		private readonly ICommunicationService _communicationService;
#endif

		public Session(ISessionKey sessionKey, IConnectionService connectionService
#if DEBUG
			, ICommunicationService communicationService
#endif
		)
		{
			_sessionKey = sessionKey;
			_connectionService = connectionService;
			_communicationService = communicationService;
			_connectionService.ConnectionLost += OnConnectionLost;
			_connectionService.Reconnected += OnReconnected;
		}

		private void OnReconnected()
		{
			throw new NotImplementedException();
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
				_connectionService.Start();
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

		private void OnConnectionLost(string reason)
		{
			InvokeSessionOnHold(reason);
		}

		private void InvokeSessionOnHold(string reason)
		{
			SessionOnHold?.Invoke(this, $"Connection lost: {reason}");
		}

		public void Dispose()
		{
			_connectionService.Dispose();
		}


#if DEBUG
		public void SendMessageToClient(object message)
		{
			_communicationService.SendAsync(message);
		}
#endif
	}
}