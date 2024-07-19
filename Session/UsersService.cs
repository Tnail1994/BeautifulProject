using DbManagement.Common.Contracts;
using Session.Common.Contracts;
using SharedBeautifulData.Objects;

namespace Session
{
	public class UsersService : IUsersService
	{
		private readonly IDbManager _dbManager;

		public UsersService(IDbManager dbManager)
		{
			_dbManager = dbManager;
		}

		public bool DoesUsernameExist(string username)
		{
			return _dbManager.GetEntities<User>()?.Any(user => user.Username == username) == true;
		}
	}
}