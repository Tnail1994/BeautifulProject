using Core.Extensions;
using Remote.Communication.Common.Contracts;
using Remote.Communication.Common.Implementations;
using Session.Common.Contracts;
using SharedBeautifulData.Messages.Authorize;

namespace Session.Services.Authorization
{
	public class AuthenticationService : IAuthenticationService
	{
		private readonly IUsersService _usersService;
		private readonly IAuthenticationSettings _settings;

		public AuthenticationService(IUsersService usersService, IAuthenticationSettings settings)
		{
			_usersService = usersService;
			_settings = settings;
		}

		public async Task<IAuthorizationInfo> Authorize(ICommunicationService communicationService)
		{
			return await Authorize(communicationService, 0);
		}

		private async Task<IAuthorizationInfo> Authorize(ICommunicationService communicationService, int attempts)
		{
			// Send login request message to client and wait for reply
			var loginReply =
				await communicationService.SendAndReceiveAsync<LoginReply>(new LoginRequest
				{
					Type = LoginRequestType.Username
				});

			var username = loginReply.Token ?? string.Empty;


			if (string.IsNullOrEmpty(username))
			{
				this.LogWarning("Cannot authorize user with empty name.", "server");

				if (attempts < _settings.MaxAuthAttempts)
					return await Authorize(communicationService, attempts + 1);

				return AuthorizationInfo.Failed;
			}

			if (!_usersService.DoesUsernameExist(username) || _usersService.IsUsernameActive(username))
			{
				this.LogWarning($"User with name {username} does not exist or is already active.", "server");

				if (attempts < _settings.MaxAuthAttempts)
					return await Authorize(communicationService, attempts + 1);

				return AuthorizationInfo.Failed;
			}

			_usersService.SetUsersActiveState(username, true);

			return AuthorizationInfo.Create(username);
		}

		public async Task UnAuthorize(ICommunicationService communicationService, string username)
		{
			try
			{
				var logoutReply = await communicationService.SendAndReceiveAsync<LogoutReply>(new LogoutRequest());

				if (!logoutReply.IsOk)
				{
					this.LogWarning($"Logging out fo user {username} was not successful on client side...");
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
			catch (Exception exception)
			{
				this.LogError($"Unknown error inside AuthenticationService: {exception.Message}" +
				              $"\n Stacktrace: {exception.StackTrace}");
			}
			finally
			{
				_usersService.SetUsersActiveState(username, false);
			}
		}
	}
}