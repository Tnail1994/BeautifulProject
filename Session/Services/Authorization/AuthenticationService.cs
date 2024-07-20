using Core.Extensions;
using Remote.Communication.Common.Contracts;
using Session.Common.Contracts;
using SharedBeautifulData.Messages.Login;

namespace Session.Services.Authorization
{
	public class AuthenticationService : IAuthenticationService
	{
		private readonly IUsersService _usersService;

		public AuthenticationService(IUsersService usersService)
		{
			_usersService = usersService;
		}

		public async Task<IAuthorizationInfo> Authorize(ICommunicationService communicationService)
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
				return AuthorizationInfo.Failed;
			}

			if (!_usersService.DoesUsernameExist(username))
			{
				this.LogWarning($"User with name {username} does not exist.", "server");
				return AuthorizationInfo.Failed;
			}

			return AuthorizationInfo.Create(username);
		}
	}
}