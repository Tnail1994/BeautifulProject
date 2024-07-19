using DbManagement.Common.Contracts;
using DbManagement.Contexts;
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
			var entities = _dbManager.GetEntities<UserDto>()?.Select(Map);
			return entities != null && entities.Any(user => user.Name == username);
		}

		//private UserDto Map(User user)
		//{
		//	return new UserDto(user.Name);
		//}

		private User Map(UserDto userDto)
		{
			return User.Create(userDto.Name);
		}
	}
}