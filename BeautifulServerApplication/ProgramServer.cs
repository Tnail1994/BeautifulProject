using DbManagement;
using DbManagement.Common.Contracts;
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
using Serilog;
using Session.Common.Contracts;
using Session.Common.Implementations;
using Session.Contexts;
using Session.Core;
using Session.Example;
using Session.Services;
using Session.Services.Authorization;
using SharedBeautifulData.Messages.Authorize;
using SharedBeautifulData.Messages.CheckAlive;
using SharedBeautifulData.Messages.RandomTestData;
using SharedBeautifulServices;
using SharedBeautifulServices.Common;
using Users;
using Users.Common;

namespace BeautifulServerApplication
{
	internal class ProgramServer
	{
		private static readonly CancellationTokenSource ServerProgramCancellationTokenSource = new();

#if DEBUG
		private static IServiceProvider? _serviceProvider;
		private static ISessionManager? _sessionManager;
		private static IHost? _host;
#endif

		static async Task Main(string[] args)
		{
			AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
			AppDomain.CurrentDomain.ProcessExit += OnAppShutdown;

			_host = CreateHostBuilder(args)
				.Build();


			var hostRunTask = _host.RunAsync(ServerProgramCancellationTokenSource.Token);

#if DEBUG
			_serviceProvider = _host.Services;
			_sessionManager = _serviceProvider.GetRequiredService<IHostedService>() as ISessionManager;
#endif

			RunConsoleInteraction();

			await hostRunTask;

			Dispose();
		}

		private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			var eExceptionObject = ((Exception)e.ExceptionObject);
			Log.Fatal($"Unhandled exception occured! \n" +
			          $"Terminating: {e.IsTerminating} \n" +
			          $"Message: {eExceptionObject.Message} \n" +
			          $"Stacktrace: {eExceptionObject.StackTrace}");
			Dispose();
		}

		private static void Dispose()
		{
			_host?.Dispose();
		}

		private static void OnAppShutdown(object? sender, EventArgs e)
		{
			Dispose();
			ServerProgramCancellationTokenSource.Cancel();
		}

