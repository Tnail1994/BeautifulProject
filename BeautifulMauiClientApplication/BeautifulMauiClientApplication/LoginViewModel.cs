using CommunityToolkit.Mvvm.ComponentModel;
using Remote.Communication.Common.Contracts;
using SharedBeautifulData.Messages.Authorize;

namespace BeautifulMauiClientApplication
{
	public class LoginViewModel : ObservableObject
	{
		private TaskCompletionSource<bool>? _awaitLoginTcs;

		private readonly IConnectionService _connectionService;
		private readonly ICommunicationService _communicationService;

		private LoginRequest? _loginRequestObject;

		public LoginViewModel(IConnectionService connectionService, ICommunicationService communicationService)
		{
			_connectionService = connectionService;
			_communicationService = communicationService;
			_connectionService.ConnectionEstablished += OnConnectionEstablished;
			_connectionService.ConnectionLost += OnConnectionLost;
			_connectionService.Reconnected += OnReconnected;
			Start();
		}

		private void Start()
		{
			_connectionService.Start();
		}

		private async void OnConnectionEstablished()
		{
			var deviceName = DeviceInfo.Current.Name;
			await _communicationService.ReceiveAndSendAsync<DeviceIdentRequest>(
				new DeviceIdentReply
				{
					Ident = deviceName,
				});

			var loginReply = await TryLogin(LoginRequestType.DeviceIdent, deviceName, true);

			var username = "b";
			var attempts = 5;
			while (loginReply is { Success: false } && attempts > 0)
			{
				await Task.Delay(1);
				loginReply = await TryLogin(LoginRequestType.Username, username, true);
				attempts--;
			}

			_awaitLoginTcs?.SetResult(loginReply?.Success == true);
		}

		private async Task<LoginReply> TryLogin(LoginRequestType type, string value, bool stayActive)
		{
			if (_loginRequestObject?.RequestValue == null)
			{
				_loginRequestObject = new LoginRequest
				{
					RequestValue = new LoginRequestValue
					{
						Type = type,
						Value = value,
						StayActive = stayActive
					}
				};
			}
			else
			{
				_loginRequestObject.RequestValue.Type = type;
				_loginRequestObject.RequestValue.Value = value;
				_loginRequestObject.RequestValue.StayActive = stayActive;
			}

			var loginReply = await _communicationService.SendAndReceiveAsync<LoginReply>(_loginRequestObject);
			return loginReply;
		}

		private void OnReconnected()
		{
		}

		private void OnConnectionLost(string reason)
		{
		}

		public Task<bool> AwaitLogin()
		{
			_awaitLoginTcs = new TaskCompletionSource<bool>();
			return _awaitLoginTcs.Task;
		}
	}
}