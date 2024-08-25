using BeautifulFundamental.Core.Communication;
using BeautifulFundamental.Core.Communication.Client;
using BeautifulFundamental.Core.Communication.Implementations;
using BeautifulFundamental.Core.Communication.Transformation;
using BeautifulFundamental.Core.Identification;
using BeautifulFundamental.Core.Messages.Authorize;
using BeautifulFundamental.Core.Messages.CheckAlive;
using BeautifulFundamental.Core.Messages.RandomTestData;
using BeautifulFundamental.Core.Services.CheckAlive;
using BeautifulFundamental.Server.Core;
using BeautifulFundamental.Server.Db;
using BeautifulFundamental.Server.Session.Context;
using BeautifulFundamental.Server.Session.Context.Db;
using BeautifulFundamental.Server.Session.Contracts.Context;
using BeautifulFundamental.Server.Session.Contracts.Context.Db;
using BeautifulFundamental.Server.Session.Contracts.Core;
using BeautifulFundamental.Server.Session.Contracts.Scope;
using BeautifulFundamental.Server.Session.Contracts.Services;
using BeautifulFundamental.Server.Session.Contracts.Services.Authorization;
using BeautifulFundamental.Server.Session.Core;
using BeautifulFundamental.Server.Session.Scope;
using BeautifulFundamental.Server.Session.Services;
using BeautifulFundamental.Server.Session.Services.Authorization;
using BeautifulFundamental.Server.UserManagement;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Serilog;
using Session.Example;

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
					// --- GENERAL ---
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
					services.AddSingleton<ISessionDataProvider, SessionsDbContext>();

					services.AddSingleton<IDbContext>(sp => sp.GetRequiredService<UsersDbContext>());
					services.AddSingleton<IDbContext>(sp => sp.GetRequiredService<SessionsDbContext>());

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
					services.Configure<IdentificationKeySettings>(
						hostContext.Configuration.GetSection(nameof(IdentificationKeySettings)));
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
					services.AddSingleton<IIdentificationKeySettings>(provider =>
						provider.GetRequiredService<IOptions<IdentificationKeySettings>>().Value);
					services.AddSingleton<ITlsSettings>(provider =>
						provider.GetRequiredService<IOptions<TlsSettings>>().Value);

					// Session wide
					services.AddScoped<ISession, BeautifulFundamental.Server.Session.Core.Session>();
					services.AddScoped<IConnectionService, ConnectionService>();
					services.AddScoped<ICommunicationService, CommunicationService>();
					services.AddScoped<IIdentificationKey, IdentificationKey>();
					services.AddScoped<ICheckAliveService, CheckAliveService>();
					services.AddScoped<IAsyncClientFactory, AsyncClientFactory>();
					services.AddScoped(provider =>
						provider.GetRequiredService<IAsyncClientFactory>().Create());

					services.AddScoped<ISessionContext, SessionContext>();
					services.AddScoped<ISessionDetailsManager, SessionDetailsManager>();

					// Make it lazy, because the details needed to be initialized. This happens, will
					// build and start the session. If the session is ready, then the loop will be needed.
					services.AddTransient<Lazy<ISessionLoop>>(provider =>
						new Lazy<ISessionLoop>(provider.GetRequiredService<ISessionLoop>));

					// --- SPECIALIZED (EXAMPLE) ---
					// Add own SessionLoop which extends from SessionLoopBase
					services.AddScoped<ISessionLoop, TestSessionLoop>();

					// Add the context, which should be got and saved automatically
					// EntryDto: The entry of the context collection, which should be saved inside db. Must provide
					//			 a convert and update method. Convert to ISessionDetail and update from ISessionDetail
					// SessionDetail: The object to work with. Must provide a convert method as well to create the Entity correctly
					services.AddSingleton<TurnContextCollection>();
					services.AddSingleton<RoundContextCollection>();
					services.AddSingleton<CurrentPlayerContextCollection>();
					services.AddSingleton<IDbContext>(sp => sp.GetRequiredService<TurnContextCollection>());
					services.AddSingleton<IDbContext>(sp => sp.GetRequiredService<RoundContextCollection>());
					services.AddSingleton<IDbContext>(sp => sp.GetRequiredService<CurrentPlayerContextCollection>());

					services.AddSingleton<IContextCollection>(sp => sp.GetRequiredService<TurnContextCollection>());
					services.AddSingleton<IContextCollection>(sp => sp.GetRequiredService<RoundContextCollection>());
					services.AddSingleton<IContextCollection>(sp =>
						sp.GetRequiredService<CurrentPlayerContextCollection>());

					services.AddScoped(GetTurnDetails);
					services.AddScoped(GetRoundDetails);
					services.AddScoped(GetCurrentPlayerDetails);
				});

		private static ITurnDetails GetTurnDetails(IServiceProvider sp)
		{
			var sessionDetailsManager = sp.GetRequiredService<ISessionDetailsManager>();
			var sessionDetail = sessionDetailsManager
				.GetSessionDetail<TurnContextEntryDto, TurnDetails>();

			if (sessionDetail == null)
			{
				sessionDetail = new TurnDetails(sp.GetRequiredService<IIdentificationKey>());
				sessionDetailsManager.Observe(sessionDetail);
			}

			return sessionDetail;
		}

		private static IRoundDetails GetRoundDetails(IServiceProvider sp)
		{
			var sessionDetailsManager = sp.GetRequiredService<ISessionDetailsManager>();
			var sessionDetail = sessionDetailsManager
				.GetSessionDetail<RoundContextEntryDto, RoundDetails>();

			if (sessionDetail == null)
			{
				sessionDetail = new RoundDetails(sp.GetRequiredService<IIdentificationKey>());
				sessionDetailsManager.Observe(sessionDetail);
			}

			return sessionDetail;
		}

		private static ICurrentPlayerDetails GetCurrentPlayerDetails(IServiceProvider sp)
		{
			var sessionDetailsManager = sp.GetRequiredService<ISessionDetailsManager>();
			var sessionDetail = sessionDetailsManager
				.GetSessionDetail<CurrentPlayerContextEntryDto, CurrentPlayerDetails>();

			if (sessionDetail == null)
			{
				sessionDetail = new CurrentPlayerDetails(sp.GetRequiredService<IIdentificationKey>());
				sessionDetailsManager.Observe(sessionDetail);
			}

			return sessionDetail;
		}
	}
}