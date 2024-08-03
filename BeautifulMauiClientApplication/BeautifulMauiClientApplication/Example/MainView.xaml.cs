namespace BeautifulMauiClientApplication
{
	public partial class MainView : ContentPage
	{
		public MainView(MainViewModel viewModel)
		{
			InitializeComponent();
			BindingContext = viewModel;
		}
	}
}