﻿using System.Net.Sockets;
using BeautifulFundamental.Core.Communication.Client;
using BeautifulFundamental.Core.Extensions;
using BeautifulFundamental.Core.Identification;
using BeautifulFundamental.Core.Services.CheckAlive;

namespace BeautifulFundamental.Core.Communication
{
	public interface IConnectionService
	{
		event Action ConnectionEstablished;
		event Action<string> ConnectionLost;
		event Action Reconnected;
		void Start();
		void Stop(bool force = false);
	}

	public class ConnectionService : IConnectionService, IDisposable
	{
		private readonly IAsyncClient _asyncClient;
		private readonly ICommunicationService _communicationService;
		private readonly ICheckAliveService _checkAliveService;
		private readonly IIdentificationKey _identificationKey;
		private readonly IConnectionSettings _connectionSettings;

		public event Action? ConnectionEstablished;
		public event Action<string>? ConnectionLost;
		public event Action? Reconnected;
		private bool _running;

		public ConnectionService(IAsyncClient asyncClient, ICommunicationService communicationService,
			ICheckAliveService checkAliveService, IIdentificationKey identificationKey,
			IConnectionSettings connectionSettings)
		{
			_asyncClient = asyncClient;
			_communicationService = communicationService;
			_checkAliveService = checkAliveService;
			_identificationKey = identificationKey;
			_connectionSettings = connectionSettings;

			_communicationService.ConnectionLost += OnConnectionLost;
			_checkAliveService.ConnectionLost += OnConnectionLost;
		}

		public async void Start()
		{
			if (_running)
			{
				this.LogVerbose("ConnectionService already running", _identificationKey.SessionId);
				return;
			}

			await TryConnecting();

			if (_asyncClient.IsNotConnected)
			{
				this.LogError($"Cannot start ConnectionService because client is not connected.",
					_identificationKey.SessionId);
				return;
			}

			_communicationService.Start();
			_checkAliveService.Start();

			_running = true;

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
			              "Check connection settings and try again.", _identificationKey.SessionId);
			return false;
		}

		public void Stop(bool force = false)
		{
			//if (!_running)
			//{
			//	this.LogVerbose("ConnectionService not running", _identificationKey.SessionId);
			//	return;
			//}

			_running = false;

			if (force)
			{
				_checkAliveService.Stop(force);
				_communicationService.Stop(force);
			}
		}

		private void OnConnectionLost(object? sender, string reason)
		{
			this.LogDebug($"[ConnectionService] Connection lost, invoke event. Id or reason:{reason}",
				_identificationKey.SessionId);
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
			_communicationService.Stop();
			_checkAliveService.Stop();
			Stop();

			await TryConnecting(0, true);
		}

		private async Task TryConnecting(int attempts = 0, bool reconnectingAttempt = false)
		{
			if (reconnectingAttempt)
				_asyncClient.ResetSocket();

			if (_asyncClient.IsConnected)
				return;

			this.LogDebug("Try to connect", _identificationKey.SessionId);

			var reconnectAttempt = attempts;

			try
			{
				while (reconnectAttempt < _connectionSettings.ReconnectAttempts)
				{
					var connectResult = await ConnectAsync();

					if (connectResult && reconnectingAttempt)
					{
						Reconnected?.Invoke();
						return;
					}

					reconnectAttempt++;
				}
			}
			catch (SocketException)
			{
				await HandleSocketException(reconnectAttempt, reconnectingAttempt);
			}
			catch (Exception ex)
			{
				if (ex.InnerException is SocketException)
					await HandleSocketException(reconnectAttempt, reconnectingAttempt);
				else
				{
					this.LogFatal($"!!! Unexpected error while reconnectingAttempt\n" +
					              $"Message: {ex.Message}\n" +
					              $"Stacktrace: {ex.StackTrace}\n", _identificationKey.SessionId);
				}
			}
		}

		private async Task HandleSocketException(int reconnectAttempt, bool reconnectingAttempt)
		{
			reconnectAttempt++;
			await Task.Delay(_connectionSettings.ReconnectDelayInSeconds * 1000);
			await TryConnecting(reconnectAttempt, reconnectingAttempt);
		}

		private void OnConnectionLost()
		{
			InvokeOnConnectionLost("Check alive did not receive answer.");
		}

		public void Dispose()
		{
			Stop();
			_checkAliveService.ConnectionLost -= OnConnectionLost;
			_communicationService.ConnectionLost -= OnConnectionLost;
		}
	}
}