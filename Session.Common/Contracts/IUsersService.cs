namespace Session.Common.Contracts
{
	public interface IUsersService
	{
		bool DoesUsernameExist(string username);
	}
}