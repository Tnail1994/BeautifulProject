using Core.Extensions;
using DbManagement.Common.Contracts;
using Remote.Communication.Common.Contracts;
using Session.Common.Contracts;
using SharedBeautifulData.Messages.Login;

namespace Session
{
	public class AuthenticationService : IAuthenticationService
	{
		private readonly IDbManager _dbManager;

		public AuthenticationService(IDbManager dbManager)
		{
			_dbManager = dbManager;
		}

		public async Task<bool> Authorize(ICommunicationService communicationService)
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
				return false;
			}

			// todo: Check it with the db entry and return true or false

			return true;
		}
	}
}