﻿using Microsoft.Extensions.DependencyInjection;
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
using Session.Common.Implementations;
using SharedBeautifulData;

namespace BeautifulClientApplication
{
	internal class ProgramClient
	{
		private static readonly CancellationTokenSource ClientProgramCancellationTokenSource = new();

		static Task Main(string[] args)
		{
			var host = CreateHostBuilder(args)
				.Build();

			host.RunAsync(ClientProgramCancellationTokenSource.Token);

			RunConsoleInteraction();

			return host.StopAsync();
		}

		private static void RunConsoleInteraction()
		{
			PlotInfo();

			while (!ClientProgramCancellationTokenSource.IsCancellationRequested)
			{
				var input = Console.ReadLine();
				if (input == "-e")
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
					// Send Message
				}
			}
		}

		private static void PlotInfo()
		{
			Console.WriteLine("Commands:");
			Console.WriteLine("i : Info");
			Console.WriteLine("e : Stop the server");
			Console.WriteLine("  : (Nothing) Send message");
		}

		private static IHostBuilder CreateHostBuilder(string[] args) =>
			Host.CreateDefaultBuilder(args)
				.ConfigureServices((hostContext, services) =>
				{
					services.AddHostedService<ClientManager>();

					services.AddTransient<IBaseMessage, UserMessage>();

					services.AddSingleton<ITransformerService, TransformerService>();

					services.AddSingleton<IAsyncClientFactory, AsyncClientFactory>();

					services.Configure<AsyncClientSettings>(
						hostContext.Configuration.GetSection(nameof(AsyncClientSettings)));

					services.AddSingleton<AsyncClientSettings>(provider =>
						provider.GetRequiredService<IOptions<AsyncClientSettings>>().Value);

					services.AddSingleton<ICommunicationService, CommunicationService>();
					services.AddSingleton<ISessionKey, SessionKey>();
				});
	}
}