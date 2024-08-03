using Microsoft.Maui.Controls;

namespace BeautifulMauiClientApplication
{
	public partial class App : Application
	{
		public App()
		{
			InitializeComponent();

			MainPage = new AppShell();
		}
	}
}