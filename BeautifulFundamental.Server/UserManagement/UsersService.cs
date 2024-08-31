using BeautifulFundamental.Core.Extensions;
using BeautifulFundamental.Server.Db;

namespace BeautifulFundamental.Server.UserManagement
{
	public interface IUsersService
	{
		bool TryGetUserByDeviceIdent(string deviceIdent, out User? user);
		bool TryGetUserByUsername(string username, out User? user);
		void SetUser(User user);
		void SetUsersActiveState(string username, bool isActive);
	}

	public class UsersService : IUsersService, IDisposable
	{
		private readonly IDbManager _dbManager;

		public UsersService(IDbManager dbManager)
		{
			_dbManager = dbManager;
		}

		public bool DoesUsernameExist(string username)
		{
			return FindUserByName(username) != null;
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
			var foundUser = FindUserByName(username);

			if (foundUser == null)
				return false;

			if (foundUser.IsActive)
				this.LogDebug($"Username {username} is already active");

			return foundUser.IsActive;
		}

		private UserDto? FindUserByName(string username)
		{
			var entities = _dbManager.GetEntities<UserDto>();
			var foundUser = entities?.FirstOrDefault(user => user.Name == username);
			return foundUser;
		}

		private UserDto? FindUserByDeviceIdent(string deviceIdent)
		{
			var entities = _dbManager.GetEntities<UserDto>();
			var foundUser = entities?.FirstOrDefault(user => user.LastLoggedInDeviceIdent == deviceIdent);
			return foundUser;
		}

		public void SetUsersActiveState(string username, bool isActive)
		{
			var foundUser = FindUserByName(username);

			if (foundUser == null)
			{
				this.LogError($"Did not find {username}. Cannot set users state.");
				return;
			}

			foundUser.IsActive = isActive;
			_dbManager.SaveChanges(foundUser);
		}

		public bool TryGetUserByDeviceIdent(string deviceIdent, out User? user)
		{
			var foundUserDto = FindUserByDeviceIdent(deviceIdent);
			return ReturnUser(out user, foundUserDto);
		}

		public bool TryGetUserByUsername(string username, out User? user)
		{
			var foundUserDto = FindUserByName(username);
			return ReturnUser(out user, foundUserDto);
		}

		public void SetUser(User user)
		{
			var foundUserDto = FindUserByName(user.Name);

			if (foundUserDto == null)
			{
				this.LogError($"Cannot set User data for {user.Name}, because not found.");
				return;
			}

			foundUserDto.ReactivateCounter = user.ReactivateCounter;
			foundUserDto.StayActive = user.StayActive;
			foundUserDto.LastLoggedInDeviceIdent = user.LastLoggedInDeviceIdent;
			foundUserDto.IsActive = user.IsActive;
			_dbManager.SaveChanges(foundUserDto);
		}

		private bool ReturnUser(out User? user, UserDto? foundUserDto)
		{
			user = foundUserDto != null ? Map(foundUserDto) : null;
			return user != null;
		}

		//private UserDto Map(User user)
		//{
		//	return new UserDto(user.Name);
		//}

		private User Map(UserDto userDto)
		{
			return User.Create(userDto.Name, userDto.IsActive, userDto.StayActive, userDto.LastLoggedInDeviceIdent,
				userDto.ReactivateCounter);
		}

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