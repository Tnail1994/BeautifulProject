using BeautifulMauiClientApplication.Startup;
using Core.Extensions;
using Remote.Communication.Common.Contracts;
using SharedBeautifulData.Messages.Authorize;

namespace BeautifulMauiClientApplication.Login
{
	public interface ILoginService : IAutoStartService
	{
		Task<bool> AwaitLogin();
		Task<bool> Login(string username, bool rememberMe);
	}

	public class LoginService : ILoginService
	{
		private readonly IConnectionService _connectionService;
		private readonly ICommunicationService _communicationService;

		private readonly TaskCompletionSource<bool> _awaitLoginTcs = new();


		private LoginRequest? _loginRequestObject;

		public LoginService(IConnectionService connectionService,
			ICommunicationService communicationService)
		{
			_connectionService = connectionService;
			_communicationService = communicationService;
			_connectionService.ConnectionEstablished += OnConnectionEstablished;
			_connectionService.ConnectionLost += OnConnectionLost;
			_connectionService.Reconnected += OnReconnected;
		}


		public Task<bool> AwaitLogin() => _awaitLoginTcs.Task;

		public async Task<bool> Login(string username, bool rememberMe)
		{
			var loginReply = await TryLogin(LoginRequestType.Username, username, rememberMe);
			SetSuccessulLoginResult(loginReply, username);
			return loginReply.Success;
		}

		public Task<StartingResult> Start()
		{
			try
			{
				_connectionService.Start();
				return Result(true);
			}
			catch (Exception ex)
			{
				this.LogError($"Error starting LoginService: {ex.Message}\n" +
				              $"Stacktrace {ex.StackTrace}");
				return Result(false);
			}
		}

		private static Task<StartingResult> Result(bool result)
		{
			return Task.FromResult(StartingResult.Create(result, nameof(LoginService)));
		}


		private async void OnConnectionEstablished()
		{
			var deviceName = DeviceInfo.Current.Name;
			deviceName = "dummy3";
			await _communicationService.ReceiveAndSendAsync<DeviceIdentRequest>(
				new DeviceIdentReply
				{
					Ident = deviceName,
				});

			var loginReply = await TryLogin(LoginRequestType.DeviceIdent, deviceName, true);

			var username = "gsdfgsdf";
			var attempts = 1;
			while (loginReply is { Success: false } && attempts > 0)
			{
				await Task.Delay(1);
				loginReply = await TryLogin(LoginRequestType.Username, username, true);
				attempts--;
			}

			SetSuccessulLoginResult(loginReply, username);
		}

		private void SetSuccessulLoginResult(LoginReply loginReply, string username)
		{
			if (loginReply is { Success: true })
			{
				this.LogDebug($"Auto login fo {username} successful");
				_awaitLoginTcs?.SetResult(true);
			}
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
	}
}