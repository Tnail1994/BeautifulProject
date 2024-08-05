namespace BeautifulMauiClientApplication
{
	public partial class App : Application
	{
		public App(LoginView loginView)
		{
			InitializeComponent();

			MainPage = loginView;

			StartApp();
		}

		private async void StartApp()
		{
			if (MainPage?.BindingContext is not LoginViewModel loginViewModel)
				throw new InvalidOperationException(
					"Cannot start app, because loginViews BindingContext is not LoginViewModel?");


			var loginSuccessful = await loginViewModel.AwaitLogin();

			var maxAwaits = 10;
			while (!loginSuccessful && maxAwaits > 0)
			{
				loginSuccessful = await loginViewModel.AwaitLogin();
				maxAwaits--;
			}

			if (loginSuccessful)
			{
				MainPage = new AppShell();
			}
		}
	}
}