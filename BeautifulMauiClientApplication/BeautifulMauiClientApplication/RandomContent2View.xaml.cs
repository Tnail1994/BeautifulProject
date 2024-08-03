namespace BeautifulMauiClientApplication
{
	public partial class RandomContent2View : ContentView
	{
		public RandomContent2View()
		{
			InitializeComponent();
			BindingContext =
				Application.Current?.MainPage?.Handler?.MauiContext?.Services.GetService<RandomContent2ViewModel>();
		}
	}
}