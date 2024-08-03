using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BeautifulMauiClientApplication
{
	public partial class MainViewModel : ObservableObject
	{
		private readonly IDataService _dataService;

		[ObservableProperty] private ObservableCollection<ItemObj> _items;

		[ObservableProperty] [NotifyPropertyChangedFor(nameof(IsNotBusy))]
		private bool _isBusy;

		public bool IsNotBusy => !IsBusy;

		public MainViewModel(IDataService dataService)
		{
			_dataService = dataService;
			Items = new ObservableCollection<ItemObj>();
		}

		[RelayCommand]
		private async Task LoadItemsAsync()
		{
			if (IsBusy)
				return;

			try
			{
				IsBusy = true;
				var items = await _dataService.GetItemsAsync();
				Items.Clear();
				foreach (var item in items)
				{
					Items.Add(item);
				}
			}
			finally
			{
				IsBusy = false;
			}
		}
	}

	public interface IDataService
	{
		Task<List<ItemObj>> GetItemsAsync();
	}

	public class DataService : IDataService
	{
		public async Task<List<ItemObj>> GetItemsAsync()
		{
			await Task.Delay(1000);
			return new List<ItemObj>
			{
				new ItemObj { Id = 1, Name = "ItemObj 1" },
				new ItemObj { Id = 2, Name = "ItemObj 2" },
				new ItemObj { Id = 3, Name = "ItemObj 3" }
			};
		}
	}

	public class ItemObj
	{
		public int Id { get; set; }
		public string Name { get; set; }
	}
}