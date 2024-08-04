using System.Net.Sockets;
using Core.Extensions;
using Remote.Communication.Common.Client.Contracts;
using Remote.Communication.Common.Contracts;
using Session.Common.Implementations;
using SharedBeautifulServices.Common;

namespace Remote.Communication
{
	public class ConnectionService : IConnectionService, IDisposable
	{
		private readonly IAsyncClient _asyncClient;
		private readonly ICommunicationService _communicationService;
		private readonly ICheckAliveService _checkAliveService;
		private readonly ISessionKey _sessionKey;
		private readonly IConnectionSettings _connectionSettings;

		public event Action? ConnectionEstablished;
		public event Action<string>? ConnectionLost;
		public event Action? Reconnected;
		private bool _running;

		public ConnectionService(IAsyncClient asyncClient, ICommunicationService communicationService,
			ICheckAliveService checkAliveService, ISessionKey sessionKey, IConnectionSettings connectionSettings)
		{
			_asyncClient = asyncClient;
			_communicationService = communicationService;
			_checkAliveService = checkAliveService;
			_sessionKey = sessionKey;
			_connectionSettings = connectionSettings;

			_communicationService.ConnectionLost += OnConnectionLost;
			_checkAliveService.ConnectionLost += OnConnectionLost;
		}

		public async void Start()
		{
			if (_running)
			{
				this.LogVerbose("ConnectionService already running", _sessionKey.SessionId);
				return;
			}

			var connectResult = await ConnectAsync();

			if (!connectResult)
				return;

			_communicationService.Start();
			_checkAliveService.Start();

			_running = true;

			// Wait a short moment
			// todo refactor check alive and communication service to be tasks 
			await Task.Delay(1000);

			ConnectionEstablished?.Invoke();
		}

		private async Task<bool> ConnectAsync()
		{
			if (_asyncClient.IsConnected)
				return true;

			var connectionResult = await _asyncClient.ConnectAsync();

			if (connectionResult)
				return true;

			this.LogError("Connection to client failed. \n" +
			              "Check connection settings and try again.", _sessionKey.SessionId);
			return false;
		}

		public void Stop()
		{
			//if (!_running)
			//{
			//	this.LogVerbose("ConnectionService not running", _sessionKey.SessionId);
			//	return;
			//}

			_communicationService.ConnectionLost -= OnConnectionLost;
			_running = false;
		}

		private void OnConnectionLost(object? sender, string reason)
		{
			this.LogDebug($"[ConnectionService] Connection lost, invoke event. Id or reason:{reason}");
			InvokeOnConnectionLost(reason);
		}

		private void InvokeOnConnectionLost(string reason)
		{
			ConnectionLost?.Invoke(reason);

			if (!_connectionSettings.ReconnectActivated)
				return;

			StartTryReconnecting();
		}

		private async void StartTryReconnecting()
		{
			await Task.Delay(_connectionSettings.ReconnectDelayInSeconds * 1000);
			await TryReconnecting();
		}

		private async Task TryReconnecting(int attempts = 0)
		{
			this.LogDebug("Try to reconnect", _sessionKey.SessionId);

			if (attempts == 0)
			{
				_communicationService.Stop();
				_checkAliveService.Stop();
				Stop();
				_asyncClient.ResetSocket();
			}

			var reconnectAttempt = attempts;
			try
			{
				while (reconnectAttempt < _connectionSettings.ReconnectAttempts)
				{
					var connectResult = await ConnectAsync();

					if (connectResult)
					{
						Reconnected?.Invoke();
						return;
					}

					reconnectAttempt++;
				}
			}
			catch (SocketException)
			{
				reconnectAttempt++;
				await Task.Delay(_connectionSettings.ReconnectDelayInSeconds * 1000);
				await TryReconnecting(reconnectAttempt);
			}
			catch (Exception e)
			{
				this.LogFatal($"!!! Unexpected error while reconnecting\n" +
				              $"Message: {e.Message}\n" +
				              $"Stacktrace: {e.StackTrace}\n", _sessionKey.SessionId);
			}
		}

		private void OnConnectionLost()
		{
			InvokeOnConnectionLost("Check alive did not receive answer.");
		}

		public void Dispose()
		{
			Stop();
		}
	}
}