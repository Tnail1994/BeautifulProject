using Core.Extensions;
using DbManagement.Common.Contracts;
using DbManagement.Contexts;
using Session.Common.Contracts;

namespace Session.Services
{
	public class UsersService : IUsersService, IDisposable
	{
		private readonly IDbManager _dbManager;

		public UsersService(IDbManager dbManager)
		{
			_dbManager = dbManager;
		}

		public bool DoesUsernameExist(string username)
		{
			return FindUser(username) != null;
		}

		//private IEnumerable<User>? GetUsers()
		//{
		//	return GetEntities()?.Select(Map);
		//}

		private IEnumerable<UserDto>? GetEntities()
		{
			return _dbManager.GetEntities<UserDto>();
		}

		public bool IsUsernameActive(string username)
		{
			var foundUser = FindUser(username);

			if (foundUser == null)
				return false;

			if (foundUser.IsActive)
				this.LogDebug($"Username {username} is already active");

			return foundUser.IsActive;
		}

		private UserDto? FindUser(string username)
		{
			var entities = _dbManager.GetEntities<UserDto>();
			var foundUser = entities?.FirstOrDefault(user => user.Name == username);
			return foundUser;
		}

		public void SetUsersActiveState(string username, bool isActive)
		{
			var foundUser = FindUser(username);

			if (foundUser == null)
			{
				this.LogError($"Did not find {username}. Cannot set users state.");
				return;
			}

			foundUser.IsActive = isActive;
			_dbManager.SaveChanges(foundUser);
		}

		//private UserDto Map(User user)
		//{
		//	return new UserDto(user.Name);
		//}

		//private User Map(UserDto userDto)
		//{
		//	return User.Create(userDto.Name, userDto.IsActive);
		//}

		public void Dispose()
		{
			var entities = GetEntities()?.Where(entity => entity.IsActive);

			if (entities == null)
			{
				this.LogError("Cannot dispose entities and set users state, because there are no entities.");
				return;
			}

			foreach (var user in entities)
			{
				user.IsActive = false;
			}
		}
	}
}