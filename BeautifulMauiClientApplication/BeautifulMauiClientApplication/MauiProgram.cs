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
using BeautifulMauiClientApplication.Example;
using BeautifulMauiClientApplication.Login;
using BeautifulMauiClientApplication.Startup;
using CommunityToolkit.Maui;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BeautifulMauiClientApplication
{
	public static class MauiProgram
	{
		public static MauiApp CreateMauiApp()
		{
			// Lauch settings https://www.youtube.com/watch?v=GgU4ulGYQEk&t=70s to get all in one folder
			// zip it and send it to one. The one can unpack and should run it.

			var builder = MauiApp.CreateBuilder();
			builder
				.UseMauiApp<App>()
				.UseMauiCommunityToolkit()
				.ConfigureFonts(fonts =>
				{
					fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
					fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
				});

#if DEBUG
			builder.Logging.AddDebug();
#endif

			Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Development");

			var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");

			// Set up configuration
			var config = new ConfigurationBuilder()
				.AddJsonFile("appsettings.json")
				.AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
				.AddEnvironmentVariables()
				.Build();

			builder.Configuration.AddConfiguration(config);

			// Message types
			builder.Services.AddTransient<INetworkMessage, CheckAliveRequest>();
			builder.Services.AddTransient<INetworkMessage, CheckAliveReply>();
			builder.Services.AddTransient<INetworkMessage, LoginReply>();
			builder.Services.AddTransient<INetworkMessage, LoginRequest>();
			builder.Services.AddTransient<INetworkMessage, LogoutReply>();
			builder.Services.AddTransient<INetworkMessage, LogoutRequest>();
			builder.Services.AddTransient<INetworkMessage, RandomDataRequest>();
			builder.Services.AddTransient<INetworkMessage, RandomDataReply>();
			builder.Services.AddTransient<INetworkMessage, DeviceIdentRequest>();
			builder.Services.AddTransient<INetworkMessage, DeviceIdentReply>();

			// Services
			builder.Services.AddSingleton<IDataService, DataService>();

			builder.Services.AddSingleton<IIdentificationKey, IdentificationKey>();

			builder.Services.AddSingleton<IAsyncClientFactory, AsyncClientFactory>();

			builder.Services.AddSingleton<ITransformerService, TransformerService>();
			builder.Services.AddSingleton<IConnectionService, ConnectionService>();
			builder.Services.AddSingleton<ICommunicationService, CommunicationService>();
			builder.Services.AddSingleton<ICheckAliveService, CheckAliveService>();
			builder.Services.AddSingleton<IAsyncClient>(provider =>
				provider.GetRequiredService<IAsyncClientFactory>().Create());

			builder.Services.AddSingleton<IAutoSynchronizedMessageHandler, AutoSynchronizedMessageHandler>();

			builder.Services.AddSingleton<IStartupService, StartupService>();

			// Auto start services
			builder.Services.AddSingleton<ILoginService, LoginService>();
			builder.Services.AddSingleton<IAutoStartService>(sp => sp.GetRequiredService<ILoginService>());

			// Configurations
			builder.Services.AddSingleton<IAsyncClientSettings>(_ =>
				config.GetSection(nameof(AsyncClientSettings)).Get<AsyncClientSettings>() ??
				AsyncClientSettings.Default);
			builder.Services.AddSingleton<ICheckAliveSettings>(_ =>
				config.GetSection(nameof(CheckAliveSettings)).Get<CheckAliveSettings>() ??
				CheckAliveSettings.Default);
			builder.Services.AddSingleton<IConnectionSettings>(_ =>
				config.GetSection(nameof(ConnectionSettings)).Get<ConnectionSettings>() ??
				ConnectionSettings.Default);
			builder.Services.AddSingleton<IIdentificationKeySettings>(_ =>
				config.GetSection(nameof(IdentificationKeySettings)).Get<IdentificationKeySettings>() ??
				IdentificationKeySettings.Default);
			builder.Services.AddSingleton<ITlsSettings>(_ =>
				config.GetSection(nameof(TlsSettings)).Get<TlsSettings>() ??
				TlsSettings.Default);

			// Pages
			builder.Services.AddTransient<MainView>();
			builder.Services.AddTransient<MainViewModel>();

			builder.Services.AddTransient<TestView>();
			builder.Services.AddTransient<TestViewModel>();

			builder.Services.AddTransient<LoginView>();
			builder.Services.AddTransient<LoginViewModel>();

			// Contents
			builder.Services.AddTransient<RandomContent1ViewModel>();
			builder.Services.AddTransient<RandomContent2ViewModel>();
			builder.Services.AddTransient<RandomContent3ViewModel>();

			return builder.Build();
		}
	}
}