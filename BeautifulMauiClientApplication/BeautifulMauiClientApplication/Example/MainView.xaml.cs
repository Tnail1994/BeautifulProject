﻿namespace BeautifulMauiClientApplication.Example
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