namespace BeautifulMauiClientApplication;

public partial class TestView : ContentPage
{
	public TestView(TestViewModel testViewModel)
	{
		InitializeComponent();
		BindingContext = testViewModel;
	}
}