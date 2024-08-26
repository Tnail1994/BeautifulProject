using BeautifulFundamental.Core;
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

			// Configurations
			var config = FundamentalApplicationBuilder.CreateAndSetupConfig(builder.Services);
			builder.Configuration.AddConfiguration(config);

			FundamentalApplicationBuilder.RegisterBeautifulFundamentalCore(builder.Services);

			builder.Services.AddSingleton<IDataService, DataService>();
			builder.Services.AddSingleton<IAutoSynchronizedMessageHandler, AutoSynchronizedMessageHandler>();
			builder.Services.AddSingleton<IStartupService, StartupService>();

			// Auto start services
			builder.Services.AddSingleton<ILoginService, LoginService>();
			builder.Services.AddSingleton<IAutoStartService>(sp => sp.GetRequiredService<ILoginService>());


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