namespace Session.Common.Contracts
{
	public interface IAuthorizationInfo
	{
		bool IsAuthorized { get; }
		string Username { get; }
	}
}