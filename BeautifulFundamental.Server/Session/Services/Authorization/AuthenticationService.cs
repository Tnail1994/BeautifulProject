using BeautifulFundamental.Core.Communication;
using BeautifulFundamental.Core.Communication.Implementations;
using BeautifulFundamental.Core.Extensions;
using BeautifulFundamental.Core.Messages.Authorize;
using BeautifulFundamental.Server.UserManagement;

namespace BeautifulFundamental.Server.Session.Services.Authorization
{
	public interface IAuthenticationService
	{
		Task<IAuthorizationInfo> Authorize(ICommunicationService communicationService);
		Task UnAuthorize(ICommunicationService communicationServicestring, string anyIdentifier);
	}

	public class AuthenticationService : IAuthenticationService
	{
		private readonly IUsersService _usersService;
		private readonly IAuthenticationSettings _settings;
		private static LoginReply? _loginReplyObj;

		public AuthenticationService(IUsersService usersService, IAuthenticationSettings settings)
		{
			_usersService = usersService;
			_settings = settings;
		}


		public async Task<IAuthorizationInfo> Authorize(ICommunicationService communicationService)
		{
			var deviceIdent =
				await communicationService.SendAndReceiveAsync<DeviceIdentReply>(new DeviceIdentRequest());

			if (!string.IsNullOrEmpty(deviceIdent.Ident) &&
			    _usersService.TryGetUserByDeviceIdent(deviceIdent.Ident, out var user) &&
			    user is { IsNotActive: true, StayActive: true } &&
			    CheckLastLoggedInDeviceIdent(user, deviceIdent) &&
			    user.ReactivateCounter <
			    _settings.MaxReactivateAuthenticationCounter)
			{
				// It is valid, that user with this device ident logs in
				var loginRequest = await ReceiveLoginRequest(communicationService);

				user.IsActive = true;
				user.ReactivateCounter++;
				user.StayActive = loginRequest.RequestValue?.StayActive == true;
				_usersService.SetUser(user);

				SendLoginReply(communicationService, true, true);

				return AuthorizationInfo.Create(user.Name);
			}

			return await Authorize(communicationService, 0, deviceIdent.Ident);
		}

		private static bool CheckLastLoggedInDeviceIdent(User user, DeviceIdentReply deviceIdent)
		{
			if (user.LastLoggedInDeviceIdent == null)
				return true;

			return user.LastLoggedInDeviceIdent.Equals(deviceIdent.Ident);
		}

		private static async Task<LoginRequest> ReceiveLoginRequest(ICommunicationService communicationService)
		{
			return await communicationService.ReceiveAsync<LoginRequest>();
		}

		private async Task<IAuthorizationInfo> Authorize(ICommunicationService communicationService, int attempts,
			string? deviceIdent)
		{
			var loginRequest = await ReceiveLoginRequest(communicationService);

			var requestValueType = loginRequest.RequestValue?.Type;
			var requestValueValue = loginRequest.RequestValue?.Value;

			var canRetry = attempts <= _settings.MaxAuthAttempts;

			if (requestValueType?.Equals(LoginRequestType.Username) == true &&
			    !string.IsNullOrEmpty(requestValueValue) &&
			    _usersService.TryGetUserByUsername(requestValueValue, out var user) && user is { IsNotActive: true })
			{
				user.IsActive = true;
				user.ReactivateCounter = 0;
				user.LastLoggedInDeviceIdent = deviceIdent;
				user.StayActive = loginRequest.RequestValue?.StayActive == true;
				_usersService.SetUser(user);
				SendLoginReply(communicationService, true, canRetry);
				return AuthorizationInfo.Create(user.Name);
			}

			SendLoginReply(communicationService, false, canRetry);

			if (canRetry)
				return await Authorize(communicationService, attempts + 1, deviceIdent);

			return AuthorizationInfo.Failed;
		}

		private static void SendLoginReply(ICommunicationService communicationService, bool loginSuccess, bool canRetry)
		{
			_loginReplyObj ??= new LoginReply
			{
				LoginResult = new LoginResult()
			};

			_loginReplyObj.Success = loginSuccess;
			_loginReplyObj.CanRetry = canRetry;

			communicationService.SendAsync(_loginReplyObj);
		}

		public async Task UnAuthorize(ICommunicationService communicationService, string anyIdentifier)
		{
			try
			{
				var logoutReply = await communicationService.SendAndReceiveAsync<LogoutReply>(new LogoutRequest());

				if (!logoutReply.IsOk)
				{
					this.LogWarning(
						$"Logging out fo (user or session id) {anyIdentifier} was not successful on client side...");
				}
			}
			catch (CommunicationServiceException communicationServiceException)
			{
				this.LogDebug(
					$"Communication exception code: {communicationServiceException.ErrorCode}; Message: {communicationServiceException.Message}");
			}
			catch (ObjectDisposedException objectDisposedException)
			{
				this.LogDebug($"CommunicationService already disposed. ExMsg: {objectDisposedException.Message}");
			}
			catch (OperationCanceledException)
			{
				this.LogDebug($"Stop receiving logout reply, if request was okay or not.");
			}
			catch (Exception exception)
			{
				this.LogError($"Unknown error inside AuthenticationService: {exception.Message}" +
				              $"\n Stacktrace: {exception.StackTrace}");
			}
			finally
			{
				if (!string.IsNullOrEmpty(anyIdentifier))
					_usersService.SetUsersActiveState(anyIdentifier, false);
			}
		}
	}
}