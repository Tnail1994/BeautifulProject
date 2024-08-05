namespace BeautifulMauiClientApplication.Login
{
	public partial class LoginView : ContentPage
	{
		public LoginView(LoginViewModel loginViewModel)
		{
			InitializeComponent();
			BindingContext = loginViewModel;
		}
	}
}