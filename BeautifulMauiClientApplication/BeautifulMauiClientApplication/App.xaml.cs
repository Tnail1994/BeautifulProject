using BeautifulMauiClientApplication.Login.Services;
using BeautifulMauiClientApplication.Login.ViewModels;
using BeautifulMauiClientApplication.Startup;

namespace BeautifulMauiClientApplication
{
	public partial class App : Application
	{
		private readonly IStartupService _startupService;
		private readonly ILoginService _loginService;

		public App(IStartupService startupService, ILoginService loginService, LoginView loginView)
		{
			_startupService = startupService;
			_loginService = loginService;
			InitializeComponent();

			MainPage = loginView;

			StartApp();
		}

		private async void StartApp()
		{
			await _startupService.Start();

			if (await _loginService.AwaitLogin())
			{
				MainPage = new AppShell();
			}
		}
	}
}