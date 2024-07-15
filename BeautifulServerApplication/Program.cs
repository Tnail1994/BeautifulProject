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

#if DEBUG
		private static IServiceProvider? _serviceProvider;
		private static ISessionManager? _sessionManager;
#endif

		static Task Main(string[] args)
		{
			Configure();

			var host = CreateHostBuilder(args)
				.Build();

			host.RunAsync(ServerProgramCancellationTokenSource.Token);

#if DEBUG
			_serviceProvider = host.Services;
			_sessionManager = _serviceProvider.GetRequiredService<IHostedService>() as ISessionManager;
#endif

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
			PlotInfo();

			while (!ServerProgramCancellationTokenSource.IsCancellationRequested)
			{
				var input = Console.ReadLine();
				if (input == "-e")
				{
					ServerProgramCancellationTokenSource.Cancel();
					break;
				}
#if DEBUG
				else if (input?.StartsWith("-sr") == true && _sessionManager != null)
				{
					_sessionManager.SendMessageToRandomClient(User.Create("1", "2"));
				}
				else if (input?.StartsWith("-sa") == true && _sessionManager != null)
				{
					_sessionManager.SendMessageToAllClients(User.Create("1", "2"));
				}
#endif
				else
				{
					PlotInfo();
				}
			}
		}

		private static void PlotInfo()
		{
			Console.WriteLine("Commands:");
			Console.WriteLine("-i : Info");
			Console.WriteLine("-e : Stop the server");
#if DEBUG
			Console.WriteLine("-sr: Send message to random client");
			Console.WriteLine("-sa: Send message to all client");
#endif
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