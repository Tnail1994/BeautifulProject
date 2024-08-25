using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using BeautifulFundamental.Core.Communication.Implementations;
using BeautifulFundamental.Core.MessageHandling;
using BeautifulFundamental.Core.Messages.CheckAlive;
using BeautifulFundamental.Core.Messages.RandomTestData;

namespace BeautifulMauiClientApplication.Example
{
	public partial class RandomContent3ViewModel : ObservableObject, IDisposable
	{
		private readonly IAutoSynchronizedMessageHandler _autoSynchronizedMessageHandler;
		private readonly string _subscribeId;

		[ObservableProperty] private string _text = string.Empty;

		private int _counter = 0;

		public RandomContent3ViewModel(IAutoSynchronizedMessageHandler autoSynchronizedMessageHandler)
		{
			_autoSynchronizedMessageHandler = autoSynchronizedMessageHandler;
			_subscribeId = _autoSynchronizedMessageHandler.Subscribe<RandomDataRequest>(OnRandomDataMessageRequest);
		}

		private INetworkMessage? OnRandomDataMessageRequest(INetworkMessage requestMessage)
		{
			if (requestMessage is RandomDataRequest randomDataRequest)
			{
				Text = randomDataRequest.MessageObject ?? String.Empty;
			}

			return null;
		}

		public void Dispose()
		{
			_autoSynchronizedMessageHandler.Unsubscribe(_subscribeId);
		}
	}

	public partial class RandomContent2ViewModel : ObservableObject, IDisposable
	{
		private readonly IAutoSynchronizedMessageHandler _autoSynchronizedMessageHandler;
		private readonly string _subscribeId;

		[ObservableProperty] private string _text = string.Empty;

		private int _counter;

		public RandomContent2ViewModel(IAutoSynchronizedMessageHandler autoSynchronizedMessageHandler)
		{
			_autoSynchronizedMessageHandler = autoSynchronizedMessageHandler;
			_subscribeId = _autoSynchronizedMessageHandler.Subscribe<CheckAliveRequest>(OnCheckAliveMessageRequest);

			SetText();
		}

		private void SetText()
		{
			Text = $"Check alive request counter: {_counter}";
		}

		private INetworkMessage? OnCheckAliveMessageRequest(INetworkMessage requestMessage)
		{
			if (requestMessage is CheckAliveRequest)
			{
				_counter++;
				SetText();
			}

			return null;
		}

		public void Dispose()
		{
			_autoSynchronizedMessageHandler.Unsubscribe(_subscribeId);
		}
	}

	public partial class RandomContent1ViewModel : ObservableObject
	{
		private readonly IAutoSynchronizedMessageHandler _autoSynchronizedMessageHandler;
		private readonly string _subscribeId;

		[ObservableProperty] private string _text = string.Empty;

		private int _counter = 0;

		public RandomContent1ViewModel(IAutoSynchronizedMessageHandler autoSynchronizedMessageHandler)
		{
			_autoSynchronizedMessageHandler = autoSynchronizedMessageHandler;
			_subscribeId = _autoSynchronizedMessageHandler.Subscribe<CheckAliveReply>(OnCheckAliveMessageReply);

			SetText();
		}

		private void SetText()
		{
			Text = $"Check alive reply counter: {_counter}";
		}

		private INetworkMessage? OnCheckAliveMessageReply(INetworkMessage requestMessage)
		{
			if (requestMessage is CheckAliveReply)
			{
				_counter++;
				SetText();
			}

			return null;
		}

		public void Dispose()
		{
			_autoSynchronizedMessageHandler.Unsubscribe(_subscribeId);
		}
	}

	public class TestViewModel : ObservableObject
	{
	}

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