using SharedBeautifulData.Objects;

namespace Session.Common.Contracts
{
	public interface IUsersService
	{
		bool TryGetUserByDeviceIdent(string deviceIdent, out User? user);
		bool TryGetUserByUsername(string username, out User? user);
		void SetUser(User user);
		void SetUsersActiveState(string username, bool isActive);
	}
}