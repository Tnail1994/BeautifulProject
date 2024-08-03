namespace BeautifulMauiClientApplication
{
	public partial class RandomContent3View : ContentView
	{
		public RandomContent3View()
		{
			InitializeComponent();
			BindingContext =
				Application.Current?.MainPage?.Handler?.MauiContext?.Services.GetService<RandomContent3ViewModel>();
		}
	}
}