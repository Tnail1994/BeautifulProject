using BeautifulFundamental.Core.Communication;
using BeautifulFundamental.Core.Extensions;
using BeautifulFundamental.Core.Messages.Authorize;
using BeautifulFundamental.Core.Services.CheckAlive;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace BeautifulClientApplication
{
	internal interface IClientManager : IHostedService;

	internal class ClientManager : IClientManager
	{
		private readonly IConnectionService _connectionService;
		private readonly ICommunicationService _communicationService;
		private CancellationToken _cancellationToken;

		private LoginRequest? _loginRequestObject;

		public ClientManager(IConnectionService connectionService, ICommunicationService communicationService)
		{
			_connectionService = connectionService;
			_communicationService = communicationService;
			_connectionService.ConnectionEstablished += OnConnectionEstablished;
			_connectionService.ConnectionLost += OnConnectionLost;
			_connectionService.Reconnected += OnReconnected;
		}

		private async void OnConnectionEstablished()
		{
			await Login();

			// Test registration
			var registrationReply = await _communicationService.SendAndReceiveAsync<RegistrationReply>(
				RegistrationRequest.Create("registrationName"));
			this.LogInfo($"Registration was successful: {registrationReply.RegistrationReplyValue?.Success}");
		}


		private async Task Login()
		{
			var maschineName = Environment.MachineName;
			await _communicationService.ReceiveAndSendAsync<DeviceIdentRequest>(
				new DeviceIdentReply
				{
					Ident = maschineName,
				});


			var loginReply = await TryLogin(LoginRequestType.DeviceIdent, maschineName, true);

			var username = "a";
			while (loginReply is { Success: false, CanRetry: true })
			{
				await Task.Delay(1000, _cancellationToken);
				loginReply = await TryLogin(LoginRequestType.Username, username, true);
			}

			if (loginReply is { Success: false })
			{
				await StopAsync(_cancellationToken);
				_connectionService.Stop(true);
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

		public Task StartAsync(CancellationToken cancellationToken)
		{
			try
			{
				_connectionService.Start();
				_cancellationToken = cancellationToken;
			}
			catch (CheckAliveException checkAliveException)
			{
				this.LogError($"CheckAliveService failed to start. {checkAliveException.Message}");
			}
			catch (Exception e)
			{
				this.LogFatal($"!!! Unexpected error while StartAsync event\n" +
				              $"Message: {e.Message}\n" +
				              $"Stacktrace: {e.StackTrace}\n");
			}

			return Task.CompletedTask;
		}

		private void OnConnectionLost(string obj)
		{
			Log.Debug($"On connection to server lost.");
		}

		private void OnReconnected()
		{
			this.LogDebug($"On reconnected to server.");
			StartAsync(_cancellationToken);
		}

		public Task StopAsync(CancellationToken cancellationToken)
		{
			_connectionService.ConnectionLost -= OnConnectionLost;
			_connectionService.Reconnected -= OnReconnected;
			return Task.CompletedTask;
		}
	}
}