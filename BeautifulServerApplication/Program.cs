using BeautifulServerApplication.Session;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Remote.Core;
using Remote.Core.Communication;
using Remote.Core.Transformation;
using Remote.Server;
using Remote.Server.Common.Contracts;

namespace BeautifulServerApplication
{
	internal class Program
	{
		private static readonly CancellationTokenSource ServerProgramCancellationTokenSource =
			new CancellationTokenSource();

		static Task Main(string[] args)
		{
			var host = CreateHostBuilder(args).Build();
			host.RunAsync(ServerProgramCancellationTokenSource.Token);

			RunConsoleInteraction();

			return host.StopAsync();
		}

		private static void RunConsoleInteraction()
		{
			Console.WriteLine("exit - Stop the server");
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

					services.AddTransient<ISessionFactory, SessionFactory>();
					services.AddTransient<IAsyncClientFactory, AsyncClientFactory>();

					services.AddScoped<ICommunicationService, CommunicationService>();
				});
	}
}