		private static async void RunConsoleInteraction()
		{
			PlotInfo();

			while (!ServerProgramCancellationTokenSource.IsCancellationRequested)
			{
				var input = Console.ReadLine();
				if (input == "-e")
				{
					await ServerProgramCancellationTokenSource.CancelAsync();
					break;
				}
#if DEBUG
				else if (input?.StartsWith("-sr") == true && _sessionManager != null)
				{
					var testMessage = new CheckAliveReply() { Success = false };
					_sessionManager.SendMessageToRandomClient(testMessage);
				}
				else if (input?.StartsWith("-sa") == true && _sessionManager != null)
				{
					var testMessage = new CheckAliveReply() { Success = false };
					_sessionManager.SendMessageToAllClients(testMessage);
				}
				else if (int.TryParse(input, out var value))
				{
					for (int i = 0; i < value; i++)
					{
						var randomMessage = new RandomDataRequest
						{
							MessageObject = i.ToString()
						};
						_sessionManager?.SendMessageToAllClients(randomMessage);

						// Without delay, the maximum send messages is round about 100
						//await Task.Delay(1);
					}
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
			Console.WriteLine("-x: Send a number x radom messages to all clients");
#endif
		}

		private static IHostBuilder CreateHostBuilder(string[] args) =>
			Host.CreateDefaultBuilder(args)
				.ConfigureServices((hostContext, services) =>
				{
					services.AddMemoryCache();

					services.AddHostedService<SessionManager>();

					// Server wide
					services.AddTransient<INetworkMessage, CheckAliveRequest>();
					services.AddTransient<INetworkMessage, CheckAliveReply>();
					services.AddTransient<INetworkMessage, LoginReply>();
					services.AddTransient<INetworkMessage, LoginRequest>();
					services.AddTransient<INetworkMessage, RandomDataRequest>();
					services.AddTransient<INetworkMessage, RandomDataReply>();
					services.AddTransient<INetworkMessage, DeviceIdentRequest>();
					services.AddTransient<INetworkMessage, DeviceIdentReply>();

					services.AddSingleton<UsersDbContext>();
					services.AddSingleton<SessionsDbContext>();
					services.AddSingleton<TurnContextCollection>();
					services.AddSingleton<RoundContextCollection>();
					services.AddSingleton<CurrentPlayerContextCollection>();

					services.AddSingleton<IDbContext>(sp => sp.GetRequiredService<UsersDbContext>());
					services.AddSingleton<IDbContext>(sp => sp.GetRequiredService<SessionsDbContext>());

					services.AddSingleton<IDbContext>(sp => sp.GetRequiredService<TurnContextCollection>());
					services.AddSingleton<IDbContext>(sp => sp.GetRequiredService<RoundContextCollection>());
					services.AddSingleton<IDbContext>(sp => sp.GetRequiredService<CurrentPlayerContextCollection>());

					services.AddSingleton<IContextCollection>(sp => sp.GetRequiredService<TurnContextCollection>());
					services.AddSingleton<IContextCollection>(sp => sp.GetRequiredService<RoundContextCollection>());
					services.AddSingleton<IContextCollection>(sp =>
						sp.GetRequiredService<CurrentPlayerContextCollection>());

					services.AddSingleton<IScopeManager, ScopeManager>();
					services.AddSingleton<IAsyncServer, AsyncServer>();
					services.AddSingleton<ITransformerService, TransformerService>();
					services.AddSingleton<IDbManager, DbManager>();
					services.AddSingleton<IDbContextResolver, DbContextResolver>();
					services.AddSingleton<IAuthenticationService, AuthenticationService>();
					services.AddSingleton<IUsersService, UsersService>();
					services.AddSingleton<ISessionsService, SessionsService>();

					services.AddSingleton<ISessionContextManager, SessionContextManager>();


					services.Configure<AsyncServerSettings>(
						hostContext.Configuration.GetSection(nameof(AsyncServerSettings)));
					services.Configure<AsyncClientSettings>(
						hostContext.Configuration.GetSection(nameof(AsyncClientSettings)));
					services.Configure<CheckAliveSettings>(
						hostContext.Configuration.GetSection(nameof(CheckAliveSettings)));
					services.Configure<ConnectionSettings>(
						hostContext.Configuration.GetSection(nameof(ConnectionSettings)));
					services.Configure<DbSettings>(
						hostContext.Configuration.GetSection(nameof(DbSettings)));
					services.Configure<DbContextSettings>(
						hostContext.Configuration.GetSection(nameof(DbContextSettings)));
					services.Configure<AuthenticationSettings>(
						hostContext.Configuration.GetSection(nameof(AuthenticationSettings)));
					services.Configure<SessionKeySettings>(
						hostContext.Configuration.GetSection(nameof(SessionKeySettings)));
					services.Configure<TlsSettings>(
						hostContext.Configuration.GetSection(nameof(TlsSettings)));

					services.AddSingleton<IAsyncServerSettings>(provider =>
						provider.GetRequiredService<IOptions<AsyncServerSettings>>().Value);
					services.AddSingleton<IAsyncClientSettings>(provider =>
						provider.GetRequiredService<IOptions<AsyncClientSettings>>().Value);
					services.AddSingleton<ICheckAliveSettings>(provider =>
						provider.GetRequiredService<IOptions<CheckAliveSettings>>().Value);
					services.AddSingleton<IConnectionSettings>(provider =>
						provider.GetRequiredService<IOptions<ConnectionSettings>>().Value);
					services.AddSingleton<IDbSettings>(provider =>
						provider.GetRequiredService<IOptions<DbSettings>>().Value);
					services.AddSingleton<IDbContextSettings>(provider =>
						provider.GetRequiredService<IOptions<DbContextSettings>>().Value);
					services.AddSingleton<IAuthenticationSettings>(provider =>
						provider.GetRequiredService<IOptions<AuthenticationSettings>>().Value);
					services.AddSingleton<ISessionKeySettings>(provider =>
						provider.GetRequiredService<IOptions<SessionKeySettings>>().Value);
					services.AddSingleton<ITlsSettings>(provider =>
						provider.GetRequiredService<IOptions<TlsSettings>>().Value);

					// Session wide
					services.AddScoped<ISession, Session.Core.Session>();
					services.AddScoped<IConnectionService, ConnectionService>();
					services.AddScoped<ICommunicationService, CommunicationService>();
					services.AddScoped<ISessionKey, SessionKey>();
					services.AddScoped<ICheckAliveService, CheckAliveService>();
					services.AddScoped<IAsyncClientFactory, AsyncClientFactory>();
					services.AddScoped<IAsyncClient>(provider =>
						provider.GetRequiredService<IAsyncClientFactory>().Create());

					services.AddScoped<ISessionContext, SessionContext>();
					services.AddScoped<ISessionDetailsProvider, SessionDetailsProvider>();

					// Make it lazy, because the details needed to be initialized. This happens, will
					// build and start the session. If the session is ready, then the loop will be needed.
					services.AddTransient<Lazy<ISessionLoop>>(provider =>
						new Lazy<ISessionLoop>(provider.GetRequiredService<ISessionLoop>));

					// Add own SessionLoop which extends from SessionLoopBase
					services.AddScoped<ISessionLoop, TestSessionLoop>();

					// Add the context, which should be got and saved automatically
					services.AddScoped<ITurnDetails>(sp =>
						sp.GetRequiredService<ISessionDetailsProvider>()
							.GetSessionDetail<TurnContextEntryDto, TurnDetails>() ??
						new TurnDetails(sp.GetRequiredService<ISessionKey>().SessionId));
					services.AddScoped<IRoundDetails>(sp =>
						sp.GetRequiredService<ISessionDetailsProvider>()
							.GetSessionDetail<RoundContextEntryDto, RoundDetails>() ??
						new RoundDetails(sp.GetRequiredService<ISessionKey>().SessionId));
					services.AddScoped<ICurrentPlayerDetails>(sp =>
						sp.GetRequiredService<ISessionDetailsProvider>()
							.GetSessionDetail<CurrentPlayerContextEntryDto, CurrentPlayerDetails>() ??
						new CurrentPlayerDetails(sp.GetRequiredService<ISessionKey>().SessionId));
				});
	}
}