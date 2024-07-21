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
using SharedBeautifulData.Messages.CheckAlive;
using SharedBeautifulData.Messages.Login;
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

			RunConsoleInteraction();

			await _host.RunAsync(ClientProgramCancellationTokenSource.Token);

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
				var readLineTask = Task.Run(Console.ReadLine);
				var receiveMessageTask = communicationService?.ReceiveAsync<LoginRequest>();

				if (receiveMessageTask == null)
				{
					Console.WriteLine("Console interaction error!");
					continue;
				}

				var completedTask = await Task.WhenAny(readLineTask, receiveMessageTask);

				if (completedTask == readLineTask)
				{
					var input = await readLineTask;
					if (input == "e")
					{
						ClientProgramCancellationTokenSource.Cancel();
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
				else if (completedTask == receiveMessageTask)
				{
					await receiveMessageTask;
					Console.WriteLine("Please login with your username:");
					var readLine = await readLineTask;
					communicationService?.SendAsync(new LoginReply
					{
						Token = readLine
					});
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