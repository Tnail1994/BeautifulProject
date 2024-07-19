using DbManagement.Common.Contracts;
using Remote.Communication.Common.Contracts;
using Session.Common.Contracts;

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
			// Send login message to client and wait for reply

			// Then read the reply (Username)
			// Check it with the db entry and return true or false
			return true;
		}
	}
}