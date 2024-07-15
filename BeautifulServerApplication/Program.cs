using BeautifulServerApplication.Session;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Remote.Core;
using Remote.Core.Communication;
using Remote.Core.Transformation;
using Remote.Server;
using Remote.Server.Common.Contracts;
using Serilog;

namespace BeautifulServerApplication
{
	internal class Program
	{
		private static readonly CancellationTokenSource ServerProgramCancellationTokenSource = new();

		static Task Main(string[] args)
		{
			var host = CreateHostBuilder(args)
				.Build();

			host.RunAsync(ServerProgramCancellationTokenSource.Token);

			RunConsoleInteraction();

			return host.StopAsync();
		}

		private static void RunConsoleInteraction()
		{
			Log.Debug("exit - Stop the server");
			while (!ServerProgramCancellationTokenSource.IsCancellationRequested)
			{
				var input = Console.ReadLine();
				if (input?.ToLower() == "exit")
				{
					ServerProgramCancellationTokenSource.Cancel();
					break;
				}
			}
		}

		private static IHostBuilder CreateHostBuilder(string[] args) =>
			Host.CreateDefaultBuilder(args)
				.ConfigureServices((hostContext, services) =>
				{
					services.AddHostedService<SessionManager>();

					services.AddTransient<IBaseMessage, UserMessage>();

					services.AddSingleton<IAsyncSocketServer, AsyncSocketServer>();
					services.AddSingleton<ITransformerService, TransformerService>();

					services.AddSingleton<ISessionFactory, SessionFactory>();
					services.AddSingleton<IAsyncClientFactory, AsyncClientFactory>();
					services.AddSingleton<IScopeFactory, ScopeFactory>();

					services.AddScoped<ICommunicationService, CommunicationService>();
				});
	}
}