using Core.Extensions;
using Microsoft.Extensions.Hosting;
using Remote.Communication.Common.Contracts;
using Serilog;
using SharedBeautifulData;

namespace BeautifulClientApplication
{
	internal interface IClientManager : IHostedService;

	internal class ClientManager(IConnectionService connectionService)
		: IClientManager
	{
		public Task StartAsync(CancellationToken cancellationToken)
		{
			try
			{
				connectionService.Start();
				connectionService.ConnectionLost += OnConnectionLost;
				connectionService.Reconnected += OnReconnected;
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
			throw new NotImplementedException();
		}

		public Task StopAsync(CancellationToken cancellationToken)
		{
			connectionService.Stop();
			return Task.CompletedTask;
		}
	}
}