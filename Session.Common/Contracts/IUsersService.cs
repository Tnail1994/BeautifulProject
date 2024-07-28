namespace Session.Common.Contracts
{
	public interface IUsersService
	{
		bool DoesUsernameExist(string username);
		bool IsUsernameActive(string username);
		void SetUsersActiveState(string username, bool isActive);
	}
}