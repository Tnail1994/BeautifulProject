using AutoSynchronizedMessageHandling;
using AutoSynchronizedMessageHandling.Common.Contracts;
using BeautifulMauiClientApplication.Example;
using CommunityToolkit.Maui;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
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
			builder.Services.AddTransient<INetworkMessage, RandomDataRequest>();
			builder.Services.AddTransient<INetworkMessage, RandomDataReply>();

			// Services
			builder.Services.AddSingleton<IDataService, DataService>();

			builder.Services.AddSingleton<ISessionKey, SessionKey>();

			builder.Services.AddSingleton<IAsyncClientFactory, AsyncClientFactory>();

			builder.Services.AddSingleton<ITransformerService, TransformerService>();
			builder.Services.AddSingleton<IConnectionService, ConnectionService>();
			builder.Services.AddSingleton<ICommunicationService, CommunicationService>();
			builder.Services.AddSingleton<ICheckAliveService, CheckAliveService>();
			builder.Services.AddSingleton<IAsyncClient>(provider =>
				provider.GetRequiredService<IAsyncClientFactory>().Create());

			builder.Services.AddSingleton<IAutoSynchronizedMessageHandler, AutoSynchronizedMessageHandler>();

			// Configurations
			builder.Services.AddSingleton<IAsyncClientSettings>(_ =>
				config.GetSection(nameof(AsyncClientSettings)).Get<AsyncClientSettings>() ??
				AsyncClientSettings.Default);
			builder.Services.AddSingleton<ICheckAliveSettings>(_ =>
				config.GetSection(nameof(CheckAliveSettings)).Get<CheckAliveSettings>() ??
				CheckAliveSettings.Default);
			builder.Services.AddSingleton<IAsyncClientFactorySettings>(_ =>
				config.GetSection(nameof(AsyncClientFactorySettings)).Get<AsyncClientFactorySettings>() ??
				AsyncClientFactorySettings.Default);
			builder.Services.AddSingleton<IConnectionSettings>(_ =>
				config.GetSection(nameof(ConnectionSettings)).Get<ConnectionSettings>() ??
				ConnectionSettings.Default);
			builder.Services.AddSingleton<ISessionKeySettings>(_ =>
				config.GetSection(nameof(SessionKeySettings)).Get<SessionKeySettings>() ??
				SessionKeySettings.Default);

			// Pages
			builder.Services.AddTransient<MainView>();
			builder.Services.AddTransient<Example.MainViewModel>();

			builder.Services.AddTransient<TestView>();
			builder.Services.AddTransient<TestViewModel>();

			// Contents
			builder.Services.AddTransient<Example.RandomContent1ViewModel>();
			builder.Services.AddTransient<Example.RandomContent2ViewModel>();
			builder.Services.AddTransient<Example.RandomContent3ViewModel>();

			return builder.Build();
		}
	}
}