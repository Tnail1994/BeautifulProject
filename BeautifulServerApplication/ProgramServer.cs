using DbManagement;
using DbManagement.Common.Contracts;
using DbManagement.Contexts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Remote.Communication;
using Remote.Communication.Client;
using Remote.Communication.Common.Client.Contracts;
using Remote.Communication.Common.Contracts;
using Remote.Communication.Common.Implementations;
using Remote.Communication.Common.Transformation.Contracts;
using Remote.Communication.Transformation;
using Remote.Server;
using Remote.Server.Common.Contracts;
using Session;
using Session.Common.Contracts;
using Session.Common.Implementations;
using SharedBeautifulData.Messages;
using SharedBeautifulData.Objects;
using SharedBeautifulServices;
using SharedBeautifulServices.Common;

namespace BeautifulServerApplication
{
	internal class ProgramServer
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

			host.Dispose();
			return Task.CompletedTask;
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
					_sessionManager.SendMessageToRandomClient(new UserMessage
						{ User = User.Create("1") });
				}
				else if (input?.StartsWith("-sa") == true && _sessionManager != null)
				{
					_sessionManager.SendMessageToAllClients(new UserMessage
						{ User = User.Create("2") });
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
					services.AddMemoryCache();

					services.AddHostedService<SessionManager>();

					// Server wide
					services.AddTransient<IBaseMessage, UserMessage>();
					services.AddTransient<IBaseMessage, CheckAliveMessage>();
					services.AddTransient<IBaseMessage, CheckAliveReplyMessage>();
					services.AddTransient<IDbContext, UsersDbContext>();

					services.AddSingleton<IAsyncServer, AsyncServer>();
					services.AddSingleton<ITransformerService, TransformerService>();
					services.AddSingleton<IDbManager, DbManager>();
					services.AddSingleton<IDbContextResolver, DbContextResolver>();
					services.AddSingleton<IAuthenticationService, AuthenticationService>();

					services.AddSingleton<IScopeManager, ScopeManager>();

					services.Configure<AsyncServerSettings>(
						hostContext.Configuration.GetSection(nameof(AsyncServerSettings)));
					services.Configure<AsyncClientSettings>(
						hostContext.Configuration.GetSection(nameof(AsyncClientSettings)));
					services.Configure<CheckAliveSettings>(
						hostContext.Configuration.GetSection(nameof(CheckAliveSettings)));
					services.Configure<AsyncClientFactorySettings>(
						hostContext.Configuration.GetSection(nameof(AsyncClientFactorySettings)));
					services.Configure<ConnectionSettings>(
						hostContext.Configuration.GetSection(nameof(ConnectionSettings)));
					services.Configure<DbSettings>(
						hostContext.Configuration.GetSection(nameof(DbSettings)));
					services.Configure<DbContextSettings>(
						hostContext.Configuration.GetSection(nameof(DbContextSettings)));

					services.AddSingleton<IAsyncServerSettings>(provider =>
						provider.GetRequiredService<IOptions<AsyncServerSettings>>().Value);
					services.AddSingleton<IAsyncClientSettings>(provider =>
						provider.GetRequiredService<IOptions<AsyncClientSettings>>().Value);
					services.AddSingleton<ICheckAliveSettings>(provider =>
						provider.GetRequiredService<IOptions<CheckAliveSettings>>().Value);
					services.AddSingleton<IAsyncClientFactorySettings>(provider =>
						provider.GetRequiredService<IOptions<AsyncClientFactorySettings>>().Value);
					services.AddSingleton<IConnectionSettings>(provider =>
						provider.GetRequiredService<IOptions<ConnectionSettings>>().Value);
					services.AddSingleton<IDbSettings>(provider =>
						provider.GetRequiredService<IOptions<DbSettings>>().Value);
					services.AddSingleton<IDbContextSettings>(provider =>
						provider.GetRequiredService<IOptions<DbContextSettings>>().Value);

					// Session wide
					services.AddScoped<ISession, Session.Session>();
					services.AddScoped<IConnectionService, ConnectionService>();
					services.AddScoped<ICommunicationService, CommunicationService>();
					services.AddScoped<ISessionKey, SessionKey>();
					services.AddScoped<ICheckAliveService, CheckAliveService>();
					services.AddScoped<IAsyncClientFactory, AsyncClientFactory>();
					services.AddScoped<IAsyncClient>(provider =>
						provider.GetRequiredService<IAsyncClientFactory>().Create());
				});
	}
}