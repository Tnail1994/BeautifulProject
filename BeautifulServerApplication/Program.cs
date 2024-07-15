using BeautifulServerApplication.Session;
using Configurations.General.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Remote.Core;
using Remote.Core.Communication;
using Remote.Core.Communication.Client;
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
			Configure();

			var host = CreateHostBuilder(args)
				.Build();

			host.RunAsync(ServerProgramCancellationTokenSource.Token);

			RunConsoleInteraction();

			return host.StopAsync();
		}

		private static void Configure()
		{
			var currentDirectory = Directory.GetCurrentDirectory();
			var basePath = Directory.GetParent(currentDirectory)?.Parent?.Parent?.ToString();

			try
			{
				if (!string.IsNullOrEmpty(basePath))
				{
					new ConfigurationBuilder()
						.SetBasePath(basePath)
						.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
						.AddEnvironmentVariables()
						.Build();
				}
			}
			catch (Exception ex)
			{
				Log.Fatal("!!! Unexpected error\n" +
				          "Base path is null or empty. Cannot load configuration." +
				          $"{ex.Message}");
			}
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

					services.AddSingleton<IAsyncServer, AsyncServer>();
					services.AddSingleton<ITransformerService, TransformerService>();

					services.AddSingleton<ISessionFactory, SessionFactory>();
					services.AddSingleton<IAsyncClientFactory, AsyncClientFactory>();
					services.AddSingleton<IScopeFactory, ScopeFactory>();

					services.AddScoped<ICommunicationService, CommunicationService>();

					services.Configure<AsyncServerSettings>(
						hostContext.Configuration.GetSection(nameof(AsyncServerSettings)));
					services.Configure<AsyncClientSettings>(
						hostContext.Configuration.GetSection(nameof(AsyncClientSettings)));
				});
	}
}