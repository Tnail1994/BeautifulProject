using Microsoft.Extensions.Hosting;
using Remote.Communication.Common.Client.Contracts;
using Remote.Communication.Common.Contracts;
using Serilog;

namespace BeautifulClientApplication
{
	internal interface IClientManager : IHostedService;

	internal class ClientManager(IAsyncClientFactory asyncClientFactory, ICommunicationService communicationService)
		: IClientManager
	{
		public Task StartAsync(CancellationToken cancellationToken)
		{
			var client = asyncClientFactory.Create();
			communicationService.SetClient(client);
			communicationService.Start();
			communicationService.ConnectionLost += OnConnectionLost;

			return Task.CompletedTask;
		}

		private void OnConnectionLost(object? sender, string e)
		{
			Log.Debug($"On connection to server lost.");
		}

		public Task StopAsync(CancellationToken cancellationToken)
		{
			communicationService.Dispose();
			return Task.CompletedTask;
		}
	}
}