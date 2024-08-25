using BeautifulFundamental.Core.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BeautifulMauiClientApplication.Login
{
	public partial class LoginViewModel : ObservableObject
	{
		private readonly ILoginService _loginService;

		[ObservableProperty] private string _username;
		[ObservableProperty] private bool _rememberMe;

		[ObservableProperty] [NotifyPropertyChangedFor(nameof(IsNotBusy))]
		private bool _isBusy;

		public bool IsNotBusy => !IsBusy;

		public LoginViewModel(ILoginService loginService)
		{
			_loginService = loginService;

			// Maybe get from _loginService 
		}

		[RelayCommand]
		private async Task LoginAsync()
		{
			if (IsBusy)
				return;

			try
			{
				// Maybe make it cancelable 
				IsBusy = true;
				var loginResult = await _loginService.Login(Username, RememberMe);
			}
			catch (Exception ex)
			{
				this.LogError($"Cannot login. Unexcepected error: {ex.Message}\n" +
				              $"Stacktrace: {ex.StackTrace}");
			}
			finally
			{
				IsBusy = false;
			}
		}
	}
}