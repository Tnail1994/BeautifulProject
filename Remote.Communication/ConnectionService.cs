using Core.Extensions;
using Remote.Communication.Common.Client.Contracts;
using Remote.Communication.Common.Contracts;
using Session.Common.Implementations;
using SharedBeautifulServices.Common;

namespace Remote.Communication
{
	public class ConnectionService : IConnectionService
	{
		private readonly IAsyncClient _asyncClient;
		private readonly ICommunicationService _communicationService;
		private readonly ICheckAliveService _checkAliveService;
		private readonly ISessionKey _sessionKey;
		public event Action<string>? ConnectionLost;
		public event Action? Reconnected;

		public ConnectionService(IAsyncClient asyncClient, ICommunicationService communicationService,
			ICheckAliveService checkAliveService, ISessionKey sessionKey)
		{
			_asyncClient = asyncClient;
			_communicationService = communicationService;
			_checkAliveService = checkAliveService;
			_sessionKey = sessionKey;

			_communicationService.ConnectionLost += OnConnectionLost;
			_checkAliveService.ConnectionLost += OnConnectionLost;
		}

		public async void Start()
		{
			var connectResult = await ConnectAsync();

			if (!connectResult)
				return;

			_communicationService.Start();
			_checkAliveService.Start();
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
			_communicationService.Stop();
			_checkAliveService.Stop();
		}

		private void OnConnectionLost(object? sender, string reason)
		{
			InvokeOnConnectionLost(reason);
		}

		private void InvokeOnConnectionLost(string reason)
		{
			ConnectionLost?.Invoke(reason);
			this.LogDebug("Try to reconnect");
			// Todo
		}

		private void OnConnectionLost()
		{
			InvokeOnConnectionLost("Check alive did not receive answer.");
		}

		public void Dispose()
		{
			_communicationService.Dispose();
			_checkAliveService.Dispose();
		}
	}
}