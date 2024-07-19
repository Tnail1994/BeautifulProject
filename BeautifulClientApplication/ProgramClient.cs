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
using Session.Common.Implementations;
using SharedBeautifulData.Messages;
using SharedBeautifulData.Messages.CheckAlive;
using SharedBeautifulData.Messages.Login;
using SharedBeautifulData.Objects;
using SharedBeautifulServices;
using SharedBeautifulServices.Common;

namespace BeautifulClientApplication
{
	internal class ProgramClient
	{
		private static readonly CancellationTokenSource ClientProgramCancellationTokenSource = new();
		private static ICommunicationService? _communicationService;

		static Task Main(string[] args)
		{
			var host = CreateHostBuilder(args)
				.Build();

			host.RunAsync(ClientProgramCancellationTokenSource.Token);
			_communicationService = host.Services.GetRequiredService<ICommunicationService>();

			RunConsoleInteraction();

			host.Dispose();

			return Task.CompletedTask;
		}

		private static void RunConsoleInteraction()
		{
			PlotInfo();

			while (!ClientProgramCancellationTokenSource.IsCancellationRequested)
			{
				var input = Console.ReadLine();
				if (input == "e")
				{
					ClientProgramCancellationTokenSource.Cancel();
					break;
				}
				else if (input == "i")
				{
					PlotInfo();
				}
				else if (_communicationService != null)
				{
					_communicationService.SendAsync(new UserMessage { User = User.Create("tk") });
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

					services.AddTransient<IBaseMessage, UserMessage>();
					services.AddTransient<IBaseMessage, CheckAliveRequest>();
					services.AddTransient<IBaseMessage, CheckAliveReply>();
					services.AddTransient<IBaseMessage, LoginReply>();
					services.AddTransient<IBaseMessage, LoginRequest>();

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
					services.Configure<AsyncClientFactorySettings>(
						hostContext.Configuration.GetSection(nameof(AsyncClientFactorySettings)));
					services.Configure<ConnectionSettings>(
						hostContext.Configuration.GetSection(nameof(ConnectionSettings)));

					services.AddSingleton<IAsyncClientSettings>(provider =>
						provider.GetRequiredService<IOptions<AsyncClientSettings>>().Value);
					services.AddSingleton<ICheckAliveSettings>(provider =>
						provider.GetRequiredService<IOptions<CheckAliveSettings>>().Value);
					services.AddSingleton<IAsyncClientFactorySettings>(provider =>
						provider.GetRequiredService<IOptions<AsyncClientFactorySettings>>().Value);
					services.AddSingleton<IConnectionSettings>(provider =>
						provider.GetRequiredService<IOptions<ConnectionSettings>>().Value);
				});
	}
}