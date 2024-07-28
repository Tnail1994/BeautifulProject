using Core.Extensions;
using Microsoft.Extensions.Hosting;
using Remote.Communication.Common.Contracts;
using Serilog;
using SharedBeautifulData.Exceptions;

namespace BeautifulClientApplication
{
	internal interface IClientManager : IHostedService;

	internal class ClientManager : IClientManager
	{
		private readonly IConnectionService _connectionService;
		private CancellationToken _cancellationToken;

		public ClientManager(IConnectionService connectionService)
		{
			_connectionService = connectionService;
			_connectionService.ConnectionLost += OnConnectionLost;
			_connectionService.Reconnected += OnReconnected;
		}

		public Task StartAsync(CancellationToken cancellationToken)
		{
			try
			{
				_connectionService.Start();
				_cancellationToken = cancellationToken;
			}
			catch (CheckAliveException checkAliveException)
			{
				this.LogError($"CheckAliveService failed to start. {checkAliveException.Message}");
			}
			catch (Exception e)
			{
				this.LogFatal($"!!! Unexpected error while StartAsync event\n" +
				              $"Message: {e.Message}\n" +
				              $"Stacktrace: {e.StackTrace}\n");
			}

			return Task.CompletedTask;
		}

		private void OnConnectionLost(string obj)
		{
			Log.Debug($"On connection to server lost.");
		}

		private void OnReconnected()
		{
			this.LogDebug($"On reconnected to server.");
			StartAsync(_cancellationToken);
		}

		public Task StopAsync(CancellationToken cancellationToken)
		{
			_connectionService.ConnectionLost -= OnConnectionLost;
			_connectionService.Reconnected -= OnReconnected;
			return Task.CompletedTask;
		}
	}
}