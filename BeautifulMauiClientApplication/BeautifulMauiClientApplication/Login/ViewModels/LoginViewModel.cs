using BeautifulMauiClientApplication.Login.Services;
using CommunityToolkit.Mvvm.ComponentModel;

namespace BeautifulMauiClientApplication.Login.ViewModels
{
	public class LoginViewModel : ObservableObject
	{
		private readonly ILoginService _loginService;

		public LoginViewModel(ILoginService loginService)
		{
			_loginService = loginService;
		}
	}
}