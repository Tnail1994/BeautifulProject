using BeautifulFundamental.Core;
using BeautifulFundamental.Core.Communication;
using BeautifulFundamental.Core.Communication.Client;
using BeautifulFundamental.Core.Identification;
using BeautifulFundamental.Core.Messages.CheckAlive;
using BeautifulFundamental.Core.Messages.RandomTestData;
using BeautifulFundamental.Core.Services.CheckAlive;
using BeautifulFundamental.Server;
using BeautifulFundamental.Server.Core;
using BeautifulFundamental.Server.Db;
using BeautifulFundamental.Server.Session.Contracts.Context;
using BeautifulFundamental.Server.Session.Contracts.Context.Db;
using BeautifulFundamental.Server.Session.Contracts.Core;
using BeautifulFundamental.Server.Session.Contracts.Services.Authorization;
using BeautifulFundamental.Server.Session.Services.Authorization;
using Microsoft.Extensions.Configuration;
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
		private static IConfigurationRoot? _config;
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
				.UseBeautifulFundamentalServer()
				.ConfigureServices((hostContext, services) =>
				{
					// --- CONFIGURATION ---
					_config = FundamentalServerApplicationBuilder.CreateAndSetupConfig(services);

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