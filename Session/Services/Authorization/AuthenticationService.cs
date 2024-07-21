using Core.Extensions;
using Remote.Communication.Common.Contracts;
using Session.Common.Contracts;
using SharedBeautifulData.Messages.Login;

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

			if (!_usersService.DoesUsernameExist(username))
			{
				this.LogWarning($"User with name {username} does not exist.", "server");

				if (attempts < _settings.MaxAuthAttempts)
					return await Authorize(communicationService, attempts + 1);

				return AuthorizationInfo.Failed;
			}

			return AuthorizationInfo.Create(username);
		}
	}
}