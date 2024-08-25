namespace BeautifulFundamental.Server.Session.Contracts.Services.Authorization
{
	public interface IAuthorizationInfo
	{
		bool IsAuthorized { get; }
		string Username { get; }
	}
}