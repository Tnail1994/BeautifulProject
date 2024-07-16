using Core.Extensions;
using Microsoft.Extensions.Hosting;
using Remote.Communication.Common.Client.Contracts;
using Remote.Communication.Common.Contracts;
using Serilog;
using SharedBeautifulData;
using SharedBeautifulServices.Common;

namespace BeautifulClientApplication
{
	internal interface IClientManager : IHostedService;

	internal class ClientManager(
		IAsyncClientFactory asyncClientFactory,
		ICheckAliveService checkAliveService,
		ICommunicationService communicationService)
		: IClientManager
	{
		public Task StartAsync(CancellationToken cancellationToken)
		{
			try
			{
				var client = asyncClientFactory.Create();
				communicationService.ConnectionLost += OnCommunicationServiceConnectionLost;
				checkAliveService.ConnectionLost += OnCheckAliveConnectionLost;
				communicationService.SetClient(client);
				communicationService.Start();
				checkAliveService.Start();
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

		private void OnCheckAliveConnectionLost()
		{
			Log.Debug($"On connection to server lost.");
		}

		private void OnCommunicationServiceConnectionLost(object? sender, string e)
		{
			Log.Debug($"On connection to server lost.");
		}

		public Task StopAsync(CancellationToken cancellationToken)
		{
			communicationService.Dispose();
			checkAliveService.Stop();
			return Task.CompletedTask;
		}
	}
}