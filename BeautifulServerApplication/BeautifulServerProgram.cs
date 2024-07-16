using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Remote.Communication;
using Remote.Communication.Client;
using Remote.Communication.Common.Client.Contracts;
using Remote.Communication.Common.Client.Implementations;
using Remote.Communication.Common.Contracts;
using Remote.Communication.Common.Implementations;
using Remote.Communication.Common.Transformation.Contracts;
using Remote.Communication.Transformation;
using Remote.Server;
using Remote.Server.Common.Contracts;
using Remote.Server.Common.Settings;
using Session;
using Session.Common.Contracts;
using Session.Common.Contracts.Services;
using Session.Common.Implementations;
using Session.Services;
using SharedBeautifulData;

namespace BeautifulServerApplication
{
	internal class BeautifulServerProgram
	{
		private static readonly CancellationTokenSource ServerProgramCancellationTokenSource = new();

#if DEBUG
		private static IServiceProvider? _serviceProvider;
		private static ISessionManager? _sessionManager;
#endif

		static Task Main(string[] args)
		{
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

					// Server wide
					services.AddTransient<IBaseMessage, UserMessage>();

					services.AddSingleton<IAsyncServer, AsyncServer>();
					services.AddSingleton<ITransformerService, TransformerService>();

					services.AddSingleton<ISessionFactory, SessionFactory>();
					services.AddSingleton<IAsyncClientFactory, AsyncClientFactory>();
					services.AddSingleton<IScopeFactory, ScopeFactory>();

					services.Configure<AsyncServerSettings>(
						hostContext.Configuration.GetSection(nameof(AsyncServerSettings)));
					services.Configure<AsyncClientSettings>(
						hostContext.Configuration.GetSection(nameof(AsyncClientSettings)));

					services.AddSingleton<AsyncServerSettings>(provider =>
						provider.GetRequiredService<IOptions<AsyncServerSettings>>().Value);
					services.AddSingleton<AsyncClientSettings>(provider =>
						provider.GetRequiredService<IOptions<AsyncClientSettings>>().Value);

					// Session wide
					services.AddScoped<ICommunicationService, CommunicationService>();
					services.AddScoped<ISessionKey, SessionKey>();
				});
	}
}