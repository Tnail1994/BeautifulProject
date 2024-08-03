using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;

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
			builder.Services.AddSingleton<IDataService, DataService>();

			builder.Services.AddTransient<MainView>();
			builder.Services.AddTransient<MainViewModel>();

			return builder.Build();
		}
	}
}