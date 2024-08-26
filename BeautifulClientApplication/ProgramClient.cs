using BeautifulFundamental.Core;
using BeautifulFundamental.Core.Communication;
using BeautifulFundamental.Core.Messages.CheckAlive;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BeautifulClientApplication
{
	internal class ProgramClient
	{
		private static readonly CancellationTokenSource ClientProgramCancellationTokenSource = new();
		private static IHost? _host;

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
				.ConfigureServices((_, services) =>
				{
					// --- GENERAL ---
					services.AddHostedService<ClientManager>();

					// --- CONFIGURATION ---
					FundamentalApplicationBuilder.CreateAndSetupConfig(services);
				});
	}
}