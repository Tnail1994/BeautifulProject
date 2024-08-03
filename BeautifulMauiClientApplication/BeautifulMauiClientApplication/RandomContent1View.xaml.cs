namespace BeautifulMauiClientApplication
{
	public partial class RandomContent1View : ContentView
	{
		public RandomContent1View()
		{
			InitializeComponent();
			BindingContext =
				Application.Current?.MainPage?.Handler?.MauiContext?.Services.GetService<RandomContent1ViewModel>();
		}
	}
}