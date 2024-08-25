namespace BeautifulFundamental.Server.UserManagement
{
	public interface IUsersService
	{
		bool TryGetUserByDeviceIdent(string deviceIdent, out User? user);
		bool TryGetUserByUsername(string username, out User? user);
		void SetUser(User user);
		void SetUsersActiveState(string username, bool isActive);
	}
}