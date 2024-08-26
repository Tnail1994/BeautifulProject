using BeautifulFundamental.Core;
using BeautifulFundamental.Core.Communication;
using BeautifulFundamental.Core.Communication.Client;
using BeautifulFundamental.Core.Communication.Implementations;
using BeautifulFundamental.Core.Communication.Transformation;
using BeautifulFundamental.Core.Identification;
using BeautifulFundamental.Core.Messages.Authorize;
using BeautifulFundamental.Core.Messages.CheckAlive;
using BeautifulFundamental.Core.Messages.RandomTestData;
using BeautifulFundamental.Core.Services.CheckAlive;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace BeautifulClientApplication
{
	internal class ProgramClient
	{
		private static readonly CancellationTokenSource ClientProgramCancellationTokenSource = new();
		private static IHost? _host;
		private static IConfigurationRoot? _config;

		static async Task Main(string[] args)
		{
			_host = CreateHostBuilder(args)
				.Build();

			var hostRunTask = _host.RunAsync(ClientProgramCancellationTokenSource.Token);

			RunConsoleInteraction();

			await hostRunTask;

			_host.Dispose();
		}

		private static async void RunConsoleInteraction()
		{
			if (_host == null)
				throw new NullReferenceException("Host is null");

			var communicationService = _host.Services.GetRequiredService<ICommunicationService>();

			PlotInfo();

			while (!ClientProgramCancellationTokenSource.IsCancellationRequested)
			{
				var input = Console.ReadLine();

				if (input == "e")
				{
					await ClientProgramCancellationTokenSource.CancelAsync();
					break;
				}
				else if (input == "i")
				{
					PlotInfo();
				}
				else
				{
					// Send dummy message
					communicationService?.SendAsync(new CheckAliveRequest());
				}
			}
		}

		private static void PlotInfo()
		{
			Console.WriteLine("Commands:");
			Console.WriteLine("i : Info");
			Console.WriteLine("e : Stop the client");
			Console.WriteLine("  : (Nothing) Send message");
		}

		private static IHostBuilder CreateHostBuilder(string[] args) =>
			Host.CreateDefaultBuilder(args)
				.UseBeautifulFundamentalCore()
				.ConfigureServices((hostContext, services) =>
				{
					// --- GENERAL ---
					services.AddHostedService<ClientManager>();

					// --- CONFIGURATION ---
					_config = FundamentalApplicationBuilder.CreateAndSetupConfig(services);
				});
	}
}