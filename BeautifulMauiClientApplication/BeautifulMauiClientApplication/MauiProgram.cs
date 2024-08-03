using System.Reflection;
using CommunityToolkit.Maui;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Remote.Communication;
using Remote.Communication.Client;
using Remote.Communication.Common.Client.Contracts;
using Remote.Communication.Common.Contracts;
using Remote.Communication.Common.Transformation.Contracts;
using Remote.Communication.Transformation;
using Session.Common.Contracts;
using Session.Common.Implementations;
using SharedBeautifulServices;
using SharedBeautifulServices.Common;

namespace BeautifulMauiClientApplication
{
	public static class MauiProgram
	{
		public static MauiApp CreateMauiApp()
		{
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
			// Set up configuration
			var config = new ConfigurationBuilder()
				.AddJsonFile("appsettings.json")
				.Build();
			builder.Configuration.AddConfiguration(config);

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
			builder.Services.AddTransient<MainViewModel>();

			builder.Services.AddTransient<TestView>();
			builder.Services.AddTransient<TestViewModel>();

			// Contents
			builder.Services.AddTransient<RandomContent1ViewModel>();

			return builder.Build();
		}
	}
}