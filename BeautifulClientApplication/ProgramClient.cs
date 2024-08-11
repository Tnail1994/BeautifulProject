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
using Session.Common.Contracts;
using Session.Common.Implementations;
using SharedBeautifulData.Messages.Authorize;
using SharedBeautifulData.Messages.CheckAlive;
using SharedBeautifulData.Messages.RandomTestData;
using SharedBeautifulServices;
using SharedBeautifulServices.Common;

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
				.ConfigureServices((hostContext, services) =>
				{
					services.AddHostedService<ClientManager>();

					services.AddTransient<INetworkMessage, CheckAliveRequest>();
					services.AddTransient<INetworkMessage, CheckAliveReply>();
					services.AddTransient<INetworkMessage, LoginReply>();
					services.AddTransient<INetworkMessage, LoginRequest>();
					services.AddTransient<INetworkMessage, RandomDataRequest>();
					services.AddTransient<INetworkMessage, RandomDataReply>();
					services.AddTransient<INetworkMessage, DeviceIdentRequest>();
					services.AddTransient<INetworkMessage, DeviceIdentReply>();
					services.AddTransient<INetworkMessage, LogoutRequest>();
					services.AddTransient<INetworkMessage, LogoutReply>();

					services.AddSingleton<ISessionKey, SessionKey>();

					services.AddSingleton<IAsyncClientFactory, AsyncClientFactory>();

					services.AddSingleton<ITransformerService, TransformerService>();
					services.AddSingleton<IConnectionService, ConnectionService>();
					services.AddSingleton<ICommunicationService, CommunicationService>();
					services.AddSingleton<ICheckAliveService, CheckAliveService>();
					services.AddSingleton<IAsyncClient>(provider =>
						provider.GetRequiredService<IAsyncClientFactory>().Create());

					services.Configure<AsyncClientSettings>(
						hostContext.Configuration.GetSection(nameof(AsyncClientSettings)));
					services.Configure<CheckAliveSettings>(
						hostContext.Configuration.GetSection(nameof(CheckAliveSettings)));
					services.Configure<ConnectionSettings>(
						hostContext.Configuration.GetSection(nameof(ConnectionSettings)));
					services.Configure<SessionKeySettings>(
						hostContext.Configuration.GetSection(nameof(SessionKeySettings)));
					services.Configure<TlsSettings>(
						hostContext.Configuration.GetSection(nameof(TlsSettings)));

					services.AddSingleton<IAsyncClientSettings>(provider =>
						provider.GetRequiredService<IOptions<AsyncClientSettings>>().Value);
					services.AddSingleton<ICheckAliveSettings>(provider =>
						provider.GetRequiredService<IOptions<CheckAliveSettings>>().Value);
					services.AddSingleton<IConnectionSettings>(provider =>
						provider.GetRequiredService<IOptions<ConnectionSettings>>().Value);
					services.AddSingleton<ISessionKeySettings>(provider =>
						provider.GetRequiredService<IOptions<SessionKeySettings>>().Value);
					services.AddSingleton<ITlsSettings>(provider =>
						provider.GetRequiredService<IOptions<TlsSettings>>().Value);
				});
	}
}