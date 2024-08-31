using BeautifulFundamental.Core.Communication;
using BeautifulFundamental.Core.Communication.Client;
using BeautifulFundamental.Core.Communication.Implementations;
using BeautifulFundamental.Core.Communication.Transformation;
using BeautifulFundamental.Core.Identification;
using BeautifulFundamental.Core.MessageHandling;
using BeautifulFundamental.Core.Messages.Authorize;
using BeautifulFundamental.Core.Messages.CheckAlive;
using BeautifulFundamental.Core.Messages.RandomTestData;
using BeautifulFundamental.Core.Services.CheckAlive;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BeautifulFundamental.Core
{
	public static class FundamentalApplicationBuilder
	{
		public static IHostBuilder UseBeautifulFundamentalCore(this IHostBuilder hostBuilder, bool scoped = false)
		{
			hostBuilder.ConfigureServices((_, services) => { RegisterBeautifulFundamentalCore(services, scoped); });

			return hostBuilder;
		}

		/// <summary>
		/// Using scoped flag means, adding following services as scoped
		/// IIdentificationKey, IAsyncClientFactory, ICommunicationService, IConnectionService,ICheckAliveService
		/// </summary>
		/// <param name="services"></param>
		/// <param name="scoped"></param>
		public static void RegisterBeautifulFundamentalCore(IServiceCollection services, bool scoped = false)
		{
			// Authentication
			services.AddTransient<INetworkMessage, DeviceIdentRequest>();
			services.AddTransient<INetworkMessage, DeviceIdentReply>();
			services.AddTransient<INetworkMessage, LoginReply>();
			services.AddTransient<INetworkMessage, LoginRequest>();
			services.AddTransient<INetworkMessage, LogoutRequest>();
			services.AddTransient<INetworkMessage, LogoutReply>();

			// Services
			services.AddTransient<INetworkMessage, CheckAliveRequest>();
			services.AddTransient<INetworkMessage, CheckAliveReply>();
			services.AddTransient<INetworkMessage, RegistrationRequest>();
			services.AddTransient<INetworkMessage, RegistrationReply>();

			// Testing
			services.AddTransient<INetworkMessage, RandomDataRequest>();
			services.AddTransient<INetworkMessage, RandomDataReply>();

			if (scoped)
			{
				services.AddScoped<IIdentificationKey, IdentificationKey>();
				services.AddScoped<IAsyncClientFactory, AsyncClientFactory>();
				services.AddScoped(provider =>
					provider.GetRequiredService<IAsyncClientFactory>().Create());

				services.AddScoped<ICommunicationService, CommunicationService>();
				services.AddScoped<IConnectionService, ConnectionService>();
				services.AddScoped<ICheckAliveService, CheckAliveService>();
				services.AddScoped<IAutoSynchronizedMessageHandler, AutoSynchronizedMessageHandler>();
			}
			else
			{
				services.AddSingleton<IIdentificationKey, IdentificationKey>();
				services.AddSingleton<IAsyncClientFactory, AsyncClientFactory>();
				services.AddSingleton(provider =>
					provider.GetRequiredService<IAsyncClientFactory>().Create());

				services.AddSingleton<ICommunicationService, CommunicationService>();
				services.AddSingleton<IConnectionService, ConnectionService>();
				services.AddSingleton<ICheckAliveService, CheckAliveService>();
				services.AddSingleton<IAutoSynchronizedMessageHandler, AutoSynchronizedMessageHandler>();
			}

			services.AddSingleton<ITransformerService, TransformerService>();
		}

		public static IConfigurationRoot CreateAndSetupConfig(IServiceCollection services)
		{
			Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Development");
			var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
			var config = new ConfigurationBuilder()
				.AddJsonFile("appsettings.json")
				.AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
				.AddEnvironmentVariables()
				.Build();
			services.AddSingleton<IAsyncClientSettings>(_ =>
				config.GetSection(nameof(AsyncClientSettings)).Get<AsyncClientSettings>() ??
				AsyncClientSettings.Default);
			services.AddSingleton<ICheckAliveSettings>(_ =>
				config.GetSection(nameof(CheckAliveSettings)).Get<CheckAliveSettings>() ??
				CheckAliveSettings.Default);
			services.AddSingleton<IConnectionSettings>(_ =>
				config.GetSection(nameof(ConnectionSettings)).Get<ConnectionSettings>() ??
				ConnectionSettings.Default);
			services.AddSingleton<IIdentificationKeySettings>(_ =>
				config.GetSection(nameof(IdentificationKeySettings)).Get<IdentificationKeySettings>() ??
				IdentificationKeySettings.Default);
			services.AddSingleton<ITlsSettings>(_ =>
				config.GetSection(nameof(TlsSettings)).Get<TlsSettings>() ??
				TlsSettings.Default);
			return config;
		}
	}
